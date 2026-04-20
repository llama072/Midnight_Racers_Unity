using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MidnightRacers.Settings
{
    /// <summary>
    /// ON / OFF toggle bound to a boolean field on SettingsData by name.
    /// Drop on a Button + TMP_Text combo; set <see cref="settingKey"/> to the field name (e.g. "vsync").
    /// </summary>
    public class SettingsToggle : MonoBehaviour
    {
        [Header("Binding")]
        [Tooltip("Field name in SettingsData (case-sensitive, e.g. \"vsync\", \"hudVisible\").")]
        public string settingKey;

        [Header("References")]
        public Button button;
        public TMP_Text label;

        [Header("Style")]
        public Color onColor = new Color(0f, 1f, 0.62f);
        public Color offColor = new Color(1f, 0.33f, 0.46f);
        public string onText = "ON";
        public string offText = "OFF";

        [Header("Behaviour")]
        public bool applyImmediately = false;     // useful for audio/HUD toggles

        private FieldInfo _field;

        private void Start()
        {
            _field = typeof(SettingsData).GetField(settingKey);
            if (_field == null)
            {
                Debug.LogError($"[SettingsToggle] Unknown settings field '{settingKey}' on {name}", this);
                return;
            }

            if (button != null) button.onClick.AddListener(Toggle);
            Refresh();
        }

        public void Toggle()
        {
            if (_field == null || SettingsManager.Instance == null) return;

            bool current = (bool)_field.GetValue(SettingsManager.Instance.Data);
            _field.SetValue(SettingsManager.Instance.Data, !current);
            Refresh();

            if (applyImmediately) SettingsManager.Instance.Apply();
        }

        public void Refresh()
        {
            if (_field == null || SettingsManager.Instance == null) return;

            bool value = (bool)_field.GetValue(SettingsManager.Instance.Data);
            if (label != null)
            {
                label.text = value ? onText : offText;
                label.color = value ? onColor : offColor;
            }
        }
    }
}