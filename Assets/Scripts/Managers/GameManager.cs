using System.Collections;
using Audio;
using UnityEngine;
using UnityEngine.UI;
using PatientData.AlgoData;
using UI.ScoreContents;
using PatientData;

namespace Managers
{
    [RequireComponent(typeof(GameStateManager))]
    [RequireComponent(typeof(AudioManager))]
    [RequireComponent(typeof(PatientAnimation))]
    public class GameManager : MonoBehaviour
    {
        #region Event System
        public event System.Action<int, int> OnMoneyChanged;
        public event System.Action<bool, bool> OnAssistantDisabled;
        #endregion

        public static GameManager Instance;
        public GameStateManager GameStateManager { get; private set; }
        public GameData GameData { get; set; }
        public AudioManager AudioManager { get; private set; }
        public PatientAnimation PatientAnimation { get; private set; }
        public StepManagerN StepManagerN { get; private set; }


        #region Structures
        public int Money
        {
            get
            {
                return _money;
            }
            set
            {
                int previousMoney = _money;
                _money = value;
                PlayerPrefs.SetInt("money", _money);
                OnMoneyChanged?.Invoke(_money, _money - previousMoney);
            }
        }

        public bool IsTutorialEnable
        {
            get { return _isTutorialEnable; }
            set
            {
                bool _isEnable = _isTutorialEnable;
                _isTutorialEnable = value;
                PlayerPrefs.SetInt("enableTutorial", _isTutorialEnable ? 1 : 0);
                OnAssistantDisabled?.Invoke(_isTutorialEnable, _isEnable);
            }
        }
        #endregion

        #region Configurable Attributes
        [Header("Debug")]
        public bool canAccessAllLevel;
        public bool instanteAnimation;
        
        [Header("Levels")]
        [SerializeField] public LevelsData LevelsData;

        [Header("Tutoriel")]
        [SerializeField] private bool _isTutorialEnable = true; // Par défault true car on suppose que le joueur y joue pour la première fois.
        public readonly string _pathXmlFile = "Assets/Resources/Data/Tutorial.xml";
        private readonly string _pathXsdFile = "Assets/Resources/Data/TutorialSchema.xsd";
        [Header("Money")]
        [SerializeField] private int _money = 20;

        [Header("Menus")]
        [SerializeField] private GameObject _mainMenu;
        [SerializeField] private GameObject _gameMenu;
        [SerializeField] private GameObject _stepMenu;
        [SerializeField] private GameObject _isTutoriaActive;
        [SerializeField] private GameObject _scorePanel;
        [SerializeField] private GameObject _levelButton;
        [SerializeField] private GameObject _shopButton;

        [Header("Panels")]
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _shopPanel;
        [SerializeField] private GameObject _settingsPanelCheckbox;
        [SerializeField] public GameObject _tutorialPanel;

        [Header("Character pluse")]
        [SerializeField] private GameObject _elderPerson; // maybe change to list

        [Header("Scrpits")]
        [SerializeField] private PlayerScoreDisplayManager folderDivider;
        #endregion

        #region Private variables
        private bool isTutorialUIEnable = false;
        private bool tutorialAnswer = true;
        #endregion

        #region Internal methods
        // LOAD STEP
        internal void LoadStep(Step stepIndex)
        {
            var currentLevel = GameStateManager.GetCurrentLevel();
            var currentPatient = GameStateManager.GetCurrentPatientCase();

            if (stepIndex == 0)
            {
                AudioManager.PlayBGM("tense_dark");
                AudioManager.StopCurrentSfx();
                ClearScreen();
                _stepMenu.SetActive(true);                
                StepManagerN.Initialize(LevelsData.patientByLevel[currentLevel].patientsCase[currentPatient]);
            }
            StepManagerN.LoadStep(stepIndex);
        }

        internal void LoadScore(string patientName)
        {
            ClearScreen();
            folderDivider.GetSetDisplayScore(patientName);
            _scorePanel.SetActive(true);
        }

        // CLEAR SCREEN
        internal void ClearScreen()
        {
            _mainMenu.SetActive(false);
            _gameMenu.SetActive(false);
            _stepMenu.SetActive(false);
            _scorePanel.SetActive(false);
            _isTutoriaActive.SetActive(false);
        }

        // CLEAR ANIMATION
        internal static void ClearAnimation()
        {
            PatientAnimation.StopAnimation();
        }

        // LOAD MAIN MENU
        internal void LoadMainMenu()
        {
            AudioManager.StopCurrentSfx();
            ClearScreen();
            _mainMenu.SetActive(true);
        }
        // LOAD GAME MENU
        internal void LoadGameMenu()
        {
            AudioManager.PlaySFX("ambiant", "AMBIANT");
            ClearScreen();
            ClearAnimation();
            _gameMenu.SetActive(true);
            _levelButton.SetActive(true);
            _shopButton.SetActive(true);
            _shopPanel.SetActive(false);

            var currentLevel = GameStateManager.GetCurrentLevel();
            var currentPatient = GameStateManager.GetCurrentPatientCase();

            // LOAD SPRITE ON SCREEN (BY DEFAULT SPRITE 0 MUST A STAND CHARACTER) 
            PatientAnimation.SetNewCharacterInArea(LevelsData.patientByLevel[currentLevel].patientsCase[currentPatient].characterSprites[0]);
            // SHOW TUTORIAL
            if (!instanteAnimation) {
                Invoke(nameof(EnableTutorial), 4.5f); // total time during the animation done before
            }
        }
        
        private void EnableTutorial()
        {
            if (Instance.GameData.FirstGameSession()) {
                _isTutoriaActive.SetActive(tutorialAnswer);
            } 
        }

        public void AnswerTutoriel(){
            tutorialAnswer = false;
        }

        // SAVE BOUGHT ITEM IN PLAYERPREFS
        public static void AddBoughtItem(string name)
        {
            PlayerPrefs.SetString("items", $"{name};{PlayerPrefs.GetString("items", "")}");
            PlayerPrefs.Save();
        }
        #endregion

        #region Main methods

        //LOAD GAME ON GRANDMA CLICK -> CALL LaunchGameAfterTime()
        public void LaunchGame()
        {
            ClearAnimation();
            StartCoroutine(LaunchGameAfterTime());
        }

        //LOAD GAME ON GRANDMA CLICK -> CHANGE STATE.MANAGER -> LOAD LEVEL 1    
        private IEnumerator LaunchGameAfterTime()
        {
            if (!instanteAnimation)
                yield return new WaitForSeconds(0.5f);
            
            GameData.UpdateMainRecordsOnLevelStart();

            var currentLevel = GameStateManager.GetCurrentLevel();
            var currentPatient = GameStateManager.GetCurrentPatientCase();

            // SET ALGO STEP BY DEFAULT LOAD STEP 0 (RESTART THE PARCOURS EVEN IF PLAYER STOP DURING)
            GameStateManager.SetStep(LevelsData.patientByLevel[currentLevel].patientsCase[currentPatient].steps[0].type);
        }

        // PLAY AUDIO
        public void ClickButton()
        {
            AudioManager.PlaySFX("ui_click2");
        }


        // PLAY AUDIO ON GRANDPA CLICK
        public void PlayBonjour()
        {
            AudioManager.PlaySFX("bonjour");
        }

        public void SetCheckBoxSettings()
        {
            _settingsPanelCheckbox.GetComponent<Toggle>().isOn = IsTutorialEnable;
        }

        // ACTIVATE UI TUTORIAL 
        public void SetTutorialUI()
        {
            isTutorialUIEnable = !isTutorialUIEnable;
            if (IsTutorialEnable)
            {
                _tutorialPanel.SetActive(isTutorialUIEnable);
            }
        }


        #endregion

        #region Initializing methods
        void Awake()
        {
            if (Instance != null)
            {
                Destroy(Instance);
                return;
            }
            Instance = this;

            GameStateManager = GetComponent<GameStateManager>();
            GameData = GetComponent<GameData>();
            StepManagerN = GetComponent<StepManagerN>();
            
            AudioManager = GetComponent<AudioManager>();
            PatientAnimation = GetComponent<PatientAnimation>();
            
            _mainMenu.SetActive(true);
            _gameMenu.SetActive(false);

            AudioManager.LoopBgm(true);
            AudioManager.LoopSfx(true, "AMBIANT");
        }


        void Start()
        {
            // Check validity of tutorial XML
            XmlManager.ValidateXML(_pathXmlFile, _pathXsdFile);
            
            LoadListItems();

            _money = PlayerPrefs.GetInt("money", 20);
            _isTutorialEnable = PlayerPrefs.GetInt("enableTutorial") == 1;
            AudioManager.PlayBGM("skyline");
        }
        #endregion

        // ON START LOAD ITEM BOUGHT DURING THE LAST SESSION
        private void LoadListItems()
        {
            // TOO CHANGE - LATER
            Transform items = _gameMenu.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(1);

            string[] savedItems = PlayerPrefs.GetString("items", "").Split(";");
            for (int i = 0; i < items.childCount; i++)
            {
                foreach (string item in savedItems)
                {
                    Transform loadedItem = items.GetChild(i);
                    if (item.Equals(loadedItem.name)) loadedItem.gameObject.SetActive(true);
                }
            }

            Transform itemButtons = _shopPanel.transform.GetChild(0).GetChild(0).GetChild(1);
            for (int i = 0; i < itemButtons.childCount; i++)
            {
                foreach (string item in savedItems)
                {
                    Transform loadedItem = itemButtons.GetChild(i);
                    if (item.Equals(loadedItem.name)) loadedItem.gameObject.GetComponent<Button>().interactable = false;
                }
            }
        }
    }
}