using System.Collections.Generic;
using Managers;
using PatientData;
using TMPro;
using UI.ScoreContents;
using UnityEngine;
using UnityEngine.UI;
using static Managers.GameStateManager;

namespace UI.LevelSelector{
    /// <summary>
    /// Manages level selection, patient case buttons, and game start logic in the level selection menu.
    /// </summary>
    /// <remarks>
    /// Handles UI elements for selecting levels and patients, including dynamically instantiating buttons,
    /// updating descriptions, and launching the game with the selected configuration.
    /// </remarks>
    public class LevelSelector : MonoBehaviour{
        // buttons, patient name and descriptions per levels
        /// <summary>
        /// Container for each level's difficulty button.
        /// </summary>
        [SerializeField, Header("Ui elements")]
        private GameObject levelSelector; // Use to desactivate levelSelector.

        [SerializeField] private TextMeshProUGUI description; // descriptions text ui.

        [SerializeField] private GameObject[]
            patientsList; // Uis to desactive when tuto selected (scroll view gameObject + Separator gameObject).

        [Header("Button")] [SerializeField] private GameObject levelContainer;

        [SerializeField] private GameObject patientContainer;
        [SerializeField] private GameObject buttonPref;
        [SerializeField] private Button startGameButton;

        private List<ButtonPressDetector> _levelCompos;

        [HideInInspector] public LevelsData levelsData;
        private int _currentIndexLevelSelected;
        private int _currentIndexPatientCase;

        private bool _init;

        private void OnEnable(){
            if (_init) return;
            _init = true;
            
            // add listerner to stats button
            startGameButton.onClick.AddListener(LoadPatientCase);
            
            levelsData = GameManager.Instance.levelsData;
            _levelCompos = new List<ButtonPressDetector>();
            foreach (PatientCaseLevel pcl in levelsData.patientByLevel) {
                ButtonPressDetector bpd = Instantiate(buttonPref, levelContainer.transform)
                    .GetComponent<ButtonPressDetector>();
                bpd.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = pcl.name;
                _levelCompos.Add(bpd);
            }
            
            // Le tutoriel et selectionner de base.
            ButtonsManager.SetButtonFocused(_levelCompos[0]);
            ActivePatientUiElements(false);
            ChangeDescription(0);
            for (int i = 0; i < _levelCompos.Count; i++) {
                int index = i;
                _levelCompos[i].onPress.AddListener(delegate{ ChangeLevel(index); });
            }

            ChangeLevel(0);
        }

        /// <summary>
        /// Activates or deactivates patient-related UI elements (e.g., scroll view and separators).
        /// </summary>
        /// <param name="active">Whether to enable or disable the patient UI elements.</param>
        private void ActivePatientUiElements(bool active){
            foreach (GameObject patientElem in patientsList) {
                patientElem.SetActive(active);
            }
        }

        /// <summary>
        /// Changes the selected level and updates UI elements accordingly, such as patient buttons and description.
        /// </summary>
        /// <param name="index">Index of the selected level.</param>
        private void ChangeLevel(int index){
            // get level composition
            _currentIndexLevelSelected = index;

            // Get nb patient case
            int nbPatient = levelsData.patientByLevel[_currentIndexLevelSelected].patientsCase.Count;


            // assign current description and patient buttons
            ShowPatientButtons(nbPatient, index);
            ActivePatientUiElements(true);
            ChangeDescription(0);
            ButtonsManager.SetButtonFocused(patientContainer.transform.GetChild(0).GetComponent<ButtonPressDetector>());
        }

        /// <summary>
        /// Instantiates and configures patient selection buttons for the selected level.
        /// </summary>
        /// <param name="nbPatient">Number of patient cases to display for the selected level.</param>
        /// <param name="level">Index of the selected level.</param>
        private void ShowPatientButtons(int nbPatient, int level){
            foreach (Transform child in patientContainer.transform) {
                Destroy(child.gameObject);
            }

            int currentPatient = GameManager.Instance.GameStateManager.GetMaxPatientCase(level);

            // Get nb patient case
            for (int i = 0; i < nbPatient; i++) {
                GameObject go = Instantiate(buttonPref, patientContainer.transform);

                string firstName = levelsData.patientByLevel[_currentIndexLevelSelected].patientsCase[i].firstName;
                go.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = firstName;

                var i1 = i;
                go.GetComponent<ButtonPressDetector>().onPress.AddListener(delegate{ ChangeDescription(i1); });
                go.gameObject.SetActive(true);

                if (i > currentPatient)
                    go.GetComponent<ButtonPressDetector>().AssignState(ButtonPressDetector.ButtonState.Disable);
                else
                    go.GetComponent<ButtonPressDetector>().AssignState(ButtonPressDetector.ButtonState.None, true);
            }
        }

        /// <summary>
        /// Updates the description text for the currently selected patient case.
        /// </summary>
        /// <param name="indexPatientCase">Index of the selected patient case within the level.</param>
        private void ChangeDescription(int indexPatientCase){
            _currentIndexPatientCase = indexPatientCase;
            string infoLevel = levelsData.patientByLevel[_currentIndexLevelSelected].patientsCase[indexPatientCase]
                .descriptionLevel;
            description.text = infoLevel;
        }

        /// <summary>
        /// Visually updates the difficulty button for the current level to reflect its selected state.
        /// </summary>
        public void UpdateLevelButton(){
            int currentLevel = GameManager.Instance.GameStateManager.GetCurrentLevel();
            _levelCompos[currentLevel].AssignState(ButtonPressDetector.ButtonState.None, true);
        }

        /// <summary>
        /// Sets the selected level and patient case in the game state and transitions to the game menu.
        /// </summary>
        // Call on "commencer" button click by player
        private void LoadPatientCase(){
            // SET Game state level and patient case
            GameManager.Instance.GameStateManager.SetLevel(_currentIndexLevelSelected);
            GameManager.Instance.GameStateManager.SetPatientCase(_currentIndexPatientCase,
                levelsData.patientByLevel[_currentIndexLevelSelected].patientsCase[_currentIndexPatientCase]);
            // Re-load game menus (need to change patient sprite)
            GameManager.Instance.LoadGameMenu();
        }
    }
}