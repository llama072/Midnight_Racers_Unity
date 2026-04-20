using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace MidnightRacers.Settings
{
    /// <summary>
    /// Top-level controller for the Settings scene.
    /// Handles category switching, keyboard navigation, Apply / Reset / Back.
    /// </summary>
    public class SettingsUIController : MonoBehaviour
    {
        [Serializable]
        public class CategoryEntry
        {
            public string name;
            public Button categoryButton;
            public TMP_Text categoryLabel;       // optional — drives the highlight color
            public GameObject panel;
        }

        [Header("Categories")]
        public CategoryEntry[] categories;
        public int defaultCategory = 0;

        [Header("Visual Highlight")]
        public Color activeColor = new Color(1f, 1f, 1f, 1f);
        public Color inactiveColor = new Color(0.42f, 0.43f, 0.54f, 1f);
        public GameObject activeIndicator;        // optional ▸ marker that follows the selected tab

        [Header("Navigation")]
        public KeyCode keyUp = KeyCode.W;
        public KeyCode keyDown = KeyCode.S;
        public KeyCode keyBack = KeyCode.Escape;
        public string mainMenuSceneName = "Main Menu";

        [Header("Buttons")]
        public Button applyButton;
        public Button resetButton;

        private int _activeIndex;

        private void Start()
        {
            for (int i = 0; i < categories.Length; i++)
            {
                int idx = i;
                if (categories[i].categoryButton != null)
                    categories[i].categoryButton.onClick.AddListener(() => SelectCategory(idx));
            }

            if (applyButton != null) applyButton.onClick.AddListener(Apply);
            if (resetButton != null) resetButton.onClick.AddListener(Reset);

            SelectCategory(Mathf.Clamp(defaultCategory, 0, categories.Length - 1));
        }

        private void Update()
        {
            if (categories == null || categories.Length == 0) return;

            if (Input.GetKeyDown(keyUp))
                SelectCategory((_activeIndex - 1 + categories.Length) % categories.Length);
            else if (Input.GetKeyDown(keyDown))
                SelectCategory((_activeIndex + 1) % categories.Length);
            else if (Input.GetKeyDown(keyBack))
                GoBack();
        }

        public void SelectCategory(int index)
        {
            _activeIndex = index;

            for (int i = 0; i < categories.Length; i++)
            {
                bool active = (i == index);
                if (categories[i].panel != null) categories[i].panel.SetActive(active);
                if (categories[i].categoryLabel != null)
                    categories[i].categoryLabel.color = active ? activeColor : inactiveColor;
            }

            if (activeIndicator != null && categories[index].categoryButton != null)
            {
                activeIndicator.transform.SetParent(categories[index].categoryButton.transform, false);
                activeIndicator.transform.localPosition = Vector3.zero;
                activeIndicator.SetActive(true);
            }
        }

        public void Apply()
        {
            if (SettingsManager.Instance == null) return;
            SettingsManager.Instance.Apply();
            SettingsManager.Instance.Save();
        }

        public void Reset()
        {
            if (SettingsManager.Instance == null) return;
            SettingsManager.Instance.ResetToDefaults();
            RefreshAllControls();
        }

        public void GoBack()
        {
            if (SettingsManager.Instance != null) SettingsManager.Instance.Save();

            if (!string.IsNullOrEmpty(mainMenuSceneName))
                SceneManager.LoadScene(mainMenuSceneName);
        }

        /// <summary>Rebinds every control under this UI to the current SettingsData values.</summary>
        public void RefreshAllControls()
        {
            foreach (var t in GetComponentsInChildren<SettingsToggle>(true)) t.Refresh();
            foreach (var d in GetComponentsInChildren<SettingsDropdown>(true)) d.Refresh();
            foreach (var r in GetComponentsInChildren<SettingsResolutionDropdown>(true)) r.Refresh();
            foreach (var v in GetComponentsInChildren<SettingsVolumeBar>(true)) v.Refresh();
            foreach (var k in GetComponentsInChildren<SettingsKeyRebind>(true)) k.Refresh();
        }
    }
}