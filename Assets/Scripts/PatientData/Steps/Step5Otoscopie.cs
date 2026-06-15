using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PatientData.Steps{
    /// <summary>
    /// Manages display of otoscopy video and patient sprite images.
    /// </summary>
    public class Step5Otoscopie : MonoBehaviour{
        [Header("Patient video otoscopie field")] [SerializeField]
        private Image videoOtoscopiePatient;

        [Header("Patient")] [SerializeField] private Image patient;
        [SerializeField] private GameObject bulleTextPatient;
        [SerializeField] private TMP_Text textPatient;

        /// <summary>
        /// Sets the otoscopy video sprite and patient sprite.
        /// </summary>
        /// <param name="sprite">Otoscopy video sprite.</param>
        /// <param name="spritePatient">Patient sprite.</param>
        public void SetImages(Sprite sprite, Sprite spritePatient){
            if (!sprite) return;
            if (!spritePatient) return;
            videoOtoscopiePatient.sprite = sprite;

            patient.sprite = spritePatient;
            patient.SetNativeSize();
        }

        public void SetTextDialogue(string patientContext){
            textPatient.text = patientContext;
            bulleTextPatient.SetActive(!string.IsNullOrEmpty(patientContext));
        }
    }
}