using PatientData;
using PatientData.AlgoData;
using System.Collections.Generic;
using UnityEngine;

namespace Managers{
    public class GameStateManager : MonoBehaviour{
        // Enums for Main_menu and waiting_room
        private enum GameState{
            MainMenu,
            GameMenu
        } 
        
        // VARIABLES
        private GameState _gameState;
        private int _currentLevel;
        private int _currentPatientCase;
        private Dictionary<int, int> _maxPatientCase;
        private Step _currentStep;

        [HideInInspector] public LevelsData levelsData;
        private PatientCaseLevel _patientCaseLevel;
        private PatientData.PatientData _patientData;
        
        /// <summary>
        /// Initializes the LevelsData reference from the GameManager and sets
        /// the default current level and patient case. These defaults can be
        /// overridden later if a saved game is loaded.
        /// </summary>
        private void Start(){
            // Set by default current level and current patient case (change later if player has a save)
            _currentLevel = 0;
            _currentPatientCase = 0;
            _maxPatientCase = new Dictionary<int, int>();
        }

        // MAIN MENU / GAME MENU TRANSITIONS
        /// <summary>
        /// Function call by buttons (play and return Menu)
        /// Basicly switch between MAIN_MENU and GAME_MENU
        /// </summary>
        /// <param name="newState">MainState: MAIN_MENU / GAME_MENU</param>
        private void SetMainState(GameState newState){
            var currInstance = GameManager.Instance;
            _gameState = newState;
            Debug.Log($"Main State: {_gameState}");
            // LOAD SCENE
            if (_gameState == GameState.GameMenu) {
                currInstance.LoadGameMenu();
                // Set level 
                SetLevel(_currentLevel);
                // Set Patient Case
                SetPatientCase(_currentPatientCase, _patientCaseLevel.patientsCase[(int)_currentPatientCase]);
            } else {
                currInstance.LoadMainMenu();
            }
        }

        /// <summary>
        /// Public function change the main state between MAIN_MENU & GAME_MENU.
        /// Its call by buttons "Play" & "return menu"
        /// </summary>
        public void ChangeMainState(){
            SetMainState(_gameState == GameState.MainMenu ? GameState.GameMenu : GameState.MainMenu);
        }

        /// <summary>
        /// Sets the current level to the given level state,
        /// loads the corresponding patient case data,
        /// and updates the game data with the current level records.
        /// </summary>
        public void SetLevel(int levelState){
            _currentLevel = levelState;
            levelsData = GameManager.Instance.levelsData;
            _patientCaseLevel = levelsData.patientByLevel[levelState];

            GameManager.Instance.GameData.SetLevelRecords(_currentLevel);

            Debug.Log($"Current Level : {_currentLevel}");
        }

        /// <summary>
        /// Returns the current level as an integer.
        /// </summary>
        public int GetCurrentLevel(){
            return _currentLevel;
        }

        /// <summary>
        /// Sets the current patient case and loads the associated patient data.
        /// Updates the game data with the current patient's record.
        /// </summary>
        /// <param name="patientCase">The patient case to set as current.</param>
        /// <param name="patient">The patient data associated with the current patient case.</param>
        public void SetPatientCase(int patientCase, PatientData.PatientData patient){
            _currentPatientCase = patientCase;
            _patientData = patient;

            if (!_maxPatientCase.TryAdd(_currentLevel, _currentPatientCase)) {
                if (_maxPatientCase[_currentLevel] < _currentPatientCase) {
                    _maxPatientCase[_currentLevel] = _currentPatientCase;
                }
            }

            GameManager.Instance.GameData.SetPatientCaseRecorder(_patientData.firstName);

            Debug.Log($"Current Patient: {_currentPatientCase}, {_patientData.lastName}");
        }

        /// <summary>
        /// Returns the current patient case as an integer.
        /// </summary>
        /// <returns>Integer representation of the current patient case.</returns>
        public int GetCurrentPatientCase(){
            return (int)_currentPatientCase;
        }

        /// <summary>
        /// Returns the max patient case the player can do for a level as an integer.
        /// </summary>
        /// <param name="level">Index of the selected level.</param>
        /// <returns>Integer representation of the current patient case.</returns>
        public int GetMaxPatientCase(int level){
            return _maxPatientCase.GetValueOrDefault(level, -1);
        }

        /// <summary>
        /// Sets the current step of the game algorithm.
        /// Updates the game data with the current step records,
        /// then loads the corresponding step content.
        /// </summary>
        /// <param name="step">The step to set as current.</param>
        public void SetStep(Step step){
            _currentStep = step;
            Debug.Log($"Algo Test G: {_currentStep}");

            GameManager.Instance.GameData.SetStepRecords(_currentStep);

            GameManager.Instance.LoadStep(_currentStep);
        }

        public string MessageScoreToShow() {
            if (_currentPatientCase + 1 < _patientCaseLevel.patientsCase.Count) {
                return "Tu as débloqué un nouveau cas.";
            }
            if (_currentLevel + 1 < levelsData.patientByLevel.Count) {
                return "Tu as débloqué un nouveau niveau.";
            }
            return "Tu as terminé tous les cas.";
        }

        /// <summary>
        /// Advances the game to the next level and patient case if available.
        /// If the current level is less than the total number of levels, it sets the current level and patient case,
        /// then returns to the game menu.
        /// Otherwise, it logs that all levels are completed, resets the game to the first level and patient case,
        /// and returns to the game menu (temporary fix).
        /// </summary>
        public void NextLevel(){
            if (_currentLevel < levelsData.patientByLevel.Count) {
                SetLevel(_currentLevel);
                SetPatientCase(_currentPatientCase, _patientCaseLevel.patientsCase[_currentPatientCase]);
                //Return to game menu
                ReturnToGameMenu();
            } else {
                Debug.Log("Tout les niveau sont terminer ! Restart du jeu!");
                // TAMPORARY FIX - Restart the game
                _currentLevel = 0;
                _currentPatientCase = 0;
                //SET RANDOM MOD (load patient in random make list of all patient)
                //Return to game menu
                ReturnToGameMenu();
            }
        }

        /// <summary>
        /// Advances to the next patient case within the current level.
        /// If there are more patient cases available, it sets the next patient case and returns to the game menu.
        /// Otherwise, it resets the patient case to the first one, increments the level,
        /// clears patient case records if necessary, and proceeds to the next level.
        /// </summary>
        public void NextPatientCase(){
            _currentPatientCase++;
            _maxPatientCase[_currentLevel] = _currentPatientCase;
            if (_currentPatientCase < _patientCaseLevel.patientsCase.Count) {
                SetPatientCase(_currentPatientCase, _patientCaseLevel.patientsCase[(int)_currentPatientCase]);
                ReturnToGameMenu();
            } else {
                Debug.Log("Tous les cas patient sont terminer! Next Level !");
                _currentPatientCase = 0; // Reset patient case
                _currentLevel++;
                _maxPatientCase[_currentLevel] = 0;
                // Clear patient case records                 
                NextLevel();
            }
        }

        /// <summary>
        /// Advances to the next step based on the provided AlgoStep.
        /// Updates the current step and triggers the step setup.
        /// </summary>
        /// <param name="step">The next AlgoStep containing the step type to set.</param>
        public void NextStep(AlgoStep step){
            _currentStep = step.type;
            Debug.Log("Next step: " + _currentStep);
            SetStep(_currentStep);
        }

        /// <summary>
        /// Called when a patient case is completed.
        /// Saves the current player data and loads the score summary screen.
        /// </summary>
        public void SaveShowScores(){
            Debug.Log("Patient completed ! ");
            // Saving player data
            SavePlayerData();
            // Load resume screen
            GameManager.Instance.LoadScore(_patientData.firstName);
        }

        /// <summary>
        /// Saves the players progress related to the current patient case,
        /// updates the level records and main game records at the end of the level.
        /// </summary>
        private void SavePlayerData(){
            GameManager.Instance.GameData.RecordsPatientCase(_patientData.firstName);
            GameManager.Instance.GameData.RecordsLevel(_currentLevel);
            GameManager.Instance.GameData.UpdateMainRecordsOnLevelEnd();
        }

        // CALL FORM 'PatientScoreManager' BY 'GoToMenu' FUNCTION
        /// <summary>
        /// Returns the player to the main game menu (patient selection screen)
        /// and plays the background music "skyline".
        /// </summary>
        private static void ReturnToGameMenu(){
            //return Game menu selection patient
            GameManager.Instance.LoadGameMenu();
            GameManager.Instance.AudioManager.PlayBGM("skyline");
        }

        /// <summary>
        /// Loads the saved player state by setting the current level and patient case.
        /// If the saved patient case is not the last in the current level, 
        /// it advances to the next patient case. Otherwise, it moves to the next level.
        /// </summary>
        /// <param name="savedLevelState">The saved level state to load.</param>
        /// <param name="savedPatientCase">The saved patient case to load.</param>
        public void LoadPlayerSaveStates(int savedLevelState, int savedPatientCase){
            if (savedLevelState <= GameManager.Instance.levelsData.patientByLevel.Count) {
                _currentLevel = savedLevelState;

                // WARNING : if cond not good
                if (savedPatientCase <
                    GameManager.Instance.levelsData.patientByLevel[_currentLevel].patientsCase.Count - 1) {
                    _currentPatientCase = savedPatientCase + 1;
                } else {
                    // TO CHANGE : Load next level & patientCase = 0
                    _currentPatientCase = savedPatientCase;
                    _currentLevel++;
                }
            }

            Debug.Log($"Next level played : {_currentLevel}, Next patient played: {_currentPatientCase}");
        }
    }
}