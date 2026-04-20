using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MidnightRacers.Settings
{
    /// <summary>
    /// Programmatically builds the entire Settings scene UI at runtime.
    /// Drop this on a single root GameObject in an empty scene → press Play → done.
    ///
    /// Optional inspector references:
    ///   - backgroundSprite, headerFont, bodyFont, monoFont
    ///
    /// Re-build any time via the context menu ("Build Now").
    /// </summary>
    [DisallowMultipleComponent]
    public class SettingsUIBuilder : MonoBehaviour
    {
        // ─── Inspector ─────────────────────────────────────────────
        [Header("Optional Assets (leave empty for defaults)")]
        public Sprite backgroundSprite;
        [Tooltip("Used for the title / category labels (Press Start 2P style).")]
        public TMP_FontAsset headerFont;
        [Tooltip("Used for setting labels (VT323 style).")]
        public TMP_FontAsset bodyFont;
        [Tooltip("Used for control values (Press Start 2P style).")]
        public TMP_FontAsset monoFont;

        [Header("Scene")]
        public string mainMenuSceneName = "Main Menu";

        [Header("Behaviour")]
        public bool buildOnAwake = true;
        public bool ensureEventSystem = true;
        [Tooltip("Auto-spawn a SettingsManager if the singleton isn't already alive (handy when testing this scene in isolation).")]
        public bool createManagerIfMissing = true;

        [Header("Layout")]
        public Vector2 referenceResolution = new Vector2(1920, 1080);

        // ─── Palette ───────────────────────────────────────────────
        static readonly Color C_ACCENT = new Color(0.72f, 0.58f, 0.96f);
        static readonly Color C_ACCENT_DIM = new Color(0.72f, 0.58f, 0.96f, 0.3f);
        static readonly Color C_TEXT = new Color(0.91f, 0.91f, 0.94f);
        static readonly Color C_TEXT_DIM = new Color(0.42f, 0.43f, 0.54f);
        static readonly Color C_BG_DARK = new Color(0.02f, 0.03f, 0.06f, 1f);
        static readonly Color C_PANEL_BG = new Color(0.04f, 0.05f, 0.12f, 0.6f);
        static readonly Color C_BORDER = new Color(0.72f, 0.58f, 0.96f, 0.25f);
        static readonly Color C_ON = new Color(0f, 1f, 0.62f);
        static readonly Color C_OFF = new Color(1f, 0.33f, 0.46f);
        static readonly Color C_VOL_FILLED = new Color(0.72f, 0.58f, 0.96f);
        static readonly Color C_VOL_HIGH = new Color(1f, 0.56f, 0.67f);
        static readonly Color C_VOL_EMPTY = new Color(1f, 1f, 1f, 0.08f);

        // ─── Runtime state ─────────────────────────────────────────
        Canvas _canvas;
        SettingsUIController _controller;

        // ─── Lifecycle ─────────────────────────────────────────────
        void Awake()
        {
            if (buildOnAwake) Build();
        }

        [ContextMenu("Build Now")]
        public void Build()
        {
            // Clear any previous build under this object.
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying) Destroy(transform.GetChild(i).gameObject);
                else DestroyImmediate(transform.GetChild(i).gameObject);
            }
            var existingCtrl = GetComponent<SettingsUIController>();
            if (existingCtrl != null)
            {
                if (Application.isPlaying) Destroy(existingCtrl);
                else DestroyImmediate(existingCtrl);
            }

            EnsureEventSystem();
            EnsureSettingsManager();

            BuildCanvas();
            BuildBackground(_canvas.transform);
            BuildScanlines(_canvas.transform);

            BuildHeader(_canvas.transform);
            var (categoryEntries, panelMap) = BuildBody(_canvas.transform);
            var (applyBtn, resetBtn) = BuildButtonBar(_canvas.transform);
            BuildFooter(_canvas.transform);

            // Controller wiring
            _controller = gameObject.AddComponent<SettingsUIController>();
            _controller.categories = new SettingsUIController.CategoryEntry[categoryEntries.Count];
            for (int i = 0; i < categoryEntries.Count; i++)
                _controller.categories[i] = categoryEntries[i];
            _controller.applyButton = applyBtn;
            _controller.resetButton = resetBtn;
            _controller.mainMenuSceneName = mainMenuSceneName;
            _controller.activeColor = C_TEXT;
            _controller.inactiveColor = C_TEXT_DIM;
            _controller.defaultCategory = 0;
        }

        // ─── Setup helpers ─────────────────────────────────────────
        void EnsureEventSystem()
        {
            if (!ensureEventSystem) return;
#if UNITY_2023_1_OR_NEWER
            if (FindFirstObjectByType<EventSystem>() != null) return;
#else
            if (FindObjectOfType<EventSystem>() != null) return;
#endif
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        void EnsureSettingsManager()
        {
            if (!createManagerIfMissing) return;
            if (SettingsManager.Instance != null) return;

            var go = new GameObject("SettingsManager");
            go.AddComponent<SettingsManager>();
        }

        // ─── Canvas / background ───────────────────────────────────
        void BuildCanvas()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);

            _canvas = canvasGo.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        void BuildBackground(Transform parent)
        {
            var bg = CreateUI("Background", parent);
            Stretch(bg.GetComponent<RectTransform>());

            var img = bg.AddComponent<Image>();
            if (backgroundSprite != null)
            {
                img.sprite = backgroundSprite;
                img.color = new Color(0.6f, 0.6f, 0.7f, 1f);   // slight tint to blend in
                img.preserveAspect = false;
            }
            else
            {
                img.color = C_BG_DARK;
            }

            // Vignette overlay on top of bg
            var vignette = CreateUI("Vignette", parent);
            Stretch(vignette.GetComponent<RectTransform>());
            var vimg = vignette.AddComponent<Image>();
            vimg.color = new Color(0, 0, 0, 0.45f);
            vimg.raycastTarget = false;
        }

        void BuildScanlines(Transform parent)
        {
            // Subtle horizontal scanline overlay using a tiny tiled sprite (1x2 transparent texture).
            var tex = new Texture2D(1, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
            tex.SetPixel(0, 0, new Color(0, 0, 0, 0.18f));
            tex.SetPixel(0, 1, new Color(0, 0, 0, 0f));
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, 1, 2), new Vector2(0.5f, 0.5f), 1f);

            var scan = CreateUI("Scanlines", parent);
            Stretch(scan.GetComponent<RectTransform>());
            var img = scan.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Tiled;
            img.raycastTarget = false;
        }

        // ─── Header ────────────────────────────────────────────────
        void BuildHeader(Transform parent)
        {
            var header = CreateUI("Header", parent);
            var rt = header.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -32);
            rt.sizeDelta = new Vector2(-96, 80);

            // Underline
            var underline = CreateUI("Underline", header.transform);
            var urt = underline.GetComponent<RectTransform>();
            urt.anchorMin = new Vector2(0, 0);
            urt.anchorMax = new Vector2(1, 0);
            urt.pivot = new Vector2(0.5f, 0);
            urt.sizeDelta = new Vector2(0, 2);
            var uimg = underline.AddComponent<Image>();
            uimg.color = C_BORDER;

            // Title
            var title = CreateText("Title", header.transform, "SETTINGS", headerFont, 36, C_TEXT, TextAlignmentOptions.Left);
            var trt = title.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0, 0);
            trt.anchorMax = new Vector2(0, 1);
            trt.pivot = new Vector2(0, 0.5f);
            trt.anchoredPosition = new Vector2(0, 0);
            trt.sizeDelta = new Vector2(600, 0);

            // Subtitle
            var sub = CreateText("Subtitle", header.transform, "// SYSTEM CONFIG", bodyFont, 28, C_ACCENT, TextAlignmentOptions.Right);
            var srt = sub.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(1, 0);
            srt.anchorMax = new Vector2(1, 1);
            srt.pivot = new Vector2(1, 0.5f);
            srt.anchoredPosition = new Vector2(0, 0);
            srt.sizeDelta = new Vector2(600, 0);
        }

        // ─── Body: categories + panels ─────────────────────────────
        (List<SettingsUIController.CategoryEntry> categories, Dictionary<string, GameObject> panels)
            BuildBody(Transform parent)
        {
            var body = CreateUI("Body", parent);
            var brt = body.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0, 0);
            brt.anchorMax = new Vector2(1, 1);
            brt.offsetMin = new Vector2(48, 80);
            brt.offsetMax = new Vector2(-48, -130);

            var hlg = body.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 32;
            hlg.childAlignment = TextAnchor.UpperLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // ── CATEGORIES sidebar
            var sidebar = CreateUI("Categories", body.transform);
            var sleg = sidebar.AddComponent<LayoutElement>();
            sleg.preferredWidth = 240;
            sleg.flexibleWidth = 0;

            var vlg = sidebar.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 4;
            vlg.padding = new RectOffset(0, 0, 12, 0);
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var categoryEntries = new List<SettingsUIController.CategoryEntry>();
            string[] catNames = { "DISPLAY", "AUDIO", "CONTROLS", "GAMEPLAY", "LANGUAGE" };

            // ── PANELS container
            var panelsContainer = CreateUI("PanelsContainer", body.transform);
            var pleg = panelsContainer.AddComponent<LayoutElement>();
            pleg.flexibleWidth = 1;

            var panelBg = panelsContainer.AddComponent<Image>();
            panelBg.color = C_PANEL_BG;
            AddOutline(panelsContainer, C_BORDER);

            var panelMap = new Dictionary<string, GameObject>();
            foreach (var name in catNames)
            {
                var entry = BuildCategoryButton(sidebar.transform, name);
                var panel = BuildPanel(panelsContainer.transform, name);
                panel.SetActive(false);
                entry.panel = panel;
                categoryEntries.Add(entry);
                panelMap[name] = panel;
            }
            categoryEntries[0].panel.SetActive(true);

            // Fill each panel with its settings
            FillDisplayPanel(panelMap["DISPLAY"]);
            FillAudioPanel(panelMap["AUDIO"]);
            FillControlsPanel(panelMap["CONTROLS"]);
            FillGameplayPanel(panelMap["GAMEPLAY"]);
            FillLanguagePanel(panelMap["LANGUAGE"]);

            return (categoryEntries, panelMap);
        }

        SettingsUIController.CategoryEntry BuildCategoryButton(Transform parent, string name)
        {
            var go = CreateUI(name, parent);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 48;

            var img = go.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0); // transparent — will tint on hover via ColorBlock

            var btn = go.AddComponent<Button>();
            var cb = btn.colors;
            cb.normalColor = new Color(0, 0, 0, 0);
            cb.highlightedColor = new Color(C_ACCENT.r, C_ACCENT.g, C_ACCENT.b, 0.10f);
            cb.pressedColor = new Color(C_ACCENT.r, C_ACCENT.g, C_ACCENT.b, 0.22f);
            cb.selectedColor = new Color(C_ACCENT.r, C_ACCENT.g, C_ACCENT.b, 0.10f);
            cb.disabledColor = new Color(0, 0, 0, 0);
            btn.colors = cb;
            btn.targetGraphic = img;

            // Left accent bar
            var bar = CreateUI("AccentBar", go.transform);
            var brt = bar.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0, 0);
            brt.anchorMax = new Vector2(0, 1);
            brt.pivot = new Vector2(0, 0.5f);
            brt.sizeDelta = new Vector2(3, 0);
            var bimg = bar.AddComponent<Image>();
            bimg.color = C_ACCENT;
            bimg.raycastTarget = false;

            // Label
            var label = CreateText("Label", go.transform, name, headerFont, 16, C_TEXT_DIM, TextAlignmentOptions.Left);
            var lrt = label.GetComponent<RectTransform>();
            Stretch(lrt);
            lrt.offsetMin = new Vector2(20, 0);
            lrt.offsetMax = new Vector2(-12, 0);
            label.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

            return new SettingsUIController.CategoryEntry
            {
                name = name,
                categoryButton = btn,
                categoryLabel = label.GetComponent<TMP_Text>(),
                // panel is set by caller
            };
        }

        GameObject BuildPanel(Transform parent, string name)
        {
            var panel = CreateUI("Panel_" + name, parent);
            Stretch(panel.GetComponent<RectTransform>());
            panel.GetComponent<RectTransform>().offsetMin = new Vector2(20, 20);
            panel.GetComponent<RectTransform>().offsetMax = new Vector2(-20, -20);

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Panel header
            var hdr = CreateText("Header", panel.transform, "▸ " + name + " SETTINGS", headerFont, 18, C_ACCENT, TextAlignmentOptions.Left);
            var le = hdr.AddComponent<LayoutElement>();
            le.preferredHeight = 48;

            return panel;
        }

        // ─── Panel content ─────────────────────────────────────────
        void FillDisplayPanel(GameObject panel)
        {
            BuildResolutionRow(panel.transform, "Resolution");
            BuildDropdownRow(panel.transform, "Window Mode", "windowMode",
                               new[] { "FULLSCREEN", "BORDERLESS", "WINDOWED" });
            BuildToggleRow(panel.transform, "VSync", "vsync");
            BuildDropdownRow(panel.transform, "FPS Cap", "fpsCapIndex",
                               new[] { "30", "60", "120", "144", "240", "UNLIMITED" });
            BuildToggleRow(panel.transform, "Pixel Perfect Camera", "pixelPerfect");
            BuildVolumeRow(panel.transform, "Brightness", "brightness");
        }

        void FillAudioPanel(GameObject panel)
        {
            BuildVolumeRow(panel.transform, "Master Volume", "masterVolume");
            BuildVolumeRow(panel.transform, "Music", "musicVolume");
            BuildVolumeRow(panel.transform, "SFX", "sfxVolume");
            BuildVolumeRow(panel.transform, "Engine Sounds", "engineVolume");
            BuildVolumeRow(panel.transform, "UI Sounds", "uiVolume");
            BuildToggleRow(panel.transform, "Mute When Unfocused", "muteWhenUnfocused");
        }

        void FillControlsPanel(GameObject panel)
        {
            BuildVolumeRow(panel.transform, "Steering Sensitivity", "steeringSensitivity");
            BuildToggleRow(panel.transform, "Invert Camera", "invertCamera");
            BuildToggleRow(panel.transform, "Controller Vibration", "controllerVibration");

            var subHdr = CreateText("Subheader", panel.transform, "▸ KEY BINDINGS", headerFont, 14, C_ACCENT, TextAlignmentOptions.Left);
            var le = subHdr.AddComponent<LayoutElement>();
            le.preferredHeight = 40;

            BuildKeyRebindRow(panel.transform, "Accelerate", "keyAccelerate");
            BuildKeyRebindRow(panel.transform, "Brake / Reverse", "keyBrake");
            BuildKeyRebindRow(panel.transform, "Steer Left", "keyLeft");
            BuildKeyRebindRow(panel.transform, "Steer Right", "keyRight");
            BuildKeyRebindRow(panel.transform, "Handbrake", "keyHandbrake");
            BuildKeyRebindRow(panel.transform, "Nitrous", "keyNitrous");
            BuildKeyRebindRow(panel.transform, "Look Back", "keyLookBack");
        }

        void FillGameplayPanel(GameObject panel)
        {
            BuildDropdownRow(panel.transform, "Difficulty", "difficulty",
                              new[] { "EASY", "NORMAL", "HARD", "SIMULATION" });
            BuildDropdownRow(panel.transform, "Speed Unit", "speedUnit",
                              new[] { "KM/H", "MPH" });
            BuildDropdownRow(panel.transform, "Default Camera", "defaultCamera",
                              new[] { "CHASE", "HOOD", "COCKPIT", "BUMPER" });
            BuildToggleRow(panel.transform, "HUD Visibility", "hudVisible");
            BuildToggleRow(panel.transform, "ABS Assist", "absAssist");
            BuildToggleRow(panel.transform, "Traction Control", "tractionControl");
            BuildToggleRow(panel.transform, "Auto Brake", "autoBrake");
        }

        void FillLanguagePanel(GameObject panel)
        {
            BuildDropdownRow(panel.transform, "Language", "language",
                             new[] { "ENGLISH", "MAGYAR", "DEUTSCH", "ESPAÑOL", "日本語" });
            BuildToggleRow(panel.transform, "Subtitles", "subtitles");
        }

        // ─── Row builders ──────────────────────────────────────────
        GameObject CreateRow(string name, Transform parent, out GameObject controlSlot)
        {
            var row = CreateUI(name, parent);
            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 50;

            var img = row.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0);

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 24;
            hlg.padding = new RectOffset(12, 12, 8, 8);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            var labelGo = CreateText("Label", row.transform, name, bodyFont, 24, C_TEXT, TextAlignmentOptions.MidlineLeft);
            var llé = labelGo.AddComponent<LayoutElement>();
            llé.flexibleWidth = 1;

            controlSlot = CreateUI("Control", row.transform);
            var clé = controlSlot.AddComponent<LayoutElement>();
            clé.preferredWidth = 280;
            clé.flexibleWidth = 0;

            return row;
        }

        void BuildToggleRow(Transform parent, string label, string settingKey)
        {
            var row = CreateRow(label, parent, out var slot);
            row.GetComponentInChildren<TMP_Text>().text = label;

            var btnGo = CreateUI("Toggle", slot.transform);
            Stretch(btnGo.GetComponent<RectTransform>());
            var img = btnGo.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.4f);
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;
            AddOutline(btnGo, C_BORDER);

            var txtGo = CreateText("Value", btnGo.transform, "ON", monoFont, 14, C_ON, TextAlignmentOptions.Center);
            Stretch(txtGo.GetComponent<RectTransform>());

            var toggle = btnGo.AddComponent<SettingsToggle>();
            toggle.settingKey = settingKey;
            toggle.button = btn;
            toggle.label = txtGo.GetComponent<TMP_Text>();
            toggle.onColor = C_ON;
            toggle.offColor = C_OFF;
            toggle.applyImmediately = false;
        }

        void BuildDropdownRow(Transform parent, string label, string settingKey, string[] options)
        {
            var row = CreateRow(label, parent, out var slot);
            row.GetComponentInChildren<TMP_Text>().text = label;

            var ddGo = CreateUI("Dropdown", slot.transform);
            Stretch(ddGo.GetComponent<RectTransform>());
            var img = ddGo.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.4f);
            AddOutline(ddGo, C_BORDER);

            var hlg = ddGo.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.padding = new RectOffset(10, 10, 6, 6);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            var leftBtn = BuildArrowButton(ddGo.transform, "◄", out var leftBtnComp);
            var valueGo = CreateText("Value", ddGo.transform, options.Length > 0 ? options[0] : "", monoFont, 14, C_TEXT, TextAlignmentOptions.Center);
            var vle = valueGo.AddComponent<LayoutElement>();
            vle.flexibleWidth = 1;
            var rightBtn = BuildArrowButton(ddGo.transform, "►", out var rightBtnComp);

            var dd = ddGo.AddComponent<SettingsDropdown>();
            dd.settingKey = settingKey;
            dd.options = options;
            dd.valueLabel = valueGo.GetComponent<TMP_Text>();
            dd.leftArrow = leftBtnComp;
            dd.rightArrow = rightBtnComp;
            dd.applyImmediately = false;
        }

        void BuildResolutionRow(Transform parent, string label)
        {
            var row = CreateRow(label, parent, out var slot);
            row.GetComponentInChildren<TMP_Text>().text = label;

            var ddGo = CreateUI("ResolutionDropdown", slot.transform);
            Stretch(ddGo.GetComponent<RectTransform>());
            var img = ddGo.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.4f);
            AddOutline(ddGo, C_BORDER);

            var hlg = ddGo.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.padding = new RectOffset(10, 10, 6, 6);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            BuildArrowButton(ddGo.transform, "◄", out var leftComp);
            var valueGo = CreateText("Value", ddGo.transform, "1920 x 1080", monoFont, 14, C_TEXT, TextAlignmentOptions.Center);
            var vle = valueGo.AddComponent<LayoutElement>();
            vle.flexibleWidth = 1;
            BuildArrowButton(ddGo.transform, "►", out var rightComp);

            var rd = ddGo.AddComponent<SettingsResolutionDropdown>();
            rd.valueLabel = valueGo.GetComponent<TMP_Text>();
            rd.leftArrow = leftComp;
            rd.rightArrow = rightComp;
        }

        void BuildVolumeRow(Transform parent, string label, string settingKey)
        {
            var row = CreateRow(label, parent, out var slot);
            row.GetComponentInChildren<TMP_Text>().text = label;

            var barGo = CreateUI("VolumeBar", slot.transform);
            Stretch(barGo.GetComponent<RectTransform>());
            var img = barGo.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.4f);
            AddOutline(barGo, C_BORDER);

            var hlg = barGo.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.padding = new RectOffset(10, 10, 6, 6);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            var segContainer = CreateUI("Segments", barGo.transform);
            var shlg = segContainer.AddComponent<HorizontalLayoutGroup>();
            shlg.spacing = 3;
            shlg.childAlignment = TextAnchor.MiddleLeft;
            shlg.childControlWidth = false;
            shlg.childControlHeight = false;
            shlg.childForceExpandWidth = false;
            shlg.childForceExpandHeight = false;
            var segLe = segContainer.AddComponent<LayoutElement>();
            segLe.preferredWidth = 14 * 10 + 3 * 9;
            segLe.preferredHeight = 22;

            var segments = new Image[10];
            for (int i = 0; i < 10; i++)
            {
                var seg = CreateUI("Seg" + i, segContainer.transform);
                var sle = seg.AddComponent<LayoutElement>();
                sle.preferredWidth = 14;
                sle.preferredHeight = 22;
                var simg = seg.AddComponent<Image>();
                simg.color = C_VOL_EMPTY;
                segments[i] = simg;
            }

            var pctGo = CreateText("Percent", barGo.transform, "0%", monoFont, 14, C_ACCENT, TextAlignmentOptions.Right);
            var pct = pctGo.AddComponent<LayoutElement>();
            pct.preferredWidth = 50;

            var bar = barGo.AddComponent<SettingsVolumeBar>();
            bar.settingKey = settingKey;
            bar.segments = segments;
            bar.percentLabel = pctGo.GetComponent<TMP_Text>();
            bar.filledColor = C_VOL_FILLED;
            bar.highColor = C_VOL_HIGH;
            bar.emptyColor = C_VOL_EMPTY;
            bar.applyImmediately = true;
        }

        void BuildKeyRebindRow(Transform parent, string label, string settingKey)
        {
            var row = CreateRow(label, parent, out var slot);
            row.GetComponentInChildren<TMP_Text>().text = label;
            row.GetComponent<LayoutElement>().preferredHeight = 36;

            // Force a smaller control area for rebind keys
            slot.GetComponent<LayoutElement>().preferredWidth = 140;

            var btnGo = CreateUI("KeyButton", slot.transform);
            Stretch(btnGo.GetComponent<RectTransform>());
            var img = btnGo.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.4f);
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;
            AddOutline(btnGo, C_BORDER);

            var txtGo = CreateText("KeyLabel", btnGo.transform, "?", monoFont, 14, C_ACCENT, TextAlignmentOptions.Center);
            Stretch(txtGo.GetComponent<RectTransform>());

            var rebind = btnGo.AddComponent<SettingsKeyRebind>();
            rebind.settingKey = settingKey;
            rebind.rebindButton = btn;
            rebind.keyLabel = txtGo.GetComponent<TMP_Text>();
        }

        GameObject BuildArrowButton(Transform parent, string symbol, out Button btnComp)
        {
            var go = CreateUI("Arrow_" + symbol, parent);
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 26;
            le.preferredHeight = 26;
            var img = go.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0);
            btnComp = go.AddComponent<Button>();
            btnComp.targetGraphic = img;

            var t = CreateText("Symbol", go.transform, symbol, monoFont, 14, C_ACCENT, TextAlignmentOptions.Center);
            Stretch(t.GetComponent<RectTransform>());
            return go;
        }

        // ─── Button bar (Apply / Reset) ────────────────────────────
        (Button apply, Button reset) BuildButtonBar(Transform parent)
        {
            var bar = CreateUI("ButtonBar", parent);
            var rt = bar.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(1, 0);
            rt.sizeDelta = new Vector2(360, 50);
            rt.anchoredPosition = new Vector2(-48, 70);

            var hlg = bar.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleRight;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            var resetBtn = BuildBarButton(bar.transform, "RESET", primary: false);
            var applyBtn = BuildBarButton(bar.transform, "APPLY", primary: true);

            return (applyBtn, resetBtn);
        }

        Button BuildBarButton(Transform parent, string label, bool primary)
        {
            var go = CreateUI(label, parent);
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 130;
            le.preferredHeight = 44;

            var img = go.AddComponent<Image>();
            img.color = primary
                ? new Color(C_ACCENT.r, C_ACCENT.g, C_ACCENT.b, 0.25f)
                : new Color(0, 0, 0, 0.4f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            AddOutline(go, primary ? C_ACCENT : C_BORDER);

            var t = CreateText("Label", go.transform, label, monoFont, 14, C_TEXT, TextAlignmentOptions.Center);
            Stretch(t.GetComponent<RectTransform>());
            return btn;
        }

        // ─── Footer ────────────────────────────────────────────────
        void BuildFooter(Transform parent)
        {
            var footer = CreateUI("Footer", parent);
            var rt = footer.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(-96, 40);
            rt.anchoredPosition = new Vector2(0, 24);

            var hlg = footer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 32;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            string[] hints =
            {
                "<color=#b794f6>W▲ S▼</color>  NAVIGATE",
                "<color=#b794f6>A◄ D►</color>  CHANGE",
                "<color=#b794f6>ENTER</color>  SELECT",
                "<color=#b794f6>ESC</color>  BACK",
            };
            foreach (var h in hints)
            {
                var t = CreateText("Hint", footer.transform, h, monoFont, 12, C_TEXT_DIM, TextAlignmentOptions.Center);
                t.GetComponent<TextMeshProUGUI>().richText = true;
                var le = t.AddComponent<LayoutElement>();
                le.preferredHeight = 30;
            }
        }

        // ─── Generic helpers ───────────────────────────────────────
        GameObject CreateUI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        GameObject CreateText(string name, Transform parent, string text,
                              TMP_FontAsset font, float size, Color color, TextAlignmentOptions align)
        {
            var go = CreateUI(name, parent);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.raycastTarget = false;
            if (font != null) t.font = font;
            return go;
        }

        void AddOutline(GameObject go, Color color)
        {
            var outline = go.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(1, -1);
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}