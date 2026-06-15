using Managers;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

namespace UI.TutorialContents{
    /// <summary>
    /// Controler that manage text placement for the Tutorial.
    /// </summary>
    public class UITutorialControler : MonoBehaviour{
        public static UITutorialControler Instance { get; private set; }
        
        [SerializeField] private TMP_Text intituleText;
        [SerializeField] private TMP_Text text;
        
        #region Local variable

        private int _indexText;
        private string _nameStep;

        // Garde en mémoire les index déjà affichés pour TutorialMichel.
        // Doit être réinitialisé quand le joueur recommence le tutoriel depuis le début.
        private readonly HashSet<int> _tutorialMichelShownIndexes = new();

        #endregion

        #region Public methods

        /// <summary>
        /// Advances to the next tutorial text by incrementing the index and loading the corresponding text.
        /// </summary>
        public void NextTextButton(){
            if (_nameStep == "Waiting_room" && _indexText < 3) {
                _indexText++;
                SetTexts(_nameStep, _indexText);
            } else {
                if (_nameStep == "Tutorial" && _indexText == 0) {
                    _indexText = 12;
                    SetTexts(_nameStep, _indexText);
                } else {
                    GameManager.Instance.SetTutorialUI();
                    _indexText = -1;
                }
            }
        }

        public void TutorialMichel(int index){
            if (GameManager.Instance.skipAssistante) return;
            
            // Uniquement si le joueur est dans le tutoriel (niveau 0).
            if (GameManager.Instance.GameStateManager.GetCurrentLevel() != 0) return;

            // Si le joueur recommence le tutoriel depuis le début, on reset la mémoire.
            if (index == 0) {
                _tutorialMichelShownIndexes.Clear();
            }

            // Chaque index une seule fois.
            if (!_tutorialMichelShownIndexes.Add(index)) return;

            _nameStep = "Tutorial";
            _indexText = index;
            GameManager.Instance.SetTutorialUI();
            SetTexts(_nameStep, index);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Sets the UI text fields based on XML tutorial data.
        /// </summary>
        /// <remarks>
        /// Loads a tutorial entry by its step name and text ID, then updates UI elements with the entry's title and text.
        /// If no entry is found, resets the tutorial UI and index.
        /// </remarks>
        /// <param name="nameStep">The name of the current tutorial step (used to locate data in the XML).</param>
        /// <param name="idSteps">The ID of the specific text entry to load.</param>
        private void SetTexts(string nameStep, int idSteps){
            TextAsset pathToXml = Resources.Load<TextAsset>($"Data/Tutorial");
            if (pathToXml) {
                TutorialEntry tutoEntry = XmlManager.LoadTutorialDataByID(pathToXml, nameStep, idSteps);
                if (tutoEntry != null) {
                    intituleText.text = tutoEntry.Intitule;
                    text.text = tutoEntry.Text;
                }
            }
        }
        #endregion
        
        /// <summary>
        /// Called the first time the object becomes enabled and active.
        /// Initializes the tutorial step name and sets related texts.
        /// </summary>
        private void Awake(){
            Instance = this;
            
            _nameStep = "Waiting_room";
            SetTexts(_nameStep, _indexText);
        }
    }
}