using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.PatientData.AlgoData
{
    /// <summary>
    /// Enum representing the different steps in the medical algorithm.
    /// </summary>
    public enum Step
    {
        Case_presentation,
        Wisper_test,
        Questionnary,
        Additional_questionnaire,
        Otoscopy,
        Weber_test,
        HHIES_test,
        Audiometry,
    }

    /// <summary>
    /// Represents an answer option for a question, with text, correctness, and optional correction info.
    /// </summary>
    [System.Serializable]
    public class AnswerData
    {
        public string answerText;
        public bool isCorrect;
        [TextArea]
        public string correctionText;
        public List<Sprite> sprites;
    }


    /// <summary>
    /// Stores a question and the patients yes/no response.
    /// </summary>
    [System.Serializable]
    public class PatientQuestionAnswer
    {
        public YesNo patientAnswer;
    }

    /// <summary>
    /// Represents a phase within a step, such as diagnostic or action phase,
    /// including question text and possible answers.
    /// </summary>
    [System.Serializable]
    public class PhaseData
    {
        public List<AnswerData> answerData;
        

        public bool IsAnswerCorrect(int index)
        {
            if (index < 0 || index >= answerData.Count) return false;
            return answerData[index].isCorrect;
        }

        public string GetCorrection(int index)
        {
            if (index < 0 || index >= answerData.Count) return "";
            return answerData[index].correctionText;
        }
    }

    /// <summary>
    /// Represents a step in the medical algorithm workflow.
    /// Includes context, questionnaire data, exam sprites, diagnostic and action phases, and metadata.
    /// </summary>
    [System.Serializable]
    public class AlgoStep
    {
        public Step type; // The step type enum
        
        [Header("Si type Wisper / Weber")]
        [TextArea]
        public string dialoguePatient;
        
        [Header("Si type: Questionnary")]
        public List<PatientQuestionAnswer> predefinedAnwser;  // Predefined answers for patient

        [Header("Si type: Video Otoscopie / test HHIES / audiometrie")]
        public Sprite spriteEarExams; // Image representing ear exams, HHIES exam or audimetry


        [Header("Phase 1: Diagnotic")]
        public PhaseData diagnosticPhase;  // Diagnostic phase data

        [Header("Phase 2: Action")]
        public PhaseData actionPhase;   // Action phase data
    }
}
