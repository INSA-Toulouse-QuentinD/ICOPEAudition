using Assets.Scripts.UI.TutorialContents;
using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using UnityEngine;

namespace Assets.Scripts.Managers{
    /// <summary>
    /// XML Manager handles loading, saving, and validating game data using XML files.
    /// It supports deserialization of complex nested data structures (including dictionaries, lists, and custom structs),
    /// recursive serialization of C# objects into XML, and validation against XSD schemas to ensure data integrity.
    /// This utility facilitates persistent storage and retrieval of game progress and settings.
    /// </summary>
    public static class XmlManager{
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
            Debug.Log(path.text);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(path.text);

            XmlNode node = doc.DocumentElement;
            if (node == null || node.Name != "Tutorial") throw new Exception("Invalid XML format.");

            node = doc.SelectSingleNode($"//Step[@Name='{stepName}']");
            if (node == null) {
                Debug.LogError($"Step '{stepName}' not found");
                return null;
            }

            TutorialEntry entry = new TutorialEntry();
            node = doc.SelectSingleNode($"//Entry[ID='{id}']");
            if (node != null) {
                string innerText = node["ID"]?.InnerText;
                if (innerText != null) {
                    entry.ID = int.Parse(innerText);
                    entry.Intitule = node["Intitule"]?.InnerText;
                    entry.Text = node["Text"]?.InnerText;
                }

                return entry;
            }

            return null;
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
            xmlDoc.Validate((sender, args) => {
                if (args.Severity == XmlSeverityType.Error) {
                    Debug.LogError("XML validation error: " + args.Message);
                    isValid = false;
                }
            });
            return isValid;
        }
    }
}