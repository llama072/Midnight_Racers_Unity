using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MidnightRacers.Settings
{
    /// <summary>
    /// Special dropdown that pulls its option list from Screen.resolutions at runtime.
    /// Bound to SettingsData.resolutionIndex.
    /// </summary>
    public class SettingsResolutionDropdown : MonoBehaviour
    {
        [Header("References")]
        public TMP_Text valueLabel;
        public Button leftArrow;
        public Button rightArrow;

        private Resolution[] _resolutions;

        private void Start()
        {
            if (SettingsManager.Instance == null)
            {
                Debug.LogError("[SettingsResolutionDropdown] SettingsManager.Instance is null.", this);
                enabled = false;
                return;
            }

            _resolutions = SettingsManager.Instance.AvailableResolutions;

            if (leftArrow  != null) leftArrow.onClick.AddListener(() => Cycle(-1));
            if (rightArrow != null) rightArrow.onClick.AddListener(() => Cycle(+1));
            Refresh();
        }

        public void Cycle(int direction)
        {
            if (_resolutions == null || _resolutions.Length == 0) return;

            var data = SettingsManager.Instance.Data;
            int next = ((data.resolutionIndex + direction) % _resolutions.Length + _resolutions.Length) % _resolutions.Length;
            data.resolutionIndex = next;
            Refresh();
        }

        public void Refresh()
        {
            if (_resolutions == null) _resolutions = SettingsManager.Instance.AvailableResolutions;
            if (_resolutions.Length == 0 || valueLabel == null) return;

            int idx = Mathf.Clamp(SettingsManager.Instance.Data.resolutionIndex, 0, _resolutions.Length - 1);
            var res = _resolutions[idx];
            valueLabel.text = $"{res.width} x {res.height}";
        }
    }
}
