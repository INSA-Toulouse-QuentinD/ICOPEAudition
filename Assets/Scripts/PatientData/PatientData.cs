using System.Collections.Generic;
using PatientData.AlgoData;
using UnityEngine;

namespace PatientData{
    /// <summary>
    /// ScriptableObject representing detailed patient data for medical scenarios.
    /// </summary>
    [CreateAssetMenu(fileName = "PatientData", menuName = "Medical/Patient")]
    public class PatientData : ScriptableObject{
        [Header("Profil")] public Sprite[] characterSprites;
        public bool isOnPhone;
        public string lastName;
        public string firstName;
        public int age;

        [Header("Fiche patient")] [TextArea] public string familySituation;

        [TextArea] public string occupationalActivities;

        [TextArea] public string context;

        [Header("Medical History")] [TextArea] public string medicalHistory;

        [TextArea, Header("Description level")]
        public string descriptionLevel;

        [Header("Algorithm steps")] public List<AlgoStep> steps;
    }
}