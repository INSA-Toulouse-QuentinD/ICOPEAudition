using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PatientData.Steps {
    /// <summary>
    /// Manages the Audiometrie step UI, displaying the audiometry image and patient sprite.
    /// </summary>
    public class StepAudiometrie : MonoBehaviour {
        [SerializeField] private Image patientImage;
        [SerializeField] private Image imageAudiometrie;
        [SerializeField] private GameObject bulleDoc;
        [SerializeField] private TextMeshProUGUI textDoc;
        [SerializeField] private RectTransform porteDocument;

        /// <summary>
        /// Sets the audiometry and patient sprites for display.
        /// </summary>
        /// <param name="spriteAudio">The audiometry test image sprite.</param>
        /// <param name="spritePatient">The patient sprite.</param>
        public void SetSprite(Sprite spriteAudio, Sprite spritePatient) {
            imageAudiometrie.sprite = spriteAudio;
            patientImage.sprite = spritePatient;
            patientImage.SetNativeSize();
        }
        
        public void SetText(string text) {
            textDoc.text = text;
            bulleDoc.SetActive(text != null);
            porteDocument.anchoredPosition = new Vector3((text == null ? 0 : 100), 50, 0);
        }
    }
}
