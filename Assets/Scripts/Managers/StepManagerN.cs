using PatientData;
using PatientData.AlgoData;
using PatientData.Steps;
using System.Collections.Generic;
using TMPro;
using UI.Buttons;
using UI.TutorialContents;
using UnityEngine;
using UnityEngine.UI;

namespace Managers{
    public class StepManagerN : MonoBehaviour{
        [SerializeField] private Button returnButton;
        [SerializeField] private Button confirmNextButton;

        // GameObject
        [Header("Steps gameObject")] [SerializeField]
        private GameObject patientDisplay;

        [SerializeField] private List<GameObject> displayList;

        [Header("Question gameObject")] [SerializeField]
        private GameObject questionsDisplay;

        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private Button[] choiceButtons;

        [Header("Correction gameObject")] [SerializeField]
        private GameObject correctionDisplay;

        [SerializeField] private TextMeshProUGUI answerText;
        [SerializeField] private TextMeshProUGUI answerSelected;
        [SerializeField] private GameObject answerJustification;
        [SerializeField] private List<GameObject> answerGameObjectSprites;

        [Header("Colors answer")] [SerializeField]
        private Color correctColor;

        [SerializeField] private Color incorrectColor;
        [SerializeField] private Image backgroundAnswer;

        // Sprite Doctor (1st position happy expression, 2nd position sad expression, 3rd position talking)
        // For the future to change to allow player to choose his character.
        [Header("Sprite doctor")] [SerializeField]
        private Sprite[] doctorSprite;

        [Header("Phone (case presentation)")] [SerializeField]
        private Sprite phoneCaseSprite;

        [Header("GameObject Image correction")] [SerializeField]
        private Image doctorExpressionsImages;

        [Header("Questionnary")] [SerializeField]
        private QuestionnaireData questionnaireData;

        // Script of each step display
        private Step1PresentationPatient _step1PresentationPatient;
        private Step2WisperTest _step2WisperTest;
        private Step3And4Questionnary _step4And5Questionnary;
        private Step5Otoscopie _step5Otoscopie;
        private Step6WeberTest _step6HhiesTest;
        private Step7HhiesTest _step7HhiesTest;
        private Step8Audiometrie _step8Audiometrie;

        // Enums
        private enum InteractionState{
            ISREADING,
            ISANSWERING,
            ISCORRECTION
        };

        private InteractionState _interactionState;

        private enum AnswerState{
            DIAGNOSTIC,
            ACTION
        }

        private AnswerState _answerState;

        // Private class 
        private NewPatientData _patientData;

        // Private variables
        private int _indexStep;
        private int _currentDisplay;
        private bool _isDiagnosticValid;
        private bool _isActionValid;
        private Step _step;

        private Dictionary<Step, int> _mappingDisplays;

        // UI state (simple): keep which answers have already been tried and were incorrect
        // so that "Précédent" (correction -> questions) does not reset buttons.
        private readonly List<bool> _diagIncorrectTried = new();
        private readonly List<bool> _actionIncorrectTried = new();

        /// <summary>
        /// Initializes the patient data and resets the step index and display index
        /// to start fresh for a new patient case.
        /// </summary>
        /// <param name="newPatient">The new patient data to initialize.</param>
        public void Initialize(NewPatientData newPatient){
            _indexStep = 0; // reset current step to 0
            _currentDisplay = 0;
            _patientData = newPatient;

            ResetTriedState();
        }

        private void ResetTriedState(){
            _diagIncorrectTried.Clear();
            _actionIncorrectTried.Clear();
        }

        private static void EnsureSize(List<bool> list, int size){
            if (size < 0) size = 0;

            while (list.Count < size) list.Add(false);
            if (list.Count > size) list.RemoveRange(size, list.Count - size);
        }

        /// <summary>
        /// Loads the specified step by updating the UI elements and setting the interaction state.
        /// Clears all previous displays and activates the relevant display based on the current step.
        /// Resets validation flags and sets the current display index accordingly.
        /// </summary>
        /// <param name="currentStep">The step to load and display.</param>
        public void LoadStep(Step currentStep){
            ClearAllDisplay();

            _step = currentStep;

            _interactionState = InteractionState.ISREADING;

            // Set bool to false each step
            _isDiagnosticValid = false;
            _isActionValid = false;
            ResetTriedState();
            _currentDisplay = _mappingDisplays[currentStep];

            Sprite patientSprite = null;

            switch (currentStep) {
                case Step.CasePresentation:
                    // Load patient sprite & patient text
                    _step1PresentationPatient.SetSprites(
                        _patientData.isOnPhone ? phoneCaseSprite : _patientData.characterSprites[0]);
                    _step1PresentationPatient.SetPresentationTexts(_patientData);
                    // Display current step

                    displayList[_currentDisplay].SetActive(true);

                    UITutorialControler.Instance.TutorialMichel(0);
                    break;
                case Step.WisperTest:
                    // Load wisper text
                    if (_patientData.characterSprites.Length > 1) {
                        patientSprite = _patientData.characterSprites[1];
                    }

                    _step2WisperTest.SetPatient(patientSprite);
                    if (GameManager.Instance.instanteAnimation) {
                        _step2WisperTest.SkipAnimation(_patientData.steps[_indexStep]);
                    } else {
                        _step2WisperTest.PlayFirstText(_patientData.steps[_indexStep]);
                    }

                    // Display current step
                    displayList[_currentDisplay].SetActive(true);

                    UITutorialControler.Instance.TutorialMichel(4);
                    break;
                case Step.Questionnary:
                    // Load questionary & answer
                    List<QuestionData> questions = questionnaireData.questions;
                    List<YesNo> answers = _patientData.steps[_indexStep].predefinedAnswer;
                    // Set texts
                    _step4And5Questionnary.SetQuestionayText(questions, answers);
                    //Set Patient Sprite
                    _step4And5Questionnary.SetPatientSprite(_patientData.characterSprites[0]);
                    // Display current step
                    displayList[_currentDisplay].SetActive(true);

                    UITutorialControler.Instance.TutorialMichel(6);
                    break;
                case Step.Otoscopy:
                    // Load patient ear image
                    if (_patientData.characterSprites.Length > 1) {
                        patientSprite = _patientData.characterSprites[1];
                    }

                    _step5Otoscopie.SetImages(_patientData.steps[_indexStep].spriteEarExams, patientSprite);
                    // Display current step 
                    displayList[_currentDisplay].SetActive(true);

                    UITutorialControler.Instance.TutorialMichel(7);
                    break;
                case Step.WeberTest:
                    // Load texts dialogue & sprite
                    _step6HhiesTest.SetTextDialogue(_patientData.steps[_indexStep].dialoguePatient);
                    _step6HhiesTest.SetImage(_patientData.characterSprites[^1]);
                    // Display current step
                    displayList[_currentDisplay].SetActive(true);

                    UITutorialControler.Instance.TutorialMichel(8);
                    break;
                case Step.HhiesTest:
                    // Load patient ear image
                    _step7HhiesTest.SetImages(_patientData.steps[_indexStep].spriteEarExams,
                        _patientData.characterSprites[0]);
                    // Display current step
                    displayList[_currentDisplay].SetActive(true);

                    UITutorialControler.Instance.TutorialMichel(9);
                    break;
                case Step.Audiometry:
                    // Load patient audiometrie + patient sprite
                    _step8Audiometrie.SetSprite(_patientData.steps[_indexStep].spriteEarExams,
                        _patientData.characterSprites[0]);
                    // Display current step
                    displayList[_currentDisplay].SetActive(true);

                    UITutorialControler.Instance.TutorialMichel(10);
                    break;
            }

            // set navigation button (Buttons)
            SetTextButtonsNavigation();
        }

        // SET TEXT AND INTERACTION
        /// <summary>
        /// Updates the navigation buttons' interactability and text based on the current interaction state.
        /// - In ISREADING state: disables the return button, enables the confirm/next button with text "Répondre".
        /// - In ISANSWERING state: enables the return button, disables the confirm/next button with text "Suivant".
        /// - In ISCORRECTION state: 
        ///     - Checks if the current answer is valid (diagnostic or action).
        ///     - If valid, enables confirm/next button and disables return button, setting confirm button text to "Suivant".
        ///     - If not valid, disables confirm/next button and enables return button.
        /// </summary>
        private void SetTextButtonsNavigation(){
            TextMeshProUGUI confirmeNextText = confirmNextButton.GetComponentInChildren<TextMeshProUGUI>();

            if (_interactionState == InteractionState.ISREADING) {
                // update buttons and text
                returnButton.interactable = false;
                confirmNextButton.interactable = true;
                confirmeNextText.text = "Répondre";
            }

            if (_interactionState == InteractionState.ISANSWERING) {
                // Update button and text
                returnButton.interactable = true;

                confirmNextButton.interactable = false;
                confirmeNextText.text = "Suivant";
            }

            if (_interactionState == InteractionState.ISCORRECTION) {
                bool isValid = false;
                switch (_answerState) {
                    case AnswerState.DIAGNOSTIC:
                        isValid = _isDiagnosticValid;
                        break;
                    case AnswerState.ACTION:
                        isValid = _isActionValid;
                        break;
                }

                if (isValid || GameManager.Instance.alwaysRight) {
                    confirmeNextText.text = "Suivant";
                    returnButton.interactable = GameManager.Instance.alwaysRight;
                    confirmNextButton.interactable = true;
                    confirmNextButton.gameObject.SetActive(true);
                } else {
                    confirmNextButton.interactable = false;
                    returnButton.interactable = true;
                }
            }
        }

        /// <summary>
        /// Sets up the current response buttons based on the patient's current step data.
        /// - If the step has a diagnostic phase and the diagnostic answer is not yet valid,
        ///   it sets the answer state to DIAGNOSTIC and creates the corresponding answer buttons.
        /// - If the diagnostic phase is already valid, it marks diagnostic as valid.
        /// - If the step has an action phase and the diagnostic phase is valid,
        ///   it sets the answer state to ACTION and creates the corresponding action answer buttons.
        /// </summary>
        private void SetResponses(){
            AlgoStep algoStep = _patientData.steps[_indexStep];
            int diagCount = algoStep.diagnosticPhase.Count;
            int actionCount = algoStep.actionPhase.Count;

            // If no diagnostic phase exists, consider it completed to allow action.
            if (diagCount == 0) {
                _isDiagnosticValid = true;
            }

            if (diagCount > 0 && !_isDiagnosticValid) {
                questionText.text = "Quel est votre diagnostic ?";
                _answerState = AnswerState.DIAGNOSTIC;
                EnsureSize(_diagIncorrectTried, diagCount);
                CreateAnswerButtons(algoStep.diagnosticPhase);
                ApplyTriedStateToButtons(_diagIncorrectTried);
                return;
            }

            if (actionCount > 0 && _isDiagnosticValid) {
                questionText.text = "Que faites-vous ?";
                _answerState = AnswerState.ACTION;
                EnsureSize(_actionIncorrectTried, actionCount);
                CreateAnswerButtons(algoStep.actionPhase);
                ApplyTriedStateToButtons(_actionIncorrectTried);

                if (_step == Step.WisperTest) {
                    UITutorialControler.Instance.TutorialMichel(5);
                }
            }

            UITutorialControler.Instance.TutorialMichel(1);
        }

        private void ApplyTriedStateToButtons(List<bool> triedIncorrect){
            int max = Mathf.Min(choiceButtons.Length, triedIncorrect.Count);
            for (int i = 0; i < max; i++) {
                if (!choiceButtons[i].gameObject.activeSelf) continue;
                if (!triedIncorrect[i]) continue;

                choiceButtons[i].GetComponent<AnswerButton>().SetIncorrect();
                choiceButtons[i].interactable = false;
            }
        }

        /// <summary>
        /// Clears existing answer buttons and creates new ones based on the provided phase data.
        /// For each answer in the phase data (up to the number of available buttons):
        /// - Sets the button text to the answer's text.
        /// - Resets the button state.
        /// - Adds a click listener that triggers OnAnswerCorrect with the corresponding answer index.
        /// - Enables and makes the button visible.
        /// </summary>
        /// <param name="answerData">List of possible answers for the current phase.</param>
        private void CreateAnswerButtons(List<AnswerData> answerData){
            ClearQuestion();

            int max = Mathf.Min(answerData.Count, choiceButtons.Length);

            for (int i = 0; i < max; i++) {
                int index = i;
                TextMeshProUGUI text = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                text.text = answerData[i].answerText;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].GetComponent<AnswerButton>().Reset();
                choiceButtons[i].onClick.AddListener(() => OnAnswerCorrect(answerData, index));
                choiceButtons[i].interactable = true;
                choiceButtons[i].enabled = true;
                choiceButtons[i].gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Advances the game to the next step if the current step's diagnostic and action phases are completed.
        /// If both phases are completed, the current step is the last, it displays the player's scores.
        /// If the step is not completed, switches the UI to the question answering display
        /// </summary>
        private void ButtonNext(){
            TipsManager.Instance.HideButton();
            
            // Control if diagnostic & action is completed
            AlgoStep algoStep = _patientData.steps[_indexStep];
            bool isDiagnosticCompleted = !(algoStep.diagnosticPhase.Count > 0) || _isDiagnosticValid;
            bool isActionCompleted = !(algoStep.actionPhase.Count > 0) || _isActionValid;
            bool isStepCompleted = isDiagnosticCompleted && isActionCompleted;

            if (isStepCompleted && _indexStep < _patientData.steps.Count) {
                // LOAD NEXT STEP 
                _indexStep++;
                if (_indexStep >= _patientData.steps.Count) {
                    GameManager.Instance.GameStateManager.SaveShowScores();

                    UITutorialControler.Instance.TutorialMichel(11);
                } else {
                    print("Increased indexStep : " + _indexStep);
                    GameManager.Instance.GameStateManager.NextStep(_patientData.steps[_indexStep]);
                }
            } else {
                _interactionState = InteractionState.ISANSWERING;
                ClearAllDisplay();
                questionsDisplay.SetActive(true);
                SetResponses();
                SetTextButtonsNavigation();
            }
        }

        /// <summary>
        /// Returns from the correction/answering state back to the question answering/document (reading) display
        /// clearing all displays and resetting the interaction state and buttons accordingly.
        /// </summary>
        private void ButtonBack(){
            TipsManager.Instance.HideButton();
            
            if (_interactionState == InteractionState.ISCORRECTION) {
                if (_answerState == AnswerState.DIAGNOSTIC) {
                    _isDiagnosticValid = false;
                } else {
                    _isActionValid = false;
                }
                
                ClearAllDisplay();

                _interactionState = InteractionState.ISANSWERING;
                questionsDisplay.SetActive(true);
                // Recreate buttons and re-apply previous incorrect choices for this step/phase.
                SetResponses();
                SetTextButtonsNavigation();
            } else if (_interactionState == InteractionState.ISANSWERING) {
                ClearAllDisplay();

                _interactionState = InteractionState.ISREADING;
                displayList[_currentDisplay].SetActive(true);
                SetTextButtonsNavigation();
            }
        }

        /// <summary>
        /// Deactivates all UI displays including the general display list,
        /// the question display, and the correction display.
        /// Used to reset the UI before showing new content.
        /// </summary>
        private void ClearAllDisplay(){
            foreach (var t in displayList) {
                t.SetActive(false);
            }

            questionsDisplay.SetActive(false);
            correctionDisplay.SetActive(false);
        }

        /// <summary>
        /// Deactivates all answer choice buttons.
        /// Clears the current questions answer options from the UI.
        /// </summary>
        private void ClearQuestion(){
            foreach (var t in choiceButtons) {
                t.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Handles the logic when the player selects an answer button.
        /// Checks if the selected answer is correct and updates the state accordingly.
        /// Provides feedback by marking incorrect answers and displaying messages.
        /// Records the answer in the game data for persistence.
        /// </summary>
        /// <param name="answerData">List of possible answers for the current phase.</param>
        /// <param name="index">The index of the selected answer button.</param>
        private void OnAnswerCorrect(List<AnswerData> answerData, int index){
            bool isDiagnosticAnswer = false;
            bool isCorrectAnswer = IsAnswerCorrect(answerData, index);
            string feedBackText = isCorrectAnswer ? "Bonne réponse !" : "Mauvaise réponse !";

            if (!isCorrectAnswer) {
                choiceButtons[index].GetComponent<AnswerButton>().SetIncorrect();

                // Remember this incorrect attempt so that "Précédent" won't reset it.
                if (_answerState == AnswerState.DIAGNOSTIC) {
                    TipsManager.Instance.InitTips(_patientData.steps[_indexStep].diagnosticPhase[index].rappelTip);
                    EnsureSize(_diagIncorrectTried, answerData.Count);
                    if (index >= 0 && index < _diagIncorrectTried.Count) _diagIncorrectTried[index] = true;
                } else {
                    TipsManager.Instance.InitTips(_patientData.steps[_indexStep].actionPhase[index].rappelTip);
                    EnsureSize(_actionIncorrectTried, answerData.Count);
                    if (index >= 0 && index < _actionIncorrectTried.Count) _actionIncorrectTried[index] = true;
                }

                choiceButtons[index].interactable = false;
            }

            switch (_answerState) {
                case AnswerState.DIAGNOSTIC:
                    _isDiagnosticValid = isCorrectAnswer || GameManager.Instance.alwaysRight;
                    isDiagnosticAnswer = true;
                    break;
                case AnswerState.ACTION:
                    _isActionValid = isCorrectAnswer || GameManager.Instance.alwaysRight;
                    break;
            }

            Debug.Log("Saving step...");

            GameManager.Instance.GameData.RecordsSteps(_step, isDiagnosticAnswer,
                choiceButtons[index].GetComponentInChildren<TextMeshProUGUI>().text);
            ShowAnswerDetail(answerData[index], feedBackText, isCorrectAnswer);

            UITutorialControler.Instance.TutorialMichel(isCorrectAnswer ? 3 : 2);
        }

        /// <summary>
        /// Checks if the answer at the specified index in the answer list is correct.
        /// </summary>
        /// <param name="answerData">List of possible answers.</param>
        /// <param name="index">Index of the selected answer.</param>
        /// <returns>True if the answer is correct; otherwise, false.</returns>
        private static bool IsAnswerCorrect(List<AnswerData> answerData, int index){
            if (index < 0 || index >= answerData.Count) {
                Debug.LogError("index out of bound");
                return false;
            }

            return answerData[index].isCorrect;
        }

        /// <summary>
        /// Displays detailed feedback for the selected answer, including text, justification, 
        /// related images, and visual cues indicating whether the answer was correct or incorrect.
        /// Also updates UI states and navigation buttons accordingly.
        /// </summary>
        /// <param name="answer">The answer data containing text, correction, and images.</param>
        /// <param name="feedBackText">Feedback message to display ("Correct!" or "Incorrect!").</param>
        /// <param name="answerCorrect">Indicates if the selected answer was correct.</param>
        private void ShowAnswerDetail(AnswerData answer, string feedBackText, bool answerCorrect){
            _interactionState = InteractionState.ISCORRECTION;

            // Clear images
            foreach (GameObject go in answerGameObjectSprites) {
                go.SetActive(false);
            }

            // Load texts
            answerText.text = feedBackText;
            answerSelected.text = answer.answerText;

            // Set background color
            if (answerCorrect) {
                backgroundAnswer.color = correctColor;
                doctorExpressionsImages.sprite = doctorSprite[0]; // Happy expression
            } else {
                backgroundAnswer.color = incorrectColor;
                doctorExpressionsImages.sprite = doctorSprite[1]; // Sad expression
            }

            // Load correction text if not null
            if (answer.correctionText != "") {
                answerJustification.GetComponent<TextMeshProUGUI>().text =
                    "<u><b>Justification :</b></u> " + answer.correctionText;
            }

            // Load image if not null
            if (answer.sprites.Count > 0) {
                int max = Mathf.Min(answer.sprites.Count, answerGameObjectSprites.Count);
                for (int i = 0; i < max; i++) {
                    answerGameObjectSprites[i].GetComponent<Image>().sprite = answer.sprites[i];
                    // Set gameObject actif
                    answerGameObjectSprites[i].SetActive(true);
                }
            }

            // Set GameObject active
            questionsDisplay.SetActive(false);
            correctionDisplay.SetActive(true);

            // Set bottom buttons
            SetTextButtonsNavigation();
        }

        /// <summary>
        /// Initializes component references by searching through the displayList GameObjects,
        /// safely assigning components if they exist without causing errors.
        ///
        /// Also sets up button listeners for navigation and interaction,
        /// and initializes a dictionary mapping each Step enum to its corresponding display index.
        /// </summary>
        private void Awake(){
            // WARNING : don't trigger error if component not found. 
            foreach (GameObject go in displayList) {
                if (go.TryGetComponent(out Step1PresentationPatient component))
                    _step1PresentationPatient = component;
                if (go.TryGetComponent(out Step2WisperTest component1)) _step2WisperTest = component1;
                if (go.TryGetComponent(out Step3And4Questionnary component2))
                    _step4And5Questionnary = component2;
                if (go.TryGetComponent(out Step5Otoscopie component3)) _step5Otoscopie = component3;
                if (go.TryGetComponent(out Step6WeberTest component4)) _step6HhiesTest = component4;
                if (go.TryGetComponent(out Step7HhiesTest component5)) _step7HhiesTest = component5;
                if (go.TryGetComponent(out Step8Audiometrie component6))
                    _step8Audiometrie = component6;
            }


            //SET LISTENER
            confirmNextButton.onClick.AddListener(ButtonNext);
            returnButton.onClick.AddListener(ButtonBack);

            _mappingDisplays = new Dictionary<Step, int>(){
                { Step.CasePresentation, 0 },
                { Step.WisperTest, 1 },
                { Step.Questionnary, 2 },
                { Step.Otoscopy, 3 },
                { Step.WeberTest, 4 },
                { Step.HhiesTest, 5 },
                { Step.Audiometry, 6 },
            };
        }
    }
}