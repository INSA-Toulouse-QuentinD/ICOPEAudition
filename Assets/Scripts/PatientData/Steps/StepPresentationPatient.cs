using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PatientData.Steps{
    /// <summary>
    /// Manages the UI presentation of patient data in Step 1 of the algorithm.
    /// </summary>
    public class StepPresentationPatient : MonoBehaviour{
        [Header("Sprites")] [SerializeField] private Image patientSprite;
        [SerializeField] private Image patientSpritePhone;
        [SerializeField] private GameObject bulle;

        // PROFILE DATA
        [Header("Profile fields")] [SerializeField]
        private TextMeshProUGUI nameFields;

        [SerializeField] private TextMeshProUGUI surnameFields;
        [SerializeField] private TextMeshProUGUI ageFields;
        [SerializeField] private TextMeshProUGUI situationFields;

        // CONTEXT DATA
        [Header("Context field")] [SerializeField]
        private TextMeshProUGUI contextField;

        // MEDICAL HISTORY DATA
        [Header("Medical history field")] [SerializeField]
        private TextMeshProUGUI historyField;

        /// <summary>
        /// Populates UI text fields with patient data.
        /// </summary>
        /// <param name="patientData">Data about the patient to display.</param>
        public void SetPresentationTexts(PatientData patientData){
            // Set profil data
            nameFields.text = "<font=\"Articulate-ExtraBold SDF\">Nom :</font> " + patientData.lastName.ToUpper();
            surnameFields.text = "<font=\"Articulate-ExtraBold SDF\">Prénom :</font> " + patientData.firstName;
            ageFields.text = "<font=\"Articulate-ExtraBold SDF\">Âge :</font> " + patientData.age + " ans";
            situationFields.text = "<font=\"Articulate-ExtraBold SDF\">Mode de vie :</font> " + patientData.lifestyle;

            // Set contexte data
            contextField.text = patientData.context;

            // Set medical history data
            historyField.text = patientData.medicalHistory;
        }

        /// <summary>
        /// Sets and adjusts the patient's sprite image.
        /// </summary>
        /// <param name="patient">The sprite representing the patient.</param>
        /// <param name="isPhone">If the patient is on phone.</param>
        /// <param name="phone">The sprite of the phone.</param>
        public void SetSprites(Sprite patient, bool isPhone, Sprite phone){
            patientSprite.sprite = isPhone ? phone : patient;
            if (isPhone) {
                patientSprite.rectTransform.sizeDelta = new Vector2(700, 700);
            } else {
                patientSprite.SetNativeSize();
            }
            
            patientSpritePhone.sprite = patient;
            bulle.SetActive(isPhone);
        }
    }
}