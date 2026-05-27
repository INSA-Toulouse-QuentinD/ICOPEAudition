using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace UI{
    /// <summary>
    /// Manages the settings menu for audio volumes and fullscreen mode.
    /// Allows the player to adjust master, music, and SFX volume levels,
    /// and toggle fullscreen mode. Settings are saved using PlayerPrefs
    /// and applied immediately via the AudioMixer and Screen settings.
    /// </summary>
    public class SettingsMenu : MonoBehaviour{
        [SerializeField] private AudioMixer mixer;

        [SerializeField] private Slider MasterSlider;
        [SerializeField] private Slider MusicSlider;
        [SerializeField] private Slider SFXSlider;

        /// <summary>
        /// Initializes the settings menu by loading saved preferences
        /// and applying them to the sliders, toggle, AudioMixer, and screen.
        /// </summary>
        private void Start(){
            float volume = PlayerPrefs.GetFloat("mastervolume", 0.5f);
            MasterSlider.value = volume;
            mixer.SetFloat("mastervolume", Mathf.Log10(volume) * 20);
            volume = PlayerPrefs.GetFloat("musicvolume", 0.5f);
            MusicSlider.value = volume;
            mixer.SetFloat("musicvolume", Mathf.Log10(volume) * 20);
            volume = PlayerPrefs.GetFloat("SFXvolume", 0.5f);
            SFXSlider.value = volume;
            mixer.SetFloat("SFXvolume", Mathf.Log10(volume) * 20);
        }

        /// <summary>
        /// Updates the master volume based on the slider value,
        /// applies it to the AudioMixer, and saves the setting.
        /// </summary>
        public void SetMaster(){
            float volume = MasterSlider.value;
            mixer.SetFloat("mastervolume", Mathf.Log10(volume) * 20);
            PlayerPrefs.SetFloat("mastervolume", volume);
        }

        /// <summary>
        /// Updates the music volume based on the slider value,
        /// applies it to the AudioMixer, and saves the setting.
        /// </summary>
        public void SetMusic(){
            float volume = MusicSlider.value;
            mixer.SetFloat("musicvolume", Mathf.Log10(volume) * 20);
            PlayerPrefs.SetFloat("musicvolume", volume);
        }

        /// <summary>
        /// Updates the sound effects (SFX) volume based on the slider value,
        /// applies it to the AudioMixer, and saves the setting.
        /// </summary>
        public void SetSFX(){
            float volume = SFXSlider.value;
            mixer.SetFloat("SFXvolume", Mathf.Log10(volume) * 20);
            PlayerPrefs.SetFloat("SFXvolume", volume);
        }
    }
}