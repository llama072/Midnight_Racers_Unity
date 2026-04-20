using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MidnightRacers.Settings
{
    /// <summary>
    /// Cycling dropdown ( ◄ VALUE ► ) bound to an integer field on SettingsData.
    /// </summary>
    public class SettingsDropdown : MonoBehaviour
    {
        [Header("Binding")]
        [Tooltip("Integer field name in SettingsData (e.g. \"windowMode\", \"difficulty\").")]
        public string settingKey;

        [Tooltip("Display strings for each option, in order. Length defines the cycle range.")]
        public string[] options;

        [Header("References")]
        public TMP_Text valueLabel;
        public Button leftArrow;
        public Button rightArrow;

        [Header("Behaviour")]
        public bool applyImmediately = false;

        private FieldInfo _field;

        private void Start()
        {
            _field = typeof(SettingsData).GetField(settingKey);
            if (_field == null)
            {
                Debug.LogError($"[SettingsDropdown] Unknown settings field '{settingKey}' on {name}", this);
                return;
            }

            if (leftArrow  != null) leftArrow.onClick.AddListener(() => Cycle(-1));
            if (rightArrow != null) rightArrow.onClick.AddListener(() => Cycle(+1));
            Refresh();
        }

        public void Cycle(int direction)
        {
            if (_field == null || SettingsManager.Instance == null || options == null || options.Length == 0) return;

            int current = (int)_field.GetValue(SettingsManager.Instance.Data);
            int next = ((current + direction) % options.Length + options.Length) % options.Length;
            _field.SetValue(SettingsManager.Instance.Data, next);
            Refresh();

            if (applyImmediately) SettingsManager.Instance.Apply();
        }

        public void Refresh()
        {
            if (_field == null || SettingsManager.Instance == null || valueLabel == null) return;

            int idx = (int)_field.GetValue(SettingsManager.Instance.Data);
            idx = Mathf.Clamp(idx, 0, options.Length - 1);
            valueLabel.text = options[idx];
        }
    }
}
