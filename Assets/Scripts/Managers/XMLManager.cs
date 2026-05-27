using UI.TutorialContents;
using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using UnityEngine;

namespace Managers{
    /// <summary>
    /// XML Manager handles loading, saving, and validating game data using XML files.
    /// It supports deserialization of complex nested data structures (including dictionaries, lists, and custom structs),
    /// recursive serialization of C# objects into XML, and validation against XSD schemas to ensure data integrity.
    /// This utility facilitates persistent storage and retrieval of game progress and settings.
    /// </summary>
    public static class XmlManager{
        // Cache simple pour éviter de re-parser le même TextAsset à chaque appel.
        // Note: si Unity recharge l'asset / change son contenu, son instanceId changera, donc le cache restera cohérent.
        private static int _cachedTutorialTextAssetInstanceId;
        private static XmlDocument _cachedTutorialDoc;

        /// <summary>
        /// Loads a tutorial entry from an XML TextAsset based on the given step name and entry ID.
        /// </summary>
        /// <param name="path">The TextAsset containing the tutorial XML data.</param>
        /// <param name="stepName">The name attribute of the Step node to search for.</param>
        /// <param name="id">The ID of the tutorial entry to load.</param>
        /// <returns>
        /// A TutorialEntry object with the data for the specified step and ID, or null if not found.
        /// </returns>
        /// <exception cref="Exception">Thrown if the XML root element is not "Tutorial".</exception>
        public static TutorialEntry LoadTutorialDataByID(TextAsset path, string stepName, int id){
            if (path == null) {
                Debug.LogError("LoadTutorialDataByID: TextAsset is null");
                return null;
            }

            if (string.IsNullOrWhiteSpace(stepName)) {
                Debug.LogError("LoadTutorialDataByID: stepName is null/empty");
                return null;
            }

            // Parse (ou réutilise) le document XML.
            XmlDocument doc;
            int instanceId = path.GetInstanceID();
            if (_cachedTutorialDoc != null && _cachedTutorialTextAssetInstanceId == instanceId) {
                doc = _cachedTutorialDoc;
            } else {
                doc = new XmlDocument();
                doc.LoadXml(path.text);
                _cachedTutorialDoc = doc;
                _cachedTutorialTextAssetInstanceId = instanceId;
            }

            XmlNode node = doc.DocumentElement;
            if (node is not{ Name: "Tutorial" }) throw new Exception("Invalid XML format.");

            XmlNode stepNode = doc.SelectSingleNode($"/Tutorial/Step[@Name='{stepName}']");
            if (stepNode == null) {
                Debug.LogError($"Step '{stepName}' not found");
                return null;
            }

            // IMPORTANT: la recherche d'Entry doit se faire *dans ce Step*.
            XmlNode entryNode = stepNode.SelectSingleNode($"Entry[ID='{id}']");
            if (entryNode == null) return null;

            // Parsing robuste.
            string idText = entryNode["ID"]?.InnerText;
            if (!int.TryParse(idText, out int parsedId)) {
                Debug.LogError($"Invalid tutorial Entry ID '{idText}' for Step '{stepName}'");
                return null;
            }

            TutorialEntry entry = new TutorialEntry{
                ID = parsedId,
                Intitule = entryNode["Intitule"]?.InnerText,
                Text = entryNode["Text"]?.InnerText
            };

            return entry;
        }

        /// <summary>
        /// Validates an XML file against a given XSD schema.
        /// Should be called at the start of the game to ensure XML integrity.
        /// </summary>
        /// <param name="pathXmlFile">The file path of the XML document to validate.</param>
        /// <param name="pathXsdFile">The file path of the XSD schema to validate against.</param>
        /// <returns>True if the XML is valid according to the schema; otherwise, false.</returns>
        public static bool ValidateXML(string pathXmlFile, string pathXsdFile){
            if (!File.Exists(pathXsdFile)) {
                Debug.LogError("XSD file not found for validation");
                return false;
            }

            if (!File.Exists(pathXmlFile)) {
                Debug.LogError("XML file not found for validation");
                return false;
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(pathXmlFile);
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.Add("", pathXsdFile);

            bool isValid = true;
            xmlDoc.Schemas = schemaSet;
            xmlDoc.Validate((_, args) => {
                if (args.Severity == XmlSeverityType.Error) {
                    Debug.LogError("XML validation error: " + args.Message);
                    isValid = false;
                }
            });
            return isValid;
        }
    }
}