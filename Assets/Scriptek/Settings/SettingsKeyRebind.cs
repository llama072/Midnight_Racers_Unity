using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MidnightRacers.Settings
{
    /// <summary>
    /// Single key-rebind row. Click the button → press any key → field is updated.
    /// Bound to a string field on SettingsData (stores the KeyCode name, e.g. "W").
    /// </summary>
    public class SettingsKeyRebind : MonoBehaviour
    {
        [Header("Binding")]
        [Tooltip("String field name in SettingsData (e.g. \"keyAccelerate\").")]
        public string settingKey;

        [Header("References")]
        public Button rebindButton;
        public TMP_Text keyLabel;

        [Header("Style")]
        public string listeningText = "...";
        public Color listeningColor = new Color(0.72f, 0.58f, 0.96f);
        public Color idleColor = Color.white;

        private FieldInfo _field;
        private bool _listening;

        private void Start()
        {
            _field = typeof(SettingsData).GetField(settingKey);
            if (_field == null)
            {
                Debug.LogError($"[SettingsKeyRebind] Unknown settings field '{settingKey}' on {name}", this);
                return;
            }

            if (rebindButton != null) rebindButton.onClick.AddListener(StartListening);
            Refresh();
        }

        public void StartListening()
        {
            _listening = true;
            if (keyLabel != null)
            {
                keyLabel.text = listeningText;
                keyLabel.color = listeningColor;
            }
        }

        private void OnGUI()
        {
            if (!_listening) return;

            Event e = Event.current;
            if (e != null && e.isKey && e.keyCode != KeyCode.None)
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    // Cancel rebind
                    _listening = false;
                    Refresh();
                    return;
                }

                if (_field != null && SettingsManager.Instance != null)
                    _field.SetValue(SettingsManager.Instance.Data, e.keyCode.ToString());

                _listening = false;
                Refresh();
            }
        }

        public void Refresh()
        {
            if (_field == null || SettingsManager.Instance == null || keyLabel == null) return;

            keyLabel.text = (string)_field.GetValue(SettingsManager.Instance.Data);
            keyLabel.color = idleColor;
        }
    }
}