using Managers;
using UnityEngine;

namespace UI.Boutique{
    /// <summary>
    /// Handles displaying the player's current money and the amount of change in the UI,
    /// updating the display and playing an animation whenever the money amount changes.
    /// </summary>
    public class MoneyGUI : MonoBehaviour{
        [Header("References")] [SerializeField]
        private TMPro.TextMeshProUGUI moneyText;

        [SerializeField] private TMPro.TextMeshProUGUI amountChangeText;
        [SerializeField] private Animation animationMoney;

        [Header("Colors")] [SerializeField] private Color positiveColor;
        [SerializeField] private Color negativeColor;

        /// <summary>
        /// Initializes the money display with the current amount and subscribes to money change events.
        /// </summary>
        private void Start(){
            moneyText.text = GameManager.Instance.Money.ToString();
            GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
        }

        /// <summary>
        /// Updates the displayed money amount and the change amount with proper sign and color,
        /// then plays an animation to highlight the change.
        /// </summary>
        /// <param name="money">The new total money amount.</param>
        /// <param name="amountChange">The change in money (positive or negative).</param>
        private void OnMoneyChanged(int money, int amountChange){
            moneyText.text = money.ToString();

            if (amountChange != 0) {
                string displayText = amountChange > 0 ? "+" : "-";
                displayText += Mathf.Abs(amountChange).ToString();
                amountChangeText.text = displayText;
                amountChangeText.color = amountChange > 0 ? positiveColor : negativeColor;

                animationMoney.Stop();
                animationMoney.Play();
            }
        }
    }
}