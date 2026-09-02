using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI.PersoSelect {
    public class SpritesEmotions : MonoBehaviour {
        public static SpritesEmotions Instance;
        public Dictionary<SpriteDocType, Dictionary<SpriteDocPose, Sprite>> sprites;

        void Awake() {
            Instance = this;

            sprites = new();

            foreach (Sprite sprite in Resources.LoadAll<Sprite>("ImagesDoc")) {
                string[] parts = sprite.name.Split('_');

                if (parts.Length != 2)
                    continue;

                if (!Enum.TryParse(parts[0], out SpriteDocType type))
                    continue;

                if (!Enum.TryParse(parts[1], out SpriteDocPose pose))
                    continue;

                if (!sprites.ContainsKey(type))
                    sprites[type] = new Dictionary<SpriteDocPose, Sprite>();

                sprites[type][pose] = sprite;
            }
        }
    }

    public enum SpriteDocType {
        Man = 0,
        Woman = 1,
    }
}
