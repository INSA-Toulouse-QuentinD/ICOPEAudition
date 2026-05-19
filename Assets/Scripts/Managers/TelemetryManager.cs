using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Managers
{
    public class TelemetryManager : MonoBehaviour
    {
        [SerializeField] private string serverURL = "https://icope.rodriguez-vincent.fr/";
        [SerializeField] private string datasManagerPHP = "datas_manager.php";
        [SerializeField] private string getDatasPHP = "get_datas.php";
        [SerializeField] private string uuidPHP = "uuid.php";

        XmlDocument xmlDocument = null;
        private string uuid;
        [SerializeField] private int nbWins = 0, nbGames = 0;
        private List<int> nbShowSteps = new() { 0, 0, 0, 0, 0 };
        private List<int> nbLosesStepsDiag = new() { 0, 0, 0, 0, 0 };
        private List<int> nbLosesStepsAction = new() { 0, 0, 0, 0, 0 };
        [SerializeField] private float gameTime, lastSessionTime;
        [SerializeField] private List<string> answers = new() { "", "", "", "" };

        internal void IncrGames() => nbGames++;
        internal void IncrWins() => nbWins++;
        internal void IncrNbShowSteps(int i) => nbShowSteps[i % nbShowSteps.Count]++;
        internal void IncrNbLosesStepsDiag(int i) => nbLosesStepsDiag[i % nbLosesStepsDiag.Count]++;
        internal void IncrNbLosesStepsAction(int i) => nbLosesStepsAction[i % nbLosesStepsAction.Count]++;
        internal int GetNbGames() => nbGames;

        IEnumerator Start()
        {
            uuid = PlayerPrefs.GetString("UUID", string.Empty);
            if (string.IsNullOrEmpty(uuid))
            {
                using (UnityWebRequest webRequest = UnityWebRequest.Get($"{serverURL}{uuidPHP}"))
                {
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        uuid = webRequest.downloadHandler.text;
                        PlayerPrefs.SetString("UUID", uuid);
                        PlayerPrefs.Save();
                    }
                    else
                    {
                        Debug.LogError("Erreur lors de la récupération de l'UUID : " + webRequest.error);
                    }
                }
            }

            using (UnityWebRequest www = UnityWebRequest.Get($"{serverURL}{getDatasPHP}?uuid={uuid}"))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    InitVariables();
                }
                else
                {
                    string response = www.downloadHandler.text;
                    string[] variables = response.Split(';');
                    variables = variables.Select(v => v.Replace("_*_POINT_COMMA_*_", ";")).ToArray();

                    InitVariables();

                    if (variables.Length >= 15)
                    {
                        if (int.TryParse(variables[1].Trim(), out nbGames) &&
                            int.TryParse(variables[2].Trim(), out nbWins) &&
                            float.TryParse(variables[14].Trim(), out lastSessionTime))
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                if (variables.Length > 4 + i && variables.Length > 9 + i &&
                                    int.TryParse(variables[4 + i].Trim(), out int diag) &&
                                    int.TryParse(variables[9 + i].Trim(), out int action))
                                {
                                    nbLosesStepsDiag[i] = diag;
                                    nbLosesStepsAction[i] = action;
                                }
                            }
                            answers = new() { "", "", "", "" };
                            if (variables[15].Length >= 2) answers[0] = variables[15].Substring(1, variables[15].Length - 2);
                            else answers[0] = "";
                            if (variables[16].Length >= 2) answers[1] = variables[16].Substring(1, variables[16].Length - 2);
                            else answers[1] = "";
                            if (variables[17].Length >= 2) answers[2] = variables[17].Substring(1, variables[17].Length - 2);
                            else answers[2] = "";
                            if (variables[18].Length >= 2) answers[3] = variables[18].Substring(1, variables[18].Length - 2);
                            else answers[3] = "";
                        }
                    }
                }
            }

            gameTime = Time.realtimeSinceStartup;
            StartCoroutine(AutoSave());
        }

        private void InitVariables()
        {
            lastSessionTime = nbGames = nbWins = 0;
            for (int i = 0; i < 5; i++)
            {
                nbLosesStepsDiag[i] = nbLosesStepsAction[i] = 0;
            }
            answers = new() { "", "", "", "" };
        }

        private IEnumerator AutoSave()
        {
            while (true)
            {
                yield return new WaitForSeconds(5f);
                SaveDatas();
            }
        }

        public void SaveDatas()
        {
            if (nbGames > 0)
            {
                float timeElapsed = Time.realtimeSinceStartup - gameTime + lastSessionTime;
                /*
                 * Nombre de patients traités
                 * Nombre de patients traités réussis
                 * Pourcentage réussite
                 * Nombre erreurs étape 1
                 * Nombre erreurs étape 2
                 * Nombre erreurs étape 3
                 * Nombre erreurs étape 4
                 * Nombre erreurs étape 5
                 * Temps passé sur le jeu
                 * Questionnaire de satisfaction (Q1)
                 * Questionnaire de satisfaction (Q2)
                 * Questionnaire de satisfaction (Q3)
                 * Questionnaire de satisfaction (Q4)
                 */
                object[] dataList =
                {
                nbGames,
                nbWins,
                (double) nbWins/nbGames * 100,
                nbLosesStepsDiag[0],
                nbLosesStepsAction[0],
                nbLosesStepsDiag[1],
                nbLosesStepsAction[1],
                nbLosesStepsDiag[2],
                nbLosesStepsAction[2],
                nbLosesStepsDiag[3],
                nbLosesStepsAction[3],
                nbLosesStepsDiag[4],
                nbLosesStepsAction[4],
                timeElapsed,
                answers[0],
                answers[1],
                answers[2],
                answers[3],
            };
                xmlDocument = ConvertToXML(dataList);
                SaveToServer();
            }
        }

        public XmlDocument ConvertToXML(object[] dataList)
        {
            return ConvertToXML(new List<object>(dataList));
        }

        public XmlDocument ConvertToXML(List<object> dataList)
        {
            StringBuilder xmlBuilder = new();

            xmlBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            xmlBuilder.AppendLine("<Root>");
            xmlBuilder.AppendLine($"\t<Data uuid=\"{uuid}\">");

            foreach (object data in dataList)
            {
                xmlBuilder.AppendLine($"\t\t<Item Type=\"{data.GetType()}\">");

                string dataString = data switch
                {
                    int intValue => intValue.ToString(),
                    float floatValue => floatValue.ToString(),
                    double doubleValue => doubleValue.ToString(),
                    string stringValue => stringValue,
                    bool boolValue => boolValue.ToString(),
                    DateTime dateTimeValue => dateTimeValue.ToString("o"),
                    _ => null // Gérer les autres types d'objets
                };

                if (dataString != null)
                {
                    xmlBuilder.AppendLine($"\t\t\t{dataString}");
                }

                xmlBuilder.AppendLine("\t\t</Item>");
            }

            xmlBuilder.AppendLine("\t</Data>");
            xmlBuilder.AppendLine("</Root>");

            XmlDocument xmlDoc = new();
            xmlDoc.LoadXml(xmlBuilder.ToString());
            return xmlDoc;
        }

        public void SaveToServer()
        {
            if (xmlDocument == null) return;
            StartCoroutine(UploadXML(xmlDocument));
        }

        private IEnumerator UploadXML(XmlDocument xmlDocument)
        {
            string xmlString = xmlDocument.OuterXml;

            using UnityWebRequest request = new($"{serverURL}{datasManagerPHP}", "POST");
            byte[] xmlData = Encoding.UTF8.GetBytes(xmlString);
            request.uploadHandler = new UploadHandlerRaw(xmlData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/xml");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error sending XML file to server: " + request.error);
            }
            else
            {
                Debug.Log("XML file successfully sent to server.");
                Debug.Log("Server response : " + request.downloadHandler.text);
            }
        }
    }

    public static class JsonHelper
    {
        public static T[] GetJsonArray<T>(string json)
        {
            string newJson = "{\"array\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }
}