using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MidnightRacers.Settings
{
    /// <summary>
    /// 10-segment "volume bar". Each segment is a clickable Image (auto-attaches a Button if missing).
    /// Bound to an integer field on SettingsData (range 0..10).
    /// </summary>
    public class SettingsVolumeBar : MonoBehaviour
    {
        [Header("Binding")]
        [Tooltip("Integer field name in SettingsData (e.g. \"masterVolume\", \"musicVolume\").")]
        public string settingKey;

        [Header("References")]
        [Tooltip("Exactly 10 Image segments, in order from left to right.")]
        public Image[] segments;
        public TMP_Text percentLabel;

        [Header("Style")]
        public Color filledColor = new Color(0.72f, 0.58f, 0.96f);   // purple
        public Color highColor = new Color(1f, 0.56f, 0.67f);      // pink (overdrive)
        public Color emptyColor = new Color(1f, 1f, 1f, 0.08f);

        [Tooltip("Segment index from which the 'high' color kicks in (0-based). Default 7 = segments 8/9/10.")]
        public int highThreshold = 7;

        [Header("Behaviour")]
        public bool applyImmediately = true;

        private FieldInfo _field;

        private void Start()
        {
            _field = typeof(SettingsData).GetField(settingKey);
            if (_field == null)
            {
                Debug.LogError($"[SettingsVolumeBar] Unknown settings field '{settingKey}' on {name}", this);
                return;
            }

            if (segments != null)
            {
                for (int i = 0; i < segments.Length; i++)
                {
                    if (segments[i] == null) continue;
                    int idx = i;
                    var btn = segments[i].GetComponent<Button>();
                    if (btn == null) btn = segments[i].gameObject.AddComponent<Button>();
                    btn.targetGraphic = segments[i];
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => SetValue(idx + 1));
                }
            }
            Refresh();
        }

        public void SetValue(int value)
        {
            if (_field == null || SettingsManager.Instance == null) return;

            int clamped = Mathf.Clamp(value, 0, segments != null ? segments.Length : 10);
            _field.SetValue(SettingsManager.Instance.Data, clamped);
            Refresh();

            if (applyImmediately) SettingsManager.Instance.ApplyAudio();
        }

        public void Refresh()
        {
            if (_field == null || SettingsManager.Instance == null || segments == null) return;

            int value = (int)_field.GetValue(SettingsManager.Instance.Data);
            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i] == null) continue;
                if (i < value)
                    segments[i].color = (i >= highThreshold) ? highColor : filledColor;
                else
                    segments[i].color = emptyColor;
            }
            if (percentLabel != null) percentLabel.text = (value * 10) + "%";
        }
    }
}