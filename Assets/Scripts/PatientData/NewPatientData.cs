using PatientData.AlgoData;
using System.Collections.Generic;
using UnityEngine;

namespace PatientData
{
    /// <summary>
    /// ScriptableObject representing detailed patient data for medical scenarios.
    /// </summary>
    [CreateAssetMenu(fileName = "NewPatientData", menuName = "Medical/Patient")]
    public class NewPatientData : ScriptableObject
    {
        [Header("Profil")]
        public Sprite[] characterSprites;
        public string lastName;
        public string firstName;
        public int age;

        [Header("Fiche patient")]
        [TextArea]
        public string familySituation;

        [TextArea]
        public string occupationalActivities;

        [TextArea]
        public string context;

        public string ADL = "6";
        public string IADL = "8";

        [Header("Medical History")]
        [TextArea]
        public string medicalHistory;
        
        [TextArea, Header("Description level")] public string descriptionLevel;

        [Header("Algorithm steps")]
        public List<AlgoStep> steps;
    }
}
