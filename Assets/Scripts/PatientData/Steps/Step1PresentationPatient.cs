using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PatientData.Steps{
    /// <summary>
    /// Manages the UI presentation of patient data in Step 1 of the algorithm.
    /// </summary>
    public class Step1PresentationPatient : MonoBehaviour{
        [Header("Sprites")] [SerializeField] private Image patientSprite;
        [SerializeField] private Image patientSpritePhone;
        [SerializeField] private GameObject bulle;

        // PROFILE DATA
        [Header("Profile fields")] [SerializeField]
        private TextMeshProUGUI nameFields;

        [SerializeField] private TextMeshProUGUI surnameFields;
        [SerializeField] private TextMeshProUGUI ageFields;
        [SerializeField] private TextMeshProUGUI situationFields;
        [SerializeField] private TextMeshProUGUI activitiesFields;

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
            nameFields.text = patientData.lastName;
            surnameFields.text = patientData.firstName;
            ageFields.text = patientData.age + " ans";
            situationFields.text = patientData.familySituation;
            activitiesFields.text = patientData.occupationalActivities;

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
            patientSprite.SetNativeSize();
            
            patientSpritePhone.sprite = patient;
            bulle.SetActive(isPhone);
        }
    }
}