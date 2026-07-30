using System.Collections;
using System.Collections.Generic;
using PatientData.AlgoData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PatientData.Steps{
    public class StepTelephone : MonoBehaviour{
        [SerializeField] private Image patientSpritePhone;
        [SerializeField] private GameObject bulleDiscussion;
        [SerializeField] private Transform content;

        private Image _lastBubble;

        public void SetSprites(Sprite patient){
            patientSpritePhone.sprite = patient;
        }

        public IEnumerator StartConv(List<Discussion> discussion){
            foreach (Discussion d in discussion) {
                GameObject go = Instantiate(bulleDiscussion, content);

                go.GetComponentInChildren<TextMeshProUGUI>().text = d.text;

                if (d.qui == "Patient") {
                    go.transform.localScale = new Vector3(-1, 1, 1);
                    go.transform.GetChild(0).localScale = new Vector3(-1, 1, 1);
                }

                // Retire la surbrillance de l'ancienne bulle
                if (_lastBubble != null) {
                    _lastBubble.color = Color.white;
                }

                // Met en surbrillance la nouvelle
                _lastBubble = go.GetComponent<Image>();
                _lastBubble.color = Color.yellow;

                yield return new WaitForSeconds(3f);
            }
        }
    }
}