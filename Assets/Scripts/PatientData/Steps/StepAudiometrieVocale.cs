using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PatientData.Steps{
    public class StepAudiometrieVocale : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Image patientImage;
        
        public void SetStep(string dialogue, Sprite sprite){
            text.text = dialogue;
            patientImage.sprite = sprite;
        }
    }
}