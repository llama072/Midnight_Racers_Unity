using UnityEngine;
using UnityEngine.Audio;

namespace MidnightRacers.Settings
{
    /// <summary>
    /// Persistent singleton that loads, saves and applies user settings.
    /// Survives scene loads. Place one instance in your boot/main menu scene.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }
        public SettingsData Data { get; private set; }

        [Header("Optional Audio Mixer")]
        [Tooltip("If assigned, volume settings drive these exposed parameters (in dB).")]
        public AudioMixer audioMixer;
        public string masterParam = "MasterVolume";
        public string musicParam = "MusicVolume";
        public string sfxParam = "SFXVolume";
        public string engineParam = "EngineVolume";
        public string uiParam = "UIVolume";

        [Header("Optional Pixel Perfect Camera")]
        [Tooltip("Reference to a PixelPerfectCamera component (or any Behaviour) — toggled by the Pixel Perfect setting.")]
        public Behaviour pixelPerfectCamera;

        // FPS cap options shown to the user. -1 means uncapped.
        public static readonly int[] FpsCapOptions = { 30, 60, 120, 144, 240, -1 };

        private const string PREFS_KEY = "MidnightRacers.Settings.v1";
        private Resolution[] _cachedResolutions;

        public Resolution[] AvailableResolutions
        {
            get
            {
                if (_cachedResolutions == null) _cachedResolutions = Screen.resolutions;
                return _cachedResolutions;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Load();
            Apply();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (Data == null) return;
            if (Data.muteWhenUnfocused)
                AudioListener.pause = !hasFocus;
        }

        // ─── Persistence ───────────────────────────────────────────

        public void Load()
        {
            if (PlayerPrefs.HasKey(PREFS_KEY))
            {
                try
                {
                    string json = PlayerPrefs.GetString(PREFS_KEY);
                    Data = JsonUtility.FromJson<SettingsData>(json) ?? new SettingsData();
                }
                catch
                {
                    Debug.LogWarning("[SettingsManager] Corrupt settings — restoring defaults.");
                    Data = new SettingsData();
                }
            }
            else
            {
                Data = new SettingsData();
                // First launch — pick a sensible default resolution (current native).
                Data.resolutionIndex = FindCurrentResolutionIndex();
            }

            // Resolution index might be invalid if user changed monitor; sanitize.
            if (Data.resolutionIndex < 0 || Data.resolutionIndex >= AvailableResolutions.Length)
                Data.resolutionIndex = FindCurrentResolutionIndex();
        }

        public void Save()
        {
            string json = JsonUtility.ToJson(Data);
            PlayerPrefs.SetString(PREFS_KEY, json);
            PlayerPrefs.Save();
        }

        public void ResetToDefaults()
        {
            Data = new SettingsData();
            Data.resolutionIndex = FindCurrentResolutionIndex();
            Apply();
            Save();
        }

        // ─── Apply ─────────────────────────────────────────────────

        public void Apply()
        {
            ApplyDisplay();
            ApplyAudio();
        }

        public void ApplyDisplay()
        {
            var resolutions = AvailableResolutions;
            if (Data.resolutionIndex >= 0 && Data.resolutionIndex < resolutions.Length)
            {
                var res = resolutions[Data.resolutionIndex];
                FullScreenMode mode = Data.windowMode switch
                {
                    0 => FullScreenMode.ExclusiveFullScreen,
                    1 => FullScreenMode.FullScreenWindow,
                    2 => FullScreenMode.Windowed,
                    _ => FullScreenMode.ExclusiveFullScreen,
                };
                Screen.SetResolution(res.width, res.height, mode, res.refreshRateRatio);
            }

            QualitySettings.vSyncCount = Data.vsync ? 1 : 0;
            Application.targetFrameRate = FpsCapOptions[Mathf.Clamp(Data.fpsCapIndex, 0, FpsCapOptions.Length - 1)];

            if (pixelPerfectCamera != null)
                pixelPerfectCamera.enabled = Data.pixelPerfect;
        }

        public void ApplyAudio()
        {
            if (audioMixer != null)
            {
                audioMixer.SetFloat(masterParam, VolumeToDb(Data.masterVolume));
                audioMixer.SetFloat(musicParam, VolumeToDb(Data.musicVolume));
                audioMixer.SetFloat(sfxParam, VolumeToDb(Data.sfxVolume));
                audioMixer.SetFloat(engineParam, VolumeToDb(Data.engineVolume));
                audioMixer.SetFloat(uiParam, VolumeToDb(Data.uiVolume));
            }
            else
            {
                // Fallback when no AudioMixer: drive the global listener volume from master.
                AudioListener.volume = Data.masterVolume / 10f;
            }
        }

        private static float VolumeToDb(int volume0To10)
        {
            if (volume0To10 <= 0) return -80f;
            return Mathf.Log10(volume0To10 / 10f) * 20f;
        }

        private int FindCurrentResolutionIndex()
        {
            var resolutions = AvailableResolutions;
            int bestIndex = resolutions.Length - 1;
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    bestIndex = i;
                }
            }
            return bestIndex;
        }
    }
}