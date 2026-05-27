using System.Collections.Generic;
using UnityEngine;

namespace PatientData.AlgoData{
    /// <summary>
    /// Enum representing the different steps in the medical algorithm.
    /// </summary>
    public enum Step{
        CasePresentation,
        WisperTest,
        Questionnary,
        AdditionalQuestionnaire,
        Otoscopy,
        WeberTest,
        HhiesTest,
        Audiometry,
    }

    /// <summary>
    /// Represents an answer option for a question, with text, correctness, and optional correction info.
    /// </summary>
    [System.Serializable]
    public class AnswerData{
        public string answerText;
        public bool isCorrect;
        [TextArea(5, 5)] public string correctionText;
        public List<Sprite> sprites;
    }

    /// <summary>
    /// Represents a step in the medical algorithm workflow.
    /// Includes context, questionnaire data, exam sprites, diagnostic and action phases, and metadata.
    /// </summary>
    [System.Serializable]
    public class AlgoStep{
        public Step type; // The step type enum

        [TextArea] public string dialoguePatient;

        public List<YesNo> predefinedAnwser = new(){
            YesNo.No, YesNo.No, YesNo.No, YesNo.No, YesNo.No, YesNo.No, YesNo.No
        }; // Predefined answers for patient

        public Sprite spriteEarExams; // Image representing ear exams, HHIES exam or audimetry
        
        public List<AnswerData> diagnosticPhase; // Diagnostic phase data

        public List<AnswerData> actionPhase; // Action phase data
    }
}