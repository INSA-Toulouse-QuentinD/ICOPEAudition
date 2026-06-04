using UnityEngine;
using UnityEngine.UI;

namespace Managers{
    public class ImageObserveurManager : MonoBehaviour{
        private Image _imageComponent;
        private GameObject _area;
        [SerializeField] private Image imageForImageComponent;

        private void Awake(){
            _imageComponent = GetComponent<Image>();
            _area = transform.GetChild(0).gameObject;

            Hide();
        }

        /// <summary>
        /// Affiche l'image en plein écran avec le sprite fourni
        /// </summary>
        /// <param name="sprite">Le sprite à afficher</param>
        public void Show(Image sprite){
            imageForImageComponent.sprite = sprite.sprite;
            _imageComponent.enabled = true;
            _area.SetActive(true);
        }

        /// <summary>
        /// Désactive l'affichage de l'image
        /// </summary>
        public void Hide(){
            _imageComponent.enabled = false;
            _area.SetActive(false);
        }
    }
}