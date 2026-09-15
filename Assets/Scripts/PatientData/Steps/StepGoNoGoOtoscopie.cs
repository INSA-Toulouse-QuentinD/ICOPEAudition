using PatientData.AlgoData;
using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PatientData.Steps{
    /// <summary>
    /// Manages the display of questionnaire questions and patient answers, along with patient sprite.
    /// </summary>
    public class StepGoNoGoOtoscopie : MonoBehaviour{
        // Content GameObject
        [Header("GameObject content")] [SerializeField]
        private GameObject content;
        [SerializeField] private Image patientSprite;
        
        [SerializeField] private Image videoOtoscopiePatient;
        
        [SerializeField] private GameObject bulleTextPatient;
        [SerializeField] private TMP_Text textPatient;

        /// <summary>
        /// Sets the questionnaire questions and corresponding patient answers into the UI.
        /// </summary>
        /// <param name="questions">List of questions to display.</param>
        /// <param name="answers">List of patient answers corresponding to questions.</param>
        public void SetQuestionaryText(List<QuestionData> questions, List<YesNo> answers){
            // Set text in children
            TextMeshProUGUI[] testMeshes = content.GetComponentsInChildren<TextMeshProUGUI>();

            int max = Mathf.Min(answers.Count, testMeshes.Length);

            for (int i = 0; i < max; i++) {
                testMeshes[i].text = "- " + questions[i].questionText + "\n" + (answers[i] == YesNo.Yes ? "<b>Oui</b>" : "<b>Non</b>");
            }
        }

        /// <summary>
        /// Sets the otoscopy video sprite and patient sprite.
        /// </summary>
        /// <param name="sprite">Otoscopy video sprite.</param>
        /// <param name="spritePatient">Patient sprite.</param>
        public void SetImages(Sprite sprite, Sprite spritePatient){
            if (!spritePatient) return;
            videoOtoscopiePatient.sprite = sprite;

            patientSprite.sprite = spritePatient;
            patientSprite.SetNativeSize();
        }

        public void SetTextDialogue(string patientContext){
            textPatient.text = patientContext;
            bulleTextPatient.SetActive(!string.IsNullOrEmpty(patientContext));
        }
    }
}