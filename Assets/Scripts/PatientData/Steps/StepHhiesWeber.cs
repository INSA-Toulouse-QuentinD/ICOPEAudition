using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PatientData.Steps{
    /// <summary>
    /// Manages the HHIES test step UI, displaying the HHIES image and the patient sprite.
    /// </summary>
    public class StepHhiesWeber : MonoBehaviour{
        [SerializeField] private TextMeshProUGUI textDoc;
        [SerializeField] private Image imagePatient;
        [SerializeField] private Image imageHhies;
        [SerializeField] private Image imageOtoscopie;

        public void SetText(string text){
            textDoc.text = text;
        }

        public void SetImages(Sprite patient, Sprite hhies, Sprite otoscopie){
            imagePatient.sprite = patient;
            imageHhies.sprite = hhies;
            imageOtoscopie.sprite = otoscopie;
        }
    }
}