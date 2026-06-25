using System.Collections.Generic;
using Managers;
using UI.LevelSelector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.ScoreContents{
    public sealed class ButtonPressDetector : Graphic, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler,
        IPointerExitHandler{
        public enum ButtonState{
            None,
            Hovered,
            Pressed,
            HoveredPressed,
            Released,
            Disable
        }

        [System.Serializable]
        private class IconColor{
            public Graphic targetGraphic;
            public ColorBlock colorBlock;
        }

        public int groupId;
        [SerializeField] private List<IconColor> iconColors;
        [SerializeField] private bool isFocus, isDisable, stayFocusOnPressed = true;

        [HideInInspector] public UnityEvent onPress, onPressExit, onHover, onHoverExit;

        public ButtonState currentButtonState = ButtonState.None;

        protected override void Awake(){
            ButtonsManager.ConnectButton(this);
            if (isFocus) ButtonsManager.SetButtonFocused(this);
            if (isDisable) AssignState(ButtonState.Disable);
        }

        protected override void Start(){
            if (GameManager.Instance.canAccessAllLevel) AssignState(ButtonState.None);
        }

        protected override void OnEnable(){
            base.OnEnable();
            AssignColor();
        }

        public void AssignState(ButtonState state, bool overrideDisable = false){
            if ((GameManager.Instance.canAccessAllLevel && currentButtonState == ButtonState.Disable) ||
                (currentButtonState != ButtonState.Disable || overrideDisable)) {
                currentButtonState = state;
                AssignColor();
            }
        }

        public void OnPointerDown(PointerEventData eventData){
            if (currentButtonState != ButtonState.Disable) {
                ButtonsManager.SetButtonFocused(this);
                onPress.Invoke();
            }
        }

        public void OnPointerUp(PointerEventData eventData){
            if (currentButtonState != ButtonState.Disable) {
                if (!stayFocusOnPressed || currentButtonState != ButtonState.Pressed) {
                    currentButtonState = ButtonState.Released;
                    AssignColor();
                }

                onPressExit.Invoke();
            }
        }

        public void OnPointerEnter(PointerEventData eventData){
            if (currentButtonState != ButtonState.Disable) {
                currentButtonState = currentButtonState == ButtonState.Pressed
                    ? ButtonState.HoveredPressed
                    : ButtonState.Hovered;
                AssignColor();
                onHover.Invoke();
            }
        }

        public void OnPointerExit(PointerEventData eventData){
            if (currentButtonState != ButtonState.Disable) {
                if (!stayFocusOnPressed || currentButtonState != ButtonState.Pressed) {
                    currentButtonState = currentButtonState == ButtonState.HoveredPressed
                        ? ButtonState.Pressed
                        : ButtonState.None;
                    AssignColor();
                }

                onHoverExit.Invoke();
            }
        }

        private void AssignColor(){
            int index = (int)currentButtonState;
            foreach (IconColor col in iconColors) {
                col.targetGraphic.color = index switch{
                    0 => col.colorBlock.normalColor,
                    1 or 3 => col.colorBlock.highlightedColor,
                    2 => col.colorBlock.pressedColor,
                    4 => col.colorBlock.selectedColor,
                    _ => col.colorBlock.disabledColor
                };
            }
        }
    }
}