using System;
using System.Collections.Generic;
using UnityEngine;

namespace PatientData{
    [Serializable]
    public class Manifest{
        public List<LevelManifest> levels;
    }

    [Serializable]
    public class LevelManifest{
        public string name;
        public List<string> patients;
    }

    /// <summary>
    /// ScriptableObject storing patient cases organized by levels.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelsData", menuName = "Medical/LevelsData")]
    public class LevelsData : ScriptableObject{
        public List<PatientCaseLevel> patientByLevel = new();
    }

    /// <summary>
    /// Represents a collection of patient cases for a specific level.
    /// </summary>
    [Serializable]
    public class PatientCaseLevel{
        public string name;
        public List<PatientData> patientsCase = new();
    }
}