using System.Collections.Generic;
using Assets.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonPressDetector : Graphic, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum ButtonState
    {
        None,
        Hovered,
        Pressed,
        HoveredPressed,
        Released,
        Disable
    }

    [System.Serializable]
    private class IconColor
    {
        public Graphic targetGraphic;
        public ColorBlock colorBlock;
    }
    public int groupId = 0;
    [SerializeField] private List<IconColor> iconColors;
    [SerializeField] private bool isFocus = false, isDisable = false, stayFocusOnPressed = true;

    public UnityEvent OnPress, OnPressExit, OnHover, OnHoverExit;

    public ButtonState currentButtonState = ButtonState.None;

    protected override void Awake()
    {
        ButtonsManager.Connectbutton(this);
        if (isFocus) ButtonsManager.SetButtonFocused(this);
        if (isDisable) AssignState(ButtonState.Disable);
        if (GameManager.Instance.canAccessAllLevel) AssignState(ButtonState.None);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        AssignColor();
    }

    public void AssignState(ButtonState state, bool overrideDisable = false)
    {
        if ((GameManager.Instance.canAccessAllLevel && currentButtonState == ButtonState.Disable) ||
            (currentButtonState != ButtonState.Disable || overrideDisable)) {
            currentButtonState = state;
            AssignColor();
        }
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (currentButtonState != ButtonState.Disable)
        {
            ButtonsManager.SetButtonFocused(this);
            OnPress.Invoke();
        }
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (currentButtonState != ButtonState.Disable)
        {
            if (!stayFocusOnPressed || currentButtonState != ButtonState.Pressed)
            {
                currentButtonState = ButtonState.Released;
                AssignColor();
            }
            OnPressExit.Invoke();
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (currentButtonState != ButtonState.Disable)
        {
            currentButtonState = currentButtonState == ButtonState.Pressed ? ButtonState.HoveredPressed : ButtonState.Hovered;
            AssignColor();
            OnHover.Invoke();
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (currentButtonState != ButtonState.Disable)
        {
            if (!stayFocusOnPressed || currentButtonState != ButtonState.Pressed)
            {
                currentButtonState = currentButtonState == ButtonState.HoveredPressed ? ButtonState.Pressed : ButtonState.None;
                AssignColor();
            }
            OnHoverExit.Invoke();
        }
    }

    private void AssignColor()
    {
        int index = (int)currentButtonState;
        foreach (IconColor col in iconColors)
        {
            col.targetGraphic.color = index == 0 ? col.colorBlock.normalColor : index == 1 || index == 3 ? col.colorBlock.highlightedColor : index == 2 ? col.colorBlock.pressedColor : index == 4 ? col.colorBlock.selectedColor : col.colorBlock.disabledColor;
        }
    }
}
