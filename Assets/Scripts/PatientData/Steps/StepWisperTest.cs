using System.Collections;
using System.Collections.Generic;
using PatientData.AlgoData;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PatientData.Steps{
    /// <summary>
    /// Manages the Whisper Test step UI, including doctor dialogue animations and patient sprite display.
    /// </summary>
    public class StepWisperTest : MonoBehaviour{
        // DOCTOR POSITION
        [Header("Doctor position")] [SerializeField]
        private GameObject doctorPos1;

        [SerializeField] private GameObject doctorPos2;

        // DOCTOR TEXT
        [Header("Doctor texts")] [SerializeField]
        private GameObject goDoctorText;
        [SerializeField] private GameObject goDoctorText1;

        [SerializeField] private GameObject goDoctorText2;
        [SerializeField] private TextMeshProUGUI targetDoctorText;
        [SerializeField] private TextMeshProUGUI targetDoctorText1;
        [SerializeField] private TextMeshProUGUI targetDoctorText2;

        // PATIENT SPRITE
        [Header("Patient sprite")] [SerializeField]
        private Image patientSprite;

        // PATIENT TEXT
        [Header("Patient text")] [SerializeField]
        private GameObject goPatientText;

        [SerializeField] private TextMeshProUGUI targetPatientText;

        // Animation timing delays
        private const float DelayBetweenWords = 1f;
        private const float DelayBetweenText = 2f;


        // Words spoken by the doctor (can be moved to ScriptableObject for configurability)
        private List<string> _availableDoctorWords;

        private readonly string[] _doctorWords ={
            "Ami", "Bateau", "Bureau", "Chameau", "Cheval", "Hibou", "Journal", "Lama", "Lapin", "Moto", "Mouton",
            "Parfait", "Pompier", "Salon", "Serpent"
        };

        private string _patientText;

        // Clears all text UI fields
        private void ClearTexts(){
            targetDoctorText1.text = "";
            targetDoctorText2.text = "";
            targetDoctorText.text = "";
            targetPatientText.text = "";
        }

        // Disables all dialogue boxes
        private void ClearDialogueBox(){
            goDoctorText1.SetActive(false);
            goDoctorText2.SetActive(false);
            goDoctorText.SetActive(false);
            goPatientText.SetActive(false);
        }

        // Hides doctor position sprites
        private void ClearDoctorSprite(){
            doctorPos1.SetActive(false);
            doctorPos2.SetActive(false);
        }

        // Returns an array of 4 random words from doctorWords
        private void ResetDoctorWordsPool(){
            _availableDoctorWords = new List<string>(_doctorWords);
        }

        private string[] GetRandomListWord(){
            if (_availableDoctorWords == null || _availableDoctorWords.Count < 4) {
                ResetDoctorWordsPool();
            }

            string[] result = new string[4];

            for (int i = 0; i < result.Length; i++) {
                int index = Random.Range(0, _availableDoctorWords!.Count);
                result[i] = _availableDoctorWords[index];
                _availableDoctorWords.RemoveAt(index);
            }

            return result;
        }

        /// <summary>
        /// Animates the display of words sequentially in the given TextMeshProUGUI target.
        /// </summary>
        /// <param name="goTarget">GameObject containing the text to show/hide.</param>
        /// <param name="target">TextMeshProUGUI component to update.</param>
        /// <param name="words">Words to animate.</param>
        /// <param name="startDelay">Delay before starting animation.</param>
        /// <param name="onComplete">Callback after animation completes.</param>
        private void AnimateText(GameObject goTarget, TextMeshProUGUI target, string[] words, float startDelay = 0f,
            TweenCallback onComplete = null){
            target.text = "";
            goTarget.SetActive(false);

            for (int i = 0; i < words.Length; i++) {
                string word = words[i];
                float delay = startDelay + i * DelayBetweenWords;

                DOVirtual.DelayedCall(delay, () => {
                    target.text = word;
                    goTarget.SetActive(true);
                });

                float hideDelay = delay + DelayBetweenWords * 0.8f;
                DOVirtual.DelayedCall(hideDelay, () => { goTarget.SetActive(false); });
            }

            if (onComplete != null) {
                float totalTime = startDelay + words.Length;
                DOVirtual.DelayedCall(totalTime, onComplete);
            }
        }

        /// <summary>
        /// Sets the patient sprite for display.
        /// </summary>
        public void SetPatient(Sprite patient){
            if (!patient) return;

            patientSprite.sprite = patient;
            patientSprite.SetNativeSize();
        }

        /// <summary>
        /// Starts the first doctor text animation and queues subsequent animations.
        /// </summary>
        public IEnumerator PlayFirstText(AlgoStep step){
            ResetDoctorWordsPool();

            // Load in memory patient text form algoStep
            _patientText = step.dialoguePatient;

            // Clear texts & doctor sprite
            ClearTexts();
            ClearDialogueBox();
            ClearDoctorSprite();

            // Activate doctor sprite position 1
            doctorPos1.SetActive(true);
            
            if (!string.IsNullOrEmpty(step.dialogueDoctor)) {
                targetDoctorText.text = step.dialogueDoctor;
                goDoctorText.SetActive(true);
                yield return new WaitForSeconds(6f);
                goDoctorText.SetActive(false);
            }

            string[] strings = GetRandomListWord();
            AnimateText(goDoctorText1, targetDoctorText1, strings, 0f,
                () => { DOVirtual.DelayedCall(DelayBetweenText, PlaySecondText); });
        }

        /// <summary>
        /// Plays the second doctor text animation then shows patient text.
        /// </summary>
        private void PlaySecondText(){
            ClearTexts();
            ClearDialogueBox();
            ClearDoctorSprite();

            doctorPos2.SetActive(true);

            string[] strings = GetRandomListWord();
            AnimateText(goDoctorText2, targetDoctorText2, strings, 0f,
                () => { DOVirtual.DelayedCall(DelayBetweenText, ShowPatientText); });
        }

        /// <summary>
        /// Displays the patient's text in the UI.
        /// </summary>
        private void ShowPatientText(){
            ClearTexts();
            ClearDialogueBox();

            goPatientText.SetActive(true);
            targetPatientText.text = _patientText;
        }

        public void SkipAnimation(AlgoStep step){
            ClearTexts();
            ClearDialogueBox();
            ClearDoctorSprite();

            doctorPos2.SetActive(true);

            goPatientText.SetActive(true);
            targetPatientText.text = step.dialoguePatient;
        }
    }
}