using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Boutique{
    [RequireComponent(typeof(Button))]
    public class Item : MonoBehaviour{
        [SerializeField] private int price;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private GameObject objectToDisplay;

        private Button _button;

        /// <summary>
        /// Initializes the component by setting the price text display,
        /// retrieving the Button component attached to the game object,
        /// and adding the Buy method as a click event listener for the button.
        /// </summary>
        void Start(){
            priceText.text = price.ToString();
            _button = gameObject.GetComponent<Button>();
            _button.onClick.AddListener(Buy);
        }

        /// <summary>
        /// Handles the purchase of an item if the player has enough money.
        /// Deducts the item's price from the player's money, activates the item,
        /// records the purchase, disables the purchase button, and plays a sound effect.
        /// </summary>
        private void Buy(){
            if (GameManager.Instance.Money >= price) {
                GameManager.Instance.Money -= price;
                objectToDisplay.SetActive(true);
                GameManager.AddBoughtItem(objectToDisplay.name);
                _button.interactable = false;
                GameManager.Instance.AudioManager.PlaySFX("money_down");
            }
        }
    }
}