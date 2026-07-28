using System.Collections.Generic;
using Managers;
using UnityEngine;

namespace PatientData.AlgoData{
    /// <summary>
    /// Enum representing the different steps in the medical algorithm.
    /// </summary>
    public enum Step{
        CasePresentation = 0,
        WisperTest = 1,
        GoNoGo = 2,
        Otoscopy = 3,
        Hhies = 4,
        WeberTest = 5,
        Audiometry = 6,
        Telephone = 7,
        HhiesWeber = 8,
    }

    /// <summary>
    /// Represents an answer option for a question, with text, correctness, and optional correction info.
    /// </summary>
    [System.Serializable]
    public class AnswerData{
        public string answerText;
        public bool isCorrect;
        [TextArea(5, 5)] public string correctionText;
        public RappelTip rappelTip;
        public List<Sprite> sprites;
    }
    
    [System.Serializable]
    public class Discussion{
        public string qui;
        public string text;
    }

    /// <summary>
    /// Represents a step in the medical algorithm workflow.
    /// Includes context, questionnaire data, exam sprites, diagnostic and action phases, and metadata.
    /// </summary>
    [System.Serializable]
    public class AlgoStep{
        public Step type; // The step type enum

        [TextArea] public string dialogueDoctor;
        [TextArea] public string dialoguePatient;

        public List<YesNo> predefinedAnswer = new(){
            YesNo.No, YesNo.No, YesNo.No, YesNo.No, YesNo.No, YesNo.No, YesNo.No
        }; // Predefined answers for patient

        public List<Discussion> discussion;

        public Sprite spriteEarExams; // Image representing ear exams, HHIES exam or audiometry
        public Sprite spriteEarExams2;

        public string overrideDiagnostic;
        public List<AnswerData> diagnosticPhase; // Diagnostic phase data

        public string overrideAction;
        public List<AnswerData> actionPhase; // Action phase data
    }
}