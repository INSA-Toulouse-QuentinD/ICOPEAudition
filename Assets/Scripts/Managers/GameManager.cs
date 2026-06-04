using System.Collections;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.UI;
using PatientData.AlgoData;
using UI.ScoreContents;
using PatientData;

namespace Managers{
    [RequireComponent(typeof(GameStateManager))]
    [RequireComponent(typeof(AudioManager))]
    [RequireComponent(typeof(PatientAnimation))]
    public class GameManager : MonoBehaviour{
        #region Event System

        public event System.Action<int, int> OnMoneyChanged;
        public event System.Action<bool, bool> OnAssistantDisabled;

        #endregion

        public static GameManager Instance;
        public GameStateManager GameStateManager{ get; private set; }
        public GameData GameData{ get; set; }
        public AudioManager AudioManager{ get; private set; }
        private PatientAnimation PatientAnimation{ get; set; }
        private StepManagerN StepManagerN{ get; set; }

        #region Structures

        public int Money{
            get => money;
            set{
                int previousMoney = money;
                money = value;
                PlayerPrefs.SetInt("money", money);
                OnMoneyChanged?.Invoke(money, money - previousMoney);
            }
        }

        public bool IsTutorialEnable{
            get => isTutorialEnable;
            set{
                bool isEnable = isTutorialEnable;
                isTutorialEnable = value;
                PlayerPrefs.SetInt("enableTutorial", isTutorialEnable ? 1 : 0);
                OnAssistantDisabled?.Invoke(isTutorialEnable, isEnable);
            }
        }

        #endregion

        #region Configurable Attributes

        [Header("Debug")] public bool canAccessAllLevel;
        public bool instanteAnimation;
        public bool skipAssistante;
        public bool alwaysRight;
        public bool forcePositionChoice;

        [Header("Levels")] [SerializeField] public LevelsData levelsData;

        [Header("Tutoriel")] [SerializeField] private bool isTutorialEnable;

        private readonly string _pathXmlFile = "Assets/Resources/Data/Tutorial.xml";
        private readonly string _pathXsdFile = "Assets/Resources/Data/TutorialSchema.xsd";
        [Header("Money")] [SerializeField] private int money;

        [Header("Menus")] [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject gameMenu;
        [SerializeField] private GameObject stepMenu;
        [SerializeField] private GameObject isTutorialActive;
        [SerializeField] private GameObject scorePanel;
        [SerializeField] private GameObject pauseButton;
        [SerializeField] private GameObject levelButton;
        [SerializeField] private GameObject shopButton;

        [Header("Panels")] [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject settingsPanelCheckbox;
        [SerializeField] public GameObject tutorialPanel;

        [Header("Scripts")] [SerializeField] private PlayerScoreDisplayManager folderDivider;

        [Header("Background")] [SerializeField]
        private List<Sprite> listBackground;

        [SerializeField] private Image stepBackground;
        [SerializeField] private Image scoreBackground;

        #endregion

        #region Private variables

        private bool _isTutorialUIEnable;
        private bool _tutorialAnswer = true;

        #endregion

        #region Internal methods

        // LOAD STEP
        internal void LoadStep(Step stepIndex){
            var currentLevel = GameStateManager.GetCurrentLevel();
            var currentPatient = GameStateManager.GetCurrentPatientCase();

            if (stepIndex == 0) {
                AudioManager.PlayBGM("tense_dark");
                AudioManager.StopCurrentSfx();
                ClearScreen();
                stepMenu.SetActive(true);
                StepManagerN.Initialize(levelsData.patientByLevel[currentLevel].patientsCase[currentPatient]);
            }

            StepManagerN.LoadStep(stepIndex);
        }

        internal void LoadScore(string patientName){
            ClearScreen();
            folderDivider.GetSetDisplayScore(patientName);
            scorePanel.SetActive(true);
        }

        // CLEAR SCREEN
        private void ClearScreen(){
            mainMenu.SetActive(false);
            gameMenu.SetActive(false);
            stepMenu.SetActive(false);
            scorePanel.SetActive(false);
            tutorialPanel.SetActive(false);
            isTutorialActive.SetActive(false);
            PatientAnimation.imageRingtone.gameObject.SetActive(false);
        }

        // CLEAR ANIMATION
        private static void ClearAnimation(){
            PatientAnimation.StopAnimation();
        }

        // LOAD MAIN MENU
        internal void LoadMainMenu(){
            AudioManager.StopCurrentSfx();
            ClearScreen();
            mainMenu.SetActive(true);
            pauseButton.SetActive(false);
        }

        // LOAD GAME MENU
        internal void LoadGameMenu(){
            AudioManager.PlaySFX("ambiant", "AMBIANT");
            ClearScreen();
            ClearAnimation();
            gameMenu.SetActive(true);
            levelButton.SetActive(true);
            shopButton.SetActive(true);
            shopPanel.SetActive(false);
            pauseButton.SetActive(true);

            var currentLevel = GameStateManager.GetCurrentLevel();
            var currentPatient = GameStateManager.GetCurrentPatientCase();

            // LOAD SPRITE ON SCREEN
            PatientAnimation.SetCharacterInArea(levelsData.patientByLevel[currentLevel].patientsCase[currentPatient]);

            // SHOW TUTORIAL
            if (skipAssistante) return;

            if (instanteAnimation) {
                EnableTutorial();
            } else {
                Invoke(nameof(EnableTutorial), 3f); // total time during the animation done before
            }
        }

        private void EnableTutorial(){
            if (Instance.GameData.FirstGameSession()) {
                isTutorialActive.SetActive(_tutorialAnswer);
            }
        }

        public void AnswerTutoriel(){
            _tutorialAnswer = false;
        }

        // SAVE BOUGHT ITEM IN PLAYERPREFS
        public static void AddBoughtItem(string name){
            PlayerPrefs.SetString("items", $"{name};{PlayerPrefs.GetString("items", "")}");
            PlayerPrefs.Save();
        }

        #endregion

        #region Main methods

        //LOAD GAME ON GRANDMA CLICK -> CALL LaunchGameAfterTime()
        public void LaunchGame(){
            ClearAnimation();
            StartCoroutine(LaunchGameAfterTime());
        }

        //LOAD GAME ON GRANDMA CLICK -> CHANGE STATE.MANAGER -> LOAD LEVEL 1    
        private IEnumerator LaunchGameAfterTime(){
            if (!instanteAnimation)
                yield return new WaitForSeconds(0.5f);

            GameData.UpdateMainRecordsOnLevelStart();

            var currentLevel = GameStateManager.GetCurrentLevel();
            var currentPatient = GameStateManager.GetCurrentPatientCase();

            if (listBackground.Count > 0) {
                Sprite sprite = listBackground[Random.Range(0, listBackground.Count)];
                stepBackground.sprite = sprite;
                scoreBackground.sprite = sprite;
            }

            // SET ALGO STEP BY DEFAULT LOAD STEP 0 (RESTART THE PARCOURS EVEN IF PLAYER STOP DURING)
            GameStateManager.SetStep(
                levelsData.patientByLevel[currentLevel].patientsCase[currentPatient].steps[0].type);
        }

        // PLAY AUDIO
        public void ClickButton(){
            AudioManager.PlaySFX("ui_click2");
        }

        public void SetCheckBoxSettings(){
            settingsPanelCheckbox.GetComponent<Toggle>().isOn = IsTutorialEnable;
        }

        // ACTIVATE UI TUTORIAL 
        public void SetTutorialUI(){
            _isTutorialUIEnable = !_isTutorialUIEnable;
            if (IsTutorialEnable) {
                tutorialPanel.SetActive(_isTutorialUIEnable);
            }
        }

        #endregion

        #region Initializing methods

        void Awake(){
            if (Instance != null) {
                Destroy(Instance);
                return;
            }

            Instance = this;

            GameStateManager = GetComponent<GameStateManager>();
            GameData = GetComponent<GameData>();
            StepManagerN = GetComponent<StepManagerN>();

            AudioManager = GetComponent<AudioManager>();
            PatientAnimation = GetComponent<PatientAnimation>();

            mainMenu.SetActive(true);
            gameMenu.SetActive(false);
            stepMenu.SetActive(false);
            tutorialPanel.SetActive(true);

            AudioManager.LoopBgm(true);
            AudioManager.LoopSfx(true, "AMBIANT");
        }


        void Start(){
            // Check validity of tutorial XML
            XmlManager.ValidateXML(_pathXmlFile, _pathXsdFile);

            LoadListItems();

            Money = PlayerPrefs.GetInt("money", 0);
            isTutorialEnable = PlayerPrefs.GetInt("enableTutorial") == 1;
            AudioManager.PlayBGM("skyline");

            LoadMainMenu();
        }

        #endregion

        // ON START LOAD ITEM BOUGHT DURING THE LAST SESSION
        private void LoadListItems(){
            Transform items = gameMenu.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(2);
            Transform itemButtons = shopPanel.transform.GetChild(0).GetChild(0).GetChild(1);
            string[] savedItems = PlayerPrefs.GetString("items", "").Split(";");
            
            foreach (string item in savedItems) {
                if (string.IsNullOrEmpty(item)) continue;
                
                Transform loadedItem = items.Find(item);
                if (loadedItem != null) loadedItem.gameObject.SetActive(true);

                loadedItem = itemButtons.Find(item);
                if (loadedItem != null) loadedItem.gameObject.GetComponent<Button>().interactable = false;
            }
        }
    }
}