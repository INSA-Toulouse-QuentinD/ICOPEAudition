using System.Collections.Generic;
using UI.ScoreContents;
using UnityEngine;

namespace UI.LevelSelector{
    /// <summary>
    /// Static manager for handling button focus states across grouped UI buttons.
    /// </summary>
    /// <remarks>
    /// Buttons are grouped by a group ID, allowing only one button in a group to be "pressed" (focused) at a time.
    /// This is useful for tab selectors, difficulty buttons, etc.
    /// </remarks>
    public static class ButtonsManager{
        private static readonly Dictionary<int, List<ButtonPressDetector>> ConnectedButtons = new();

        /// <summary>
        /// Adds a button to its corresponding group in the manager, allowing it to be managed for focus state.
        /// </summary>
        /// <param name="button">The button to connect, which contains a group ID.</param>
        public static void ConnectButton(ButtonPressDetector button){
            if (ConnectedButtons.TryGetValue(button.groupId, out var connectedButton))
                connectedButton.Add(button);
            else ConnectedButtons.Add(button.groupId, new List<ButtonPressDetector>{ button });
        }

        /// <summary>
        /// Sets the specified button as the focused (pressed) one within its group, and resets others to normal state.
        /// </summary>
        /// <param name="button">The button to set as focused (pressed).</param>
        public static void SetButtonFocused(ButtonPressDetector button){
            foreach (ButtonPressDetector connectButton in ConnectedButtons[button.groupId]) {
                connectButton.AssignState(connectButton == button
                    ? ButtonPressDetector.ButtonState.Pressed
                    : ButtonPressDetector.ButtonState.None);
            }

            button.AssignState(ButtonPressDetector.ButtonState.Pressed);
        }
    }
}