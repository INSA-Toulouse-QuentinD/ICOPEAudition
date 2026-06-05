using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Managers;
using PatientData.AlgoData;
using UnityEngine.Networking;

namespace PatientData{
    /// <summary>
    /// Utility to load patient data described as JSON files at runtime and bind them into the
    /// existing runtime LevelsData structure. Designed to work in both Editor (local file I/O)
    /// and WebGL (HTTP downloads from web server).
    /// </summary>
    public static class PatientDataJsonLoader{
        [Serializable]
        public class PatientDto{
            public string spriteUp;
            public string spriteSit;
            public string spriteWeber;
            public bool isOnPhone;
            public string lastName;
            public string firstName;
            public int age;

            public string familySituation;
            public string occupationalActivities;
            public string context;
            public string medicalHistory;
            public string descriptionLevel;

            public List<AlgoStepDto> steps;
        }

        [Serializable]
        public class AlgoStepDto{
            public string type;
            public string dialoguePatient;
            public string spriteEarExams;
            public string overrideDiagnosticQuestion;

            public List<AnswerDto> diagnosticPhase;
            public List<AnswerDto> actionPhase;

            public List<string> predefinedAnswer;
        }

        [Serializable]
        public class AnswerDto{
            public string answerText;
            public bool isCorrect;
            public string correctionText;
            public string rappelTip;
            public string spriteJustification1;
            public string spriteJustification2;
        }

        public static IEnumerator LoadLevelData(Action<LevelsData> onDone){
            LevelsData result = ScriptableObject.CreateInstance<LevelsData>();

            string manifestJson = null;

            yield return LoadJson("Levels/manifest.json", json => { manifestJson = json; });

            if (manifestJson == null) {
                onDone?.Invoke(null);
                yield break;
            }

            Manifest manifest = JsonUtility.FromJson<Manifest>(manifestJson);

            foreach (LevelManifest levelManifest in manifest.levels) {
                PatientCaseLevel level = new PatientCaseLevel{
                    name = ExtractName(levelManifest.name)
                };

                List<string> sortedPatients = SortByIndex(levelManifest.patients);

                foreach (string patientName in sortedPatients) {
                    string relativePath = $"Levels/{levelManifest.name}/{patientName}/{ExtractName(patientName)}.json";

                    string patientJson = null;
                    yield return LoadJson(relativePath, json => { patientJson = json; });
                    if (patientJson == null) continue;

                    PatientData data = null;
                    yield return ConvertToPatientData(patientJson, patientData => data = patientData);
                    level.patientsCase.Add(data);
                }

                result.patientByLevel.Add(level);
            }

            onDone?.Invoke(result);
        }

        private static IEnumerator ConvertToPatientData(string json, Action<PatientData> onDone){
            PatientDto dto = JsonUtility.FromJson<PatientDto>(json);
            PatientData data = ScriptableObject.CreateInstance<PatientData>();

            Sprite up = null; //DEBUG!!! Devinez le chemin plutôt que mettre 3 variables (toujour nommé {ExtractName(patientName)}_up /_sit /_weber)
            Sprite sit = null;
            Sprite weber = null;
            yield return LoadSprite(dto.spriteUp, s => up = s);
            yield return LoadSprite(dto.spriteSit, s => sit = s);
            yield return LoadSprite(dto.spriteWeber, s => weber = s);

            List<Sprite> sprites = new();
            if (up) sprites.Add(up); //DEBUG!!! obligatoire !!! (si !isOnPhone)
            if (sit) sprites.Add(sit); //DEBUG!!! obligatoire !!! (si !isOnPhone)
            if (weber) sprites.Add(weber); //DEBUG!!! obligatoire si Weber !!!
            data.characterSprites = sprites.ToArray();

            data.isOnPhone = dto.isOnPhone;
            data.lastName = dto.lastName;
            data.firstName = dto.firstName;
            data.age = dto.age;

            data.familySituation = dto.familySituation;
            data.occupationalActivities = dto.occupationalActivities;
            data.context = dto.context;

            data.medicalHistory = dto.medicalHistory;
            data.descriptionLevel = dto.descriptionLevel;

            List<AlgoStep> steps = null;
            yield return ConvertSteps(dto.steps, result => steps = result);
            data.steps = steps;

            onDone?.Invoke(data);
        }

        private static IEnumerator ConvertSteps(List<AlgoStepDto> dtos, Action<List<AlgoStep>> onDone){
            List<AlgoStep> result = new List<AlgoStep>();

            if (dtos == null) {
                onDone?.Invoke(result);
                yield break;
            }

            foreach (AlgoStepDto dto in dtos) {
                AlgoStep step = new AlgoStep();

                Enum.TryParse(dto.type, out Step stepType);
                step.type = stepType;

                step.dialoguePatient = dto.dialoguePatient;
                step.overrideDiagnosticQuestion = dto.overrideDiagnosticQuestion;

                // spriteEarExams (LOAD)
                if (!string.IsNullOrEmpty(dto.spriteEarExams)) {
                    Sprite ear = null;
                    yield return LoadSprite(dto.spriteEarExams, s => ear = s);
                    step.spriteEarExams = ear;
                }

                step.predefinedAnswer = ConvertYesNo(dto.predefinedAnswer);

                List<AnswerData> diag = null;
                yield return ConvertAnswers(dto.diagnosticPhase, r => diag = r);
                step.diagnosticPhase = diag;

                List<AnswerData> act = null;
                yield return ConvertAnswers(dto.actionPhase, r => act = r);
                step.actionPhase = act;

                result.Add(step);
            }

            onDone?.Invoke(result);
        }

        private static List<YesNo> ConvertYesNo(List<string> input){
            List<YesNo> result = new List<YesNo>();

            if (input == null)
                return result;

            foreach (string s in input) {
                Enum.TryParse(s, out YesNo value);
                result.Add(value);
            }

            return result;
        }

        private static IEnumerator ConvertAnswers(
            List<AnswerDto> dtos,
            Action<List<AnswerData>> onDone){
            List<AnswerData> result = new List<AnswerData>();

            if (dtos == null) {
                onDone?.Invoke(result);
                yield break;
            }

            foreach (AnswerDto dto in dtos) {
                Enum.TryParse(dto.rappelTip, out RappelTip tip);

                AnswerData a = new AnswerData{
                    answerText = dto.answerText,
                    isCorrect = dto.isCorrect,
                    correctionText = dto.correctionText,
                    rappelTip = tip
                };

                if (!string.IsNullOrEmpty(dto.spriteJustification1)) {
                    Sprite s1 = null;
                    yield return LoadSprite(dto.spriteJustification1, x => s1 = x);

                    if (s1) a.sprites.Add(s1);
                }

                if (!string.IsNullOrEmpty(dto.spriteJustification2)) {
                    Sprite s2 = null;
                    yield return LoadSprite(dto.spriteJustification2, x => s2 = x);

                    if (s2) a.sprites.Add(s2);
                }

                result.Add(a);
            }

            onDone?.Invoke(result);
        }

        #region Littles Functions

        private static string ExtractName(string raw){
            int index = raw.LastIndexOf('_');
            return index >= 0 ? raw.Substring(index + 1) : raw;
        }

        private static List<string> SortByIndex(List<string> input){
            input.Sort((a, b) => {
                int ia = GetIndex(a);
                int ib = GetIndex(b);
                return ia.CompareTo(ib);
            });

            return input;
        }

        private static int GetIndex(string name){
            int i = name.IndexOf('_');
            if (i <= 0) return 0;

            string number = name.Substring(0, i);

            int.TryParse(number, out int result);
            return result;
        }

        #endregion

        #region Files Loader

        public static IEnumerator LoadJson(string relativePath, Action<string> callback){
            string path = Path.Combine(Application.streamingAssetsPath, relativePath);

#if UNITY_WEBGL && !UNITY_EDITOR
            UnityWebRequest request = UnityWebRequest.Get(path);
#else
            UnityWebRequest request = UnityWebRequest.Get("file:///" + path);
#endif

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success) {
                Debug.LogError(request.error);
                callback?.Invoke(null);
                yield break;
            }

            callback?.Invoke(request.downloadHandler.text);
        }

        public static IEnumerator LoadSprite(string relativePath, Action<Sprite> callback){
            string path = Path.Combine(Application.streamingAssetsPath, relativePath, "Images");

#if UNITY_WEBGL && !UNITY_EDITOR
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(path);
#else
            UnityWebRequest request = UnityWebRequestTexture.GetTexture("file:///" + path);
#endif

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success) {
                Debug.LogError(request.error);
                callback?.Invoke(null);
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            callback?.Invoke(sprite);
        }

        #endregion
    }
}