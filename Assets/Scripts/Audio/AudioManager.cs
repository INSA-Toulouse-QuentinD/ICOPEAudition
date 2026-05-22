using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio{
    /// <summary>
    /// Manages audio playback within the game.
    /// </summary>
    /// <remarks>
    /// The SoundManager class provides functionalities for playing background music, sound effects,
    /// adjusting volume levels, muting/unmuting audio, and controlling playback behavior.
    /// </remarks>
    public class AudioManager : MonoBehaviour
    {
        #region Structures
        [System.Serializable]
        private struct AudioSrc
        {
            [SerializeField] public string id;
            [SerializeField] public AudioSource source;
        }

        [System.Serializable]
        private struct Bgm
        {
            [SerializeField] public string id;
            [SerializeField] public AudioClip clip;
            [SerializeField] public string title;
            [SerializeField] public string artist;

            public string GetName()
            {
                return $"{title} - {artist}";
            }
        }

        [System.Serializable]
        private struct Sfx
        {
            [SerializeField] public string id;
            [SerializeField] public AudioClip clip;
        }
        #endregion

        #region Configurable Attributes
        [Header("Audio Sources")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioSource bgmSource;
        [SerializeField][Tooltip("Sfx Audio Sources")] private List<AudioSrc> sfxAudioSrc = new List<AudioSrc>();

        [Header("Musics & Sound Effects")]
        [SerializeField][Tooltip("Background Musics")] private List<Bgm> bgms = new List<Bgm>();
        [SerializeField][Tooltip("Sound Effects")] private List<Sfx> sfxs = new List<Sfx>();
        #endregion

        #region Private Attributes
        private Dictionary<string, AudioSource> sfxSources = new Dictionary<string, AudioSource>();
        private Dictionary<string, AudioClip> BgmClips = new Dictionary<string, AudioClip>();
        private Dictionary<string, AudioClip> SfxClips = new Dictionary<string, AudioClip>();
        private Dictionary<string, string> BgmName = new Dictionary<string, string>();

        private string currentBGMPlayedName, currentBGMPlayedId;
        private Coroutine resumeBgmCoroutine, scratchAndStopBgmCoroutine;
        #endregion

        #region Initializing variables
        void Awake()
        {
            foreach (var bgm in bgms)
            {
                BgmClips.Add(bgm.id, bgm.clip);
                BgmName.Add(bgm.id, bgm.GetName());
            }

            foreach (var sfx in sfxs)
            {
                SfxClips.Add(sfx.id, sfx.clip);
            }

            foreach (var sfxSrc in sfxAudioSrc)
            {
                sfxSources.Add(sfxSrc.id, sfxSrc.source);
            }

            currentBGMPlayedName = "";
            currentBGMPlayedId = "";
        }
        #endregion

        #region Main methods
        /// <summary>
        /// Plays the background music attached to the given ID.
        /// </summary>
        /// <remarks>
        /// If the specified ID is not found, the method returns without playing any music.
        /// The current background music is stopped before playing the new one.
        /// </remarks>
        /// <param name="id">The ID of the background music to play.</param>
        /// <param name="startTime">Optional: The start time for playback in seconds (default: 0).</param>
        public void PlayBGM(string id, float startTime = 0.0f)
        {
            if (!BgmClips.TryGetValue(id, out AudioClip clip)) return;

            StopCurrentBgm();
            bgmSource.clip = clip;
            bgmSource.time = startTime;
            bgmSource.Play();

            currentBGMPlayedId = id;
            if (BgmName.TryGetValue(id, out string name)) currentBGMPlayedName = name;
        }

        /// <summary>
        /// Plays the sound effect attached to the given ID on the specified audio source.
        /// </summary>
        /// <remarks>
        /// If the specified ID is not found in the sound effect dictionary or the specified audio source ID is not found in the source dictionary, the method returns without playing any sound.
        /// If randomizePitch is true, the pitch of the sound effect will be randomized before playing.
        /// </remarks>
        /// <param name="id">The ID of the sound effect to play.</param>
        /// <param name="sfxId">Optional: The ID of the audio source to play the sound effect on (default: "SFX").</param>
        /// <param name="randomizePitch">Optional: Whether to randomize the pitch of the sound effect before playing (default: true).</param>
        public void PlaySFX(string id, string sfxId = "SFX", bool randomizePitch = true)
        {
            if (SfxClips.TryGetValue(id, out AudioClip clip) && sfxSources.TryGetValue(sfxId, out AudioSource sfxSource))
            {
                sfxSource.clip = clip;
                if (randomizePitch) RandomizePitch(sfxId);
                sfxSource.Play();
            }
        }
        #endregion

        #region Sound Settings and Customizations
        /// <summary>
        /// Randomizes the pitch of the specified audio source.
        /// </summary>
        /// <remarks>
        /// If the specified audio source ID is not found in the source dictionary, the method returns without making any changes.
        /// The pitch is randomized within the range of 0.9 to 1.1.
        /// </remarks>
        /// <param name="sfxId">Optional: The ID of the audio source to randomize the pitch for (default: "SFX").</param>
        public void RandomizePitch(string sfxId = "SFX")
        {
            float randomPitch = Random.Range(0.9f, 1.1f);
            if (sfxSources.TryGetValue(sfxId, out AudioSource sfxSource))
            {
                sfxSource.pitch = randomPitch;
            }
        }

        /// <summary>
        /// Get the name of the currently playing background music.
        /// </summary>
        /// <returns>The name of the currently playing background music.</returns>
        public string GetCurrentBGMName()
        {
            return currentBGMPlayedName;
        }

        public string GetCurrentBGMId()
        {
            return currentBGMPlayedId;
        }

        /// <summary>
        /// Gets the current playback time of the background music.
        /// </summary>
        /// <returns>The current playback time of the background music in seconds.</returns>
        public float GetBgmTime()
        {
            return bgmSource.time;
        }

        /// <summary>
        /// Retrieves the total duration of the background music currently assigned to the AudioSource component.
        /// </summary>
        /// <returns>
        /// The total duration of the background music in seconds.
        /// </returns>
        public float GetBgmTotalTime()
        {
            return bgmSource.clip.length;
        }

        /// <summary>
        /// Resumes playback of the background music from a specific time with optional scratch effect.
        /// </summary>
        /// <remarks>
        /// If the specified ID is not found in the background music dictionary, the method returns without resuming playback.
        /// The current background music is stopped before resuming playback of the new one.
        /// The optional scratch effect modifies the pitch of the background music over a specified duration.
        /// </remarks>
        /// <param name="id">The ID of the background music to resume playback from.</param>
        /// <param name="startTime">The start time for playback in seconds.</param>
        /// <param name="duration">Optional: The duration of the scratch effect in seconds (default: 2).</param>
        /// <param name="scratchSpeed">Optional: The speed of the scratch effect (default: 0.5).</param>
        /// <param name="startScratchSpeed">Optional: The initial scratch speed (default: 0.0).</param>
        /// <param name="targetPitch">Optional: The target pitch after the scratch effect (default: 1.0).</param>
        public void ResumeBgmAtTime(string id, float startTime, float duration = 2f, float scratchSpeed = 0.5f, float startScratchSpeed = 0.0f, float targetPitch = 1.0f)
        {
            if (BgmClips.TryGetValue(id, out AudioClip clip))
            {
                StopCurrentBgm();
                bgmSource.clip = clip;
                bgmSource.time = startTime;
                bgmSource.pitch = startScratchSpeed;
                if (resumeBgmCoroutine != null) StopCoroutine(resumeBgmCoroutine);
                resumeBgmCoroutine = StartCoroutine(_ResumeBgmAtTimeWithDuration(duration, scratchSpeed, startScratchSpeed, targetPitch));
            }
        }

        /// <summary>
        /// Resumes playback of the background music from a specific time with optional scratch effect.
        /// </summary>
        /// <remarks>
        /// If the specified ID is not found in the background music dictionary, the method returns without resuming playback.
        /// The current background music is stopped before resuming playback of the new one.
        /// The optional scratch effect modifies the pitch of the background music over a specified duration.
        /// </remarks>
        /// <param name="duration">The duration of the scratch effect in seconds.</param>
        /// <param name="scratchSpeed">The speed of the scratch effect.</param>
        /// <param name="startScratchSpeed">The initial scratch speed.</param>
        /// <param name="targetPitch">The target pitch after the scratch effect.</param>
        /// <returns>An IEnumerator used for coroutine execution.</returns>
        private IEnumerator _ResumeBgmAtTimeWithDuration(float duration, float scratchSpeed, float startScratchSpeed, float targetPitch)
        {
            float startTime = Time.time;
            float currentSpeed = startScratchSpeed;
            bgmSource.Play();

            while ((Time.time - startTime < duration) && bgmSource.pitch < targetPitch)
            {
                bgmSource.pitch = currentSpeed;
                currentSpeed += (scratchSpeed * Time.deltaTime);
                if (currentSpeed > targetPitch)
                {
                    currentSpeed = targetPitch;
                }
                yield return null;
            }
        }

        /// <summary>
        /// Applies a scratch effect to the background music and stops it after a specified duration.
        /// </summary>
        /// <remarks>
        /// If the background music source is null, has no clip attached, or is not currently playing, the method returns without applying any effect.
        /// The scratch effect modifies the pitch of the background music over the specified duration until reaching the minimum pitch value.
        /// </remarks>
        /// <param name="duration">Optional: The duration of the scratch effect in seconds (default: 2).</param>
        /// <param name="scratchSpeed">Optional: The speed of the scratch effect (default: 0.5).</param>
        /// <param name="minPitch">Optional: The minimum pitch value to reach during the scratch effect (default: 0.0).</param>
        public void ScratchAndStopBgm(float duration = 2f, float scratchSpeed = 0.5f, float minPitch = 0.0f)
        {
            if (bgmSource == null) return;
            if (bgmSource.clip == null) return;
            if (!bgmSource.isPlaying) return;
            if (scratchAndStopBgmCoroutine != null) StopCoroutine(scratchAndStopBgmCoroutine);
            scratchAndStopBgmCoroutine = StartCoroutine(_ScratchAndStopBgmWithDuration(duration, scratchSpeed, minPitch));
        }

        /// <summary>
        /// Applies a scratch effect to the background music and stops it after a specified duration.
        /// </summary>
        /// <remarks>
        /// If the background music source is null, has no clip attached, or is not currently playing, the method returns without applying any effect.
        /// The scratch effect modifies the pitch of the background music over the specified duration until reaching the minimum pitch value.
        /// </remarks>
        /// <param name="duration">The duration of the scratch effect in seconds.</param>
        /// <param name="scratchSpeed">The speed of the scratch effect.</param>
        /// <param name="minPitch">The minimum pitch value to reach during the scratch effect.</param>
        /// <returns>An IEnumerator used for coroutine execution.</returns>
        private IEnumerator _ScratchAndStopBgmWithDuration(float duration, float scratchSpeed, float minPitch)
        {
            float startTime = Time.time;
            float currentSpeed = bgmSource.pitch;

            while (Time.time - startTime < duration)
            {
                bgmSource.pitch = (currentSpeed < minPitch) ? minPitch : currentSpeed;
                currentSpeed -= (scratchSpeed * Time.deltaTime);

                yield return null;
            }
            StopCurrentBgm();
        }

        /// <summary>
        /// Applies a filter effect to the background music for a specified duration.
        /// </summary>
        /// <remarks>
        /// This method starts a filter effect coroutine with the given parameters.
        /// The filter effect gradually changes the cutoff frequency and resonance of a filter effect over time,
        /// starting with the specified cutoff frequency and resonance and transitioning to a target cutoff frequency
        /// over the given duration. Additionally, it maintains the target cutoff frequency for an additional duration if specified.
        /// </remarks>
        /// <param name="duration">The total duration of the filter effect in seconds.</param>
        /// <param name="startFilterCutOff">Optional: The initial cutoff frequency of the filter effect (default: 22000 Hz).</param>
        /// <param name="filterCutoffFrequency">Optional: The target cutoff frequency to transition to (default: 720 Hz).</param>
        /// <param name="filterResonance">Optional: The resonance value of the filter effect (default: 1.2).</param>
        /// <param name="lockFrequencyDuration">Optional: The duration to maintain the target cutoff frequency after transition (default: 0.5 seconds).</param>
        public void FilterEffect(float duration, float startFilterCutOff = 22000, float filterCutoffFrequency = 720, float filterResonance = 1.2f, float lockFrequencyDuration = .5f)
        {
            if (duration <= .0f) return;
            StartCoroutine(FilterEffectForDuration(duration, startFilterCutOff, filterCutoffFrequency, filterResonance, lockFrequencyDuration));
        }

        /// <summary>
        /// Applies a filter effect to the background music for a specified duration.
        /// </summary>
        /// <remarks>
        /// This method gradually changes the cutoff frequency and resonance of a filter effect over time.
        /// The effect starts with a specified cutoff frequency and resonance, and then smoothly transitions
        /// to a target cutoff frequency over the given duration. Additionally, it maintains the target
        /// cutoff frequency for an additional duration if specified.
        /// </remarks>
        /// <param name="duration">The total duration of the filter effect in seconds.</param>
        /// <param name="startFilterCutOff">The initial cutoff frequency of the filter effect.</param>
        /// <param name="filterCutoffFrequency">The target cutoff frequency to transition to.</param>
        /// <param name="filterResonance">The resonance value of the filter effect.</param>
        /// <param name="lockFrequencyDuration">Optional: The duration to maintain the target cutoff frequency after transition (default: 0).</param>
        /// <returns>An IEnumerator used for coroutine execution.</returns>
        private IEnumerator FilterEffectForDuration(float duration, float startFilterCutOff, float filterCutoffFrequency, float filterResonance, float lockFrequencyDuration)
        {
            audioMixer.SetFloat("FilterCutoff", startFilterCutOff);
            audioMixer.SetFloat("FilterResonance", filterResonance);

            float stepFilterCutOff = (startFilterCutOff - filterCutoffFrequency) / duration;
            float startTime = Time.time;
            duration += lockFrequencyDuration;

            while (Time.time - startTime < duration)
            {
                float currentCutOff = startFilterCutOff - stepFilterCutOff * (Time.time - startTime);
                if (currentCutOff < filterCutoffFrequency)
                {
                    currentCutOff = filterCutoffFrequency;
                }
                audioMixer.SetFloat("FilterCutoff", currentCutOff);
                yield return null;
            }

            audioMixer.ClearFloat("FilterCutoff");
            audioMixer.ClearFloat("FilterResonance");
        }

        /// <summary>
        /// Resets the pitch of the background music to its default value.
        /// </summary>
        /// <remarks>
        /// This method sets the pitch of the background music source to 1.0, restoring it to its original playback speed.
        /// </remarks>
        public void ResetBgmPitch()
        {
            bgmSource.pitch = 1.0f;
        }

        /// <summary>
        /// Changes the master volume level of the audio mixer.
        /// </summary>
        /// <remarks>
        /// This method adjusts the master volume level of the audio mixer based on the provided value.
        /// The value should be in the range [0.0, 1.0], where 0.0 represents silence and 1.0 represents full volume.
        /// </remarks>
        /// <param name="value">The new master volume level, ranging from 0.0 (silence) to 1.0 (full volume).</param>
        public void ChangeMasterVolume(float value)
        {
            audioMixer.SetFloat("MasterVolume", -80 + (Mathf.Clamp01(value) * 80));
        }

        /// <summary>
        /// Changes the volume level of the background music.
        /// </summary>
        /// <remarks>
        /// This method adjusts the volume level of the background music source based on the provided value.
        /// The value should be in the range [0.0, 1.0], where 0.0 represents silence and 1.0 represents full volume.
        /// </remarks>
        /// <param name="value">The new volume level for the background music, ranging from 0.0 (silence) to 1.0 (full volume).</param>
        public void ChangeBgmVolume(float value)
        {
            bgmSource.volume = value;
        }

        /// <summary>
        /// Changes the volume level of all sound effects.
        /// </summary>
        /// <remarks>
        /// This method adjusts the volume level of all sound effect sources based on the provided value.
        /// The value should be in the range [0.0, 1.0], where 0.0 represents silence and 1.0 represents full volume.
        /// </remarks>
        /// <param name="value">The new volume level for the sound effects, ranging from 0.0 (silence) to 1.0 (full volume).</param>
        public void ChangeSfxVolume(float value)
        {
            foreach ((_, AudioSource src) in sfxSources)
            {
                src.volume = value;
            }
        }

        /// <summary>
        /// Mutes or unmutes the background music.
        /// </summary>
        /// <remarks>
        /// This method sets the mute state of the background music source based on the provided boolean value.
        /// If <paramref name="muted"/> is true, the background music will be muted. Otherwise, it will be unmuted.
        /// </remarks>
        /// <param name="muted">True to mute the background music, false to unmute it.</param>
        public void MuteBgm(bool muted)
        {
            bgmSource.mute = muted;
        }

        /// <summary>
        /// Mutes or unmutes all sound effects.
        /// </summary>
        /// <remarks>
        /// This method sets the mute state of all sound effect sources based on the provided boolean value.
        /// If <paramref name="muted"/> is true, all sound effects will be muted. Otherwise, they will be unmuted.
        /// </remarks>
        /// <param name="muted">True to mute all sound effects, false to unmute them.</param>
        public void MuteSfx(bool muted)
        {
            foreach ((_, AudioSource src) in sfxSources)
            {
                src.mute = muted;
            }
        }

        /// <summary>
        /// Toggles the mute state of the background music.
        /// </summary>
        /// <remarks>
        /// This method toggles the mute state of the background music source.
        /// If the background music is currently muted, it will be unmuted, and vice versa.
        /// </remarks>
        public void ToggleBgm()
        {
            bgmSource.mute ^= true;
        }

        /// <summary>
        /// Toggles the mute state of all sound effects.
        /// </summary>
        /// <remarks>
        /// This method toggles the mute state of all sound effect sources.
        /// If any sound effect is currently muted, it will be unmuted, and vice versa.
        /// </remarks>
        public void ToggleSfx()
        {
            foreach ((_, AudioSource src) in sfxSources)
            {
                src.mute ^= true;
            }
        }

        /// <summary>
        /// Sets the looping behavior of the background music.
        /// </summary>
        /// <remarks>
        /// This method sets whether the background music should loop indefinitely or not.
        /// If <paramref name="loop"/> is true, the background music will loop. Otherwise, it will play only once.
        /// </remarks>
        /// <param name="loop">True to enable looping of the background music, false to disable it.</param>
        public void LoopBgm(bool loop)
        {
            bgmSource.loop = loop;
        }

        /// <summary>
        /// Sets the looping behavior of all sound effects.
        /// </summary>
        /// <remarks>
        /// This method sets whether all sound effects should loop indefinitely or not.
        /// If <paramref name="loop"/> is true, all sound effects will loop. Otherwise, they will play only once.
        /// </remarks>
        /// <param name="loop">True to enable looping of all sound effects, false to disable it.</param>
        public void LoopSfx(bool loop)
        {
            foreach ((_, AudioSource src) in sfxSources)
            {
                src.loop = loop;
            }
        }

        /// <summary>
        /// Sets the looping behavior of one sound effect.
        /// </summary>
        /// <remarks>
        /// This method sets whether one sound effect should loop indefinitely or not.
        /// If <paramref name="loop"/> is true, one sound effects will loop. Otherwise, they will play only once.
        /// </remarks>
        /// <param name="loop">True to enable looping of one sound effects, false to disable it.</param>
        public void LoopSfx(bool loop, string sfxId)
        {
            if (sfxSources.TryGetValue(sfxId, out AudioSource src))
            {
                src.loop = loop;
            }
        }

        /// <summary>
        /// Pauses or resumes the background music.
        /// </summary>
        /// <remarks>
        /// This method pauses or resumes playback of the background music source based on the provided boolean value.
        /// If <paramref name="pause"/> is true, the background music will be paused. Otherwise, it will be resumed.
        /// </remarks>
        /// <param name="pause">True to pause the background music, false to resume it.</param>
        public void PauseBgm(bool pause)
        {
            if (pause)
            {
                bgmSource.Pause();
            }
            else
            {
                bgmSource.UnPause();
            }
        }

        /// <summary>
        /// Pauses or resumes all sound effects.
        /// </summary>
        /// <remarks>
        /// This method pauses or resumes playback of all sound effect sources based on the provided boolean value.
        /// If <paramref name="pause"/> is true, all sound effects will be paused. Otherwise, they will be resumed.
        /// </remarks>
        /// <param name="pause">True to pause all sound effects, false to resume them.</param>
        public void PauseSfx(bool pause)
        {
            if (pause)
            {
                foreach ((_, AudioSource src) in sfxSources)
                {
                    src.Pause();
                }
            }
            else
            {
                foreach ((_, AudioSource src) in sfxSources)
                {
                    src.UnPause();
                }
            }
        }

        /// <summary>
        /// Stops the playback of the current background music.
        /// </summary>
        /// <remarks>
        /// This method stops the playback of the current background music source and clears the name of the currently played background music.
        /// </remarks>
        public void StopCurrentBgm()
        {
            bgmSource.Stop();
            currentBGMPlayedName = "";
            currentBGMPlayedId = "";
        }

        /// <summary>
        /// Stops the playback of all currently playing sound effects.
        /// </summary>
        /// <remarks>
        /// This method stops the playback of all currently playing sound effect sources.
        /// </remarks>
        public void StopCurrentSfx()
        {
            foreach ((_, AudioSource src) in sfxSources)
            {
                src.Stop();
            }
        }
        #endregion
    }
}