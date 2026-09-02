using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PersoSelect {
    public class DocSprite : MonoBehaviour {
        public SpriteDocPose spritePose;

        void Start() {
            GetComponent<Image>().sprite = SpritesEmotions.Instance.sprites[GameManager.Instance.selectType][spritePose];
        }
    }

    public enum SpriteDocPose {
        Idle = 0,
        Bad = 1,
        Hand = 2,
        Sit = 3,
        Happy = 4,
        Tel = 5
    }
}