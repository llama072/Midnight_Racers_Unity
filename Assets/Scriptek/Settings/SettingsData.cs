using System;

namespace MidnightRacers.Settings
{
    /// <summary>
    /// Plain data container for all user-adjustable settings.
    /// Serialized to JSON and stored in PlayerPrefs by SettingsManager.
    /// </summary>
    [Serializable]
    public class SettingsData
    {
        // ─── DISPLAY ───────────────────────────────────────────────
        public int resolutionIndex = -1;          // -1 = use current native; otherwise index into Screen.resolutions
        public int windowMode = 0;           // 0 = Fullscreen, 1 = Borderless, 2 = Windowed
        public bool vsync = true;
        public int fpsCapIndex = 1;           // index into SettingsManager.FpsCapOptions
        public bool pixelPerfect = true;
        public int brightness = 6;           // 0..10

        // ─── AUDIO ─────────────────────────────────────────────────
        public int masterVolume = 8;       // 0..10
        public int musicVolume = 5;
        public int sfxVolume = 7;
        public int engineVolume = 9;
        public int uiVolume = 6;
        public bool muteWhenUnfocused = false;

        // ─── CONTROLS ──────────────────────────────────────────────
        public int steeringSensitivity = 5;       // 0..10
        public bool invertCamera = false;
        public bool controllerVibration = true;

        public string keyAccelerate = "W";
        public string keyBrake = "S";
        public string keyLeft = "A";
        public string keyRight = "D";
        public string keyHandbrake = "Space";
        public string keyNitrous = "LeftShift";
        public string keyLookBack = "C";

        // ─── GAMEPLAY ──────────────────────────────────────────────
        public int difficulty = 1;          // 0=Easy, 1=Normal, 2=Hard, 3=Sim
        public int speedUnit = 0;          // 0=KM/H, 1=MPH
        public int defaultCamera = 0;          // 0=Chase, 1=Hood, 2=Cockpit, 3=Bumper
        public bool hudVisible = true;
        public bool absAssist = true;
        public bool tractionControl = true;
        public bool autoBrake = false;

        // ─── LANGUAGE ──────────────────────────────────────────────
        public int language = 0;                 // 0=EN, 1=HU, 2=DE, 3=ES, 4=JP
        public bool subtitles = true;
    }
}