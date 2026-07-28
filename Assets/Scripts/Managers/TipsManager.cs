using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers{
    public class TipsManager : MonoBehaviour{
        public static TipsManager Instance;
        
        [SerializeField] private TMP_Text titre;
        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject button;
        [SerializeField] private Button buttonNext;
        [SerializeField] private Button buttonPrevious;
        [SerializeField] private List<GameObject> tipsList;

        private GameObject _actualTip;
        private int _actualChild;

        void Awake(){
            Instance = this;
            
            panel.SetActive(false);
            button.SetActive(false);
        }
        
        public void InitTips(RappelTip rappelTip){
            int index = (int)rappelTip - 1;
            if (index < 0 || index >= tipsList.Count) {
                return;
            }

            _actualTip = tipsList[index];
            titre.text = _actualTip.name;
            
            foreach (GameObject tip in tipsList) {
                tip.SetActive(false);
            }
            _actualTip.SetActive(true);
            
            foreach (Transform child in _actualTip.transform) {
                child.gameObject.SetActive(false);
            }
            _actualTip.transform.GetChild(0).gameObject.SetActive(true);
            _actualChild = 0;
            ButtonInteraction();
            
            button.SetActive(true);
        }
        
        public void ShowTips(){
            panel.SetActive(true);
            button.SetActive(false);
        }
        
        public void HideTips(){
            panel.SetActive(false);
            button.SetActive(true);
        }
        
        public void HideButton(){
            button.SetActive(false);
        }

        public void Next(){
            if (_actualTip.transform.childCount > _actualChild + 1) {
                _actualTip.transform.GetChild(_actualChild + 1).gameObject.SetActive(true);
                _actualTip.transform.GetChild(_actualChild).gameObject.SetActive(false);
                _actualChild++;
                ButtonInteraction();
            }
        }

        public void Previous(){
            if (_actualChild > 0) {
                _actualTip.transform.GetChild(_actualChild - 1).gameObject.SetActive(true);
                _actualTip.transform.GetChild(_actualChild).gameObject.SetActive(false);
                _actualChild--;
                ButtonInteraction();
            }
        }

        private void ButtonInteraction(){
            buttonNext.interactable = _actualTip.transform.childCount > _actualChild + 1;
            buttonPrevious.interactable = _actualChild > 0;
        }
    }

    public enum RappelTip{
        None,
        Algorithme,
        RedFlag,
        Wisper,
        Weber,
        Tympan,
        Prescription,
        Rinne,
        Acoumetry,
        Hhies,
        Hygiene,
        Acouphene,
        Bdc,
        Appareil
    }
}