using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Buttons {
    /// <summary>
    /// Manages an answer button in the UI with different visual states
    /// for normal, correct, and incorrect answers.
    /// Requires Unity UI Button and Image components.
    /// </summary>
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    public class AnswerButton : MonoBehaviour {
        public bool IsConfirmed { get; private set; }
        [HideInInspector] public Button button;
        private TMP_Text text;
        private Image image;

        [Header("Colors")] [SerializeField] private Color normalColor;
        [SerializeField] private Color disableColor;
        [SerializeField] private Color correctColor;
        [SerializeField] private Color incorrectColor;

        /// <summary>
        /// Initializes references to the Button, TMP_Text, and Image components on the GameObject and its children.
        /// </summary>
        void Awake() {
            button = gameObject.GetComponent<Button>();
            text = gameObject.GetComponentInChildren<TMP_Text>();
            image = gameObject.GetComponent<Image>();
        }

        /// <summary> Enables or disables interaction on the button. </summary>
        public void SetInteractable(bool interactable) {
            button.interactable = interactable;
            text.color = interactable ? normalColor : disableColor;
        }

        /// <summary> Resets the button to its normal state and makes it interactable. </summary>
        public void Reset() {
            IsConfirmed = false;
            image.color = normalColor;
            SetInteractable(true);
        }

        /// <summary> Marks the button as correct, changes color, and disables interaction. </summary>
        public void SetCorrect() {
            IsConfirmed = true;
            image.color = correctColor;
            SetInteractable(false);
        }

        /// <summary> Marks the button as incorrect, changes color, and disables interaction. </summary>
        public void SetIncorrect() {
            IsConfirmed = true;
            image.color = incorrectColor;
            SetInteractable(false);
        }
    }
}
