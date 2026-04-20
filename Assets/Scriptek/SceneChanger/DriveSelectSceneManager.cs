using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DriveSelectSceneManager : MonoBehaviour
{
    [Header("Scene Nevek")]
    [SerializeField] private string dayDriveSceneName = "DayDrive";
    [SerializeField] private string nightDriveSceneName = "NightDrive";
    [SerializeField] private string garageSceneName = "Garage";

    [Header("Beállítások")]
    [SerializeField] private float itemSpacing = 70f;
    [SerializeField] private float flickerSpeed = 3.5f;

    private readonly Color colActiveDark = new Color(0.55f, 0.1f, 1f, 1f);
    private readonly Color colActiveLight = new Color(1f, 1f, 1f, 1f);
    private readonly Color colPink = new Color(1f, 0.42f, 0.82f, 1f);
    private readonly Color colErr = new Color(1f, 0.4f, 0.45f, 1f);

    // Két állapot: fő menü vagy Street Race al-menü
    private enum Mode { Main, StreetSub }
    private Mode currentMode = Mode.Main;

    // STREET RACE legyen felül
    private readonly string[] mainLabels = { "STREET RACE", "DRAG RACE", "< RETURN" };
    private readonly string[] subLabels = { "DAYTIME", "NIGHT TIME", "< RETURN" };

    private string[] currentLabels;

    private int itemCount;
    private int currentIndex = 0;

    private RectTransform[] itemRects;
    private TextMeshProUGUI[] menuTexts;
    private Image[] itemBGs;
    private Image[] itemAccents;
    private Button[] itemButtons;

    private CanvasGroup navGroup;
    private GameObject menuContainer;

    // Felugró "Not available" ablak
    private GameObject popupRoot;
    private CanvasGroup popupGroup;

    private bool isTransitioning = false;
    private bool popupOpen = false;
    private float flickerTimer = 0f;

    void Start()
    {
        BuildCanvas();
        BuildMenu(mainLabels);
        StartCoroutine(FadeIn());
    }

    void Update()
    {
        if (isTransitioning) return;

        // Ha a popup nyitva, csak ESC/ENTER zárja be
        if (popupOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                ClosePopup();
            }
            return;
        }

        HandleInput();
        AnimateItems();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) Navigate(-1);
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) Navigate(1);
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
            SelectCurrent();
        if (Input.GetKeyDown(KeyCode.Escape))
            GoBack();
    }

    void Navigate(int dir)
    {
        currentIndex = Mathf.Clamp(currentIndex + dir, 0, itemCount - 1);
    }

    void SelectCurrent()
    {
        if (currentMode == Mode.Main)
        {
            switch (currentIndex)
            {
                case 0: // STREET RACE → al-menü
                    SwitchToSub();
                    break;
                case 1: // DRAG RACE → felugró "Not available"
                    OpenNotAvailablePopup();
                    break;
                case 2: // < RETURN (vissza a Garage-ba)
                    LoadSceneWithTransition(garageSceneName);
                    break;
            }
        }
        else // StreetSub
        {
            switch (currentIndex)
            {
                case 0: // DAYTIME
                    LoadSceneWithTransition(dayDriveSceneName);
                    break;
                case 1: // NIGHT TIME
                    LoadSceneWithTransition(nightDriveSceneName);
                    break;
                case 2: // < RETURN (vissza a fő menühöz)
                    SwitchToMain();
                    break;
            }
        }
    }

    void GoBack()
    {
        if (currentMode == Mode.StreetSub)
            SwitchToMain();
        else
            LoadSceneWithTransition(garageSceneName);
    }

    void SwitchToSub()
    {
        currentMode = Mode.StreetSub;
        currentIndex = 0;
        RebuildMenu(subLabels);
    }

    void SwitchToMain()
    {
        currentMode = Mode.Main;
        currentIndex = 0; // STREET RACE van felül
        RebuildMenu(mainLabels);
    }

    // ---- ANIMÁCIÓ ----
    void AnimateItems()
    {
        if (menuTexts == null) return;
        flickerTimer += Time.deltaTime;

        for (int i = 0; i < itemCount; i++)
        {
            bool active = (i == currentIndex);
            float dist = Mathf.Abs(i - currentIndex);

            if (itemRects[i] != null)
                itemRects[i].anchoredPosition = new Vector2(0f, -(i * itemSpacing));

            if (active)
            {
                float pulse = (Mathf.Sin(flickerTimer * flickerSpeed) + 1f) / 2f;
                menuTexts[i].color = Color.Lerp(colActiveDark, colActiveLight, pulse);
                menuTexts[i].fontSize = Mathf.Lerp(menuTexts[i].fontSize, 32f, Time.deltaTime * 10f);

                if (itemBGs[i] != null)
                    itemBGs[i].color = Color.Lerp(itemBGs[i].color,
                        new Color(0.15f, 0.05f, 0.3f, 0.92f), Time.deltaTime * 8f);
                if (itemAccents[i] != null)
                    itemAccents[i].color = Color.Lerp(itemAccents[i].color,
                        new Color(0.6f, 0.2f, 1f, 1f), Time.deltaTime * 8f);
            }
            else
            {
                float fade = Mathf.Lerp(0.2f, 0.7f, 1f - Mathf.Clamp01(dist / 2f));
                menuTexts[i].color = new Color(1f, 1f, 1f, fade);
                menuTexts[i].fontSize = Mathf.Lerp(menuTexts[i].fontSize,
                    Mathf.Lerp(18f, 24f, 1f - Mathf.Clamp01(dist)), Time.deltaTime * 10f);
                if (itemBGs[i] != null)
                    itemBGs[i].color = Color.Lerp(itemBGs[i].color,
                        new Color(0f, 0f, 0f, Mathf.Lerp(0.15f, 0.55f, 1f - Mathf.Clamp01(dist))),
                        Time.deltaTime * 8f);
                if (itemAccents[i] != null)
                    itemAccents[i].color = Color.Lerp(itemAccents[i].color,
                        new Color(0.6f, 0.2f, 1f, 0.3f), Time.deltaTime * 8f);
            }
        }
    }

    // ---- SCENE VÁLTÁS ----
    void LoadSceneWithTransition(string sceneName)
    {
        isTransitioning = true;
        StartCoroutine(SelectFlashAndLoad(sceneName));
    }

    IEnumerator SelectFlashAndLoad(string sceneName)
    {
        for (int i = 0; i < 4; i++)
        {
            menuTexts[currentIndex].color = Color.white;
            yield return new WaitForSeconds(0.06f);
            menuTexts[currentIndex].color = colActiveDark;
            yield return new WaitForSeconds(0.06f);
        }

        float t = 0f;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            if (navGroup != null) navGroup.alpha = Mathf.Lerp(1f, 0f, t / 0.4f);
            yield return null;
        }

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }

    IEnumerator FadeIn()
    {
        if (navGroup == null) yield break;
        navGroup.alpha = 0f;
        float e = 0f;
        while (e < 0.6f) { e += Time.deltaTime; navGroup.alpha = e / 0.6f; yield return null; }
        navGroup.alpha = 1f;
    }

    // ---- POPUP ----
    void OpenNotAvailablePopup()
    {
        if (popupRoot == null) return;
        popupRoot.SetActive(true);
        popupOpen = true;
        StartCoroutine(FadePopup(true));
    }

    void ClosePopup()
    {
        if (popupRoot == null) return;
        StartCoroutine(FadePopupAndDisable());
    }

    IEnumerator FadePopup(bool fadeIn)
    {
        if (popupGroup == null) yield break;
        float e = 0f, dur = 0.2f;
        float from = fadeIn ? 0f : 1f;
        float to = fadeIn ? 1f : 0f;
        popupGroup.alpha = from;
        while (e < dur)
        {
            e += Time.unscaledDeltaTime;
            popupGroup.alpha = Mathf.Lerp(from, to, e / dur);
            yield return null;
        }
        popupGroup.alpha = to;
    }

    IEnumerator FadePopupAndDisable()
    {
        yield return FadePopup(false);
        popupRoot.SetActive(false);
        popupOpen = false;
    }

    // ---- UI ÉPÍTÉS ----
    void BuildCanvas()
    {
        var cgo = new GameObject("DriveSelectNavCanvas");
        cgo.transform.SetParent(transform, false);
        var cv = cgo.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 200;
        var sc = cgo.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        cgo.AddComponent<GraphicRaycaster>();
        navGroup = cgo.AddComponent<CanvasGroup>();

        // Menu container (jobb oldalt, mint a garage)
        menuContainer = new GameObject("MenuContainer");
        menuContainer.transform.SetParent(cgo.transform, false);
        var contRect = menuContainer.AddComponent<RectTransform>();
        contRect.anchorMin = new Vector2(1f, 0.5f);
        contRect.anchorMax = new Vector2(1f, 0.5f);
        contRect.pivot = new Vector2(1f, 0.5f);
        contRect.sizeDelta = new Vector2(360f, 3 * itemSpacing);
        contRect.anchoredPosition = new Vector2(-24f, 0f);

        // Alsó hint (ugyanaz mint garage)
        var hint = new GameObject("Hint");
        hint.transform.SetParent(cgo.transform, false);
        var ht = hint.AddComponent<TextMeshProUGUI>();
        ht.text = "W ▲  /  S ▼  —  NAVIGATE          ENTER  —  SELECT          ESC  —  BACK";
        ht.fontSize = 16f;
        ht.color = new Color(0.75f, 0.75f, 0.85f, 0.85f);
        ht.alignment = TextAlignmentOptions.Center;
        ht.enableWordWrapping = false;
        var hr = hint.GetComponent<RectTransform>();
        hr.anchorMin = new Vector2(0.5f, 0f); hr.anchorMax = new Vector2(0.5f, 0f);
        hr.pivot = new Vector2(0.5f, 0f);
        hr.sizeDelta = new Vector2(900f, 40f);
        hr.anchoredPosition = new Vector2(0f, 24f);

        // POPUP ("Not available")
        BuildPopup(cgo.transform);
    }

    void BuildPopup(Transform parent)
    {
        popupRoot = new GameObject("NotAvailablePopup");
        popupRoot.transform.SetParent(parent, false);
        var rootRect = popupRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        popupGroup = popupRoot.AddComponent<CanvasGroup>();
        popupGroup.alpha = 0f;

        // Sötétítő háttér (teljes képernyő)
        var dim = new GameObject("Dim");
        dim.transform.SetParent(popupRoot.transform, false);
        var dimImg = dim.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.55f);
        var dimRect = dim.GetComponent<RectTransform>();
        dimRect.anchorMin = Vector2.zero;
        dimRect.anchorMax = Vector2.one;
        dimRect.offsetMin = Vector2.zero;
        dimRect.offsetMax = Vector2.zero;
        var dimBtn = dim.AddComponent<Button>();
        dimBtn.onClick.AddListener(ClosePopup);

        // A modális doboz
        var box = new GameObject("Box");
        box.transform.SetParent(popupRoot.transform, false);
        var boxImg = box.AddComponent<Image>();
        boxImg.color = new Color(0.08f, 0.05f, 0.14f, 0.96f);
        var boxRect = box.GetComponent<RectTransform>();
        boxRect.anchorMin = new Vector2(0.5f, 0.5f);
        boxRect.anchorMax = new Vector2(0.5f, 0.5f);
        boxRect.pivot = new Vector2(0.5f, 0.5f);
        boxRect.sizeDelta = new Vector2(560f, 240f);
        boxRect.anchoredPosition = Vector2.zero;

        // Felső pink sáv
        var topLine = new GameObject("TopLine");
        topLine.transform.SetParent(box.transform, false);
        var topImg = topLine.AddComponent<Image>();
        topImg.color = colPink;
        var topRect = topLine.GetComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0f, 1f);
        topRect.anchorMax = new Vector2(1f, 1f);
        topRect.pivot = new Vector2(0.5f, 1f);
        topRect.sizeDelta = new Vector2(0f, 3f);
        topRect.anchoredPosition = Vector2.zero;

        // Alsó pink sáv
        var botLine = new GameObject("BotLine");
        botLine.transform.SetParent(box.transform, false);
        var botImg = botLine.AddComponent<Image>();
        botImg.color = colPink;
        var botRect = botLine.GetComponent<RectTransform>();
        botRect.anchorMin = new Vector2(0f, 0f);
        botRect.anchorMax = new Vector2(1f, 0f);
        botRect.pivot = new Vector2(0.5f, 0f);
        botRect.sizeDelta = new Vector2(0f, 3f);
        botRect.anchoredPosition = Vector2.zero;

        // Cím: DRAG RACE
        var titleGO = new GameObject("PopupTitle");
        titleGO.transform.SetParent(box.transform, false);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "DRAG RACE";
        titleTMP.fontSize = 34f;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = Color.white;
        titleTMP.enableWordWrapping = false;
        var titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(0f, 50f);
        titleRect.anchoredPosition = new Vector2(0f, -28f);

        // Üzenet
        var msgGO = new GameObject("PopupMsg");
        msgGO.transform.SetParent(box.transform, false);
        var msgTMP = msgGO.AddComponent<TextMeshProUGUI>();
        msgTMP.text = "NOT AVAILABLE NOW\n<size=18><color=#FFB2D9>COMING SOON...</color></size>";
        msgTMP.fontSize = 22f;
        msgTMP.fontStyle = FontStyles.Bold;
        msgTMP.alignment = TextAlignmentOptions.Center;
        msgTMP.color = colErr;
        msgTMP.richText = true;
        var msgRect = msgGO.GetComponent<RectTransform>();
        msgRect.anchorMin = new Vector2(0f, 0.5f);
        msgRect.anchorMax = new Vector2(1f, 0.5f);
        msgRect.pivot = new Vector2(0.5f, 0.5f);
        msgRect.sizeDelta = new Vector2(0f, 80f);
        msgRect.anchoredPosition = new Vector2(0f, 0f);

        // OK gomb
        var okGO = new GameObject("OkBtn");
        okGO.transform.SetParent(box.transform, false);
        var okImg = okGO.AddComponent<Image>();
        okImg.color = new Color(0.15f, 0.05f, 0.3f, 0.92f);
        var okRect = okGO.GetComponent<RectTransform>();
        okRect.anchorMin = new Vector2(0.5f, 0f);
        okRect.anchorMax = new Vector2(0.5f, 0f);
        okRect.pivot = new Vector2(0.5f, 0f);
        okRect.sizeDelta = new Vector2(200f, 44f);
        okRect.anchoredPosition = new Vector2(0f, 28f);

        var okBtn = okGO.AddComponent<Button>();
        var okColors = okBtn.colors;
        okColors.highlightedColor = new Color(0.6f, 0.2f, 1f, 0.6f);
        okColors.pressedColor = new Color(0.6f, 0.2f, 1f, 0.8f);
        okBtn.colors = okColors;
        okBtn.onClick.AddListener(ClosePopup);

        // Lila accent csík a gomb bal oldalán
        var okAccent = new GameObject("Accent");
        okAccent.transform.SetParent(okGO.transform, false);
        var okAccImg = okAccent.AddComponent<Image>();
        okAccImg.color = new Color(0.6f, 0.2f, 1f, 1f);
        var okAccRect = okAccent.GetComponent<RectTransform>();
        okAccRect.anchorMin = new Vector2(0f, 0f);
        okAccRect.anchorMax = new Vector2(0f, 1f);
        okAccRect.pivot = new Vector2(0f, 0.5f);
        okAccRect.sizeDelta = new Vector2(4f, 0f);
        okAccRect.anchoredPosition = Vector2.zero;

        // OK szöveg
        var okTxtGO = new GameObject("Label");
        okTxtGO.transform.SetParent(okGO.transform, false);
        var okTxt = okTxtGO.AddComponent<TextMeshProUGUI>();
        okTxt.text = "OK";
        okTxt.fontSize = 22f;
        okTxt.fontStyle = FontStyles.Bold;
        okTxt.alignment = TextAlignmentOptions.Center;
        okTxt.color = Color.white;
        var okTxtRect = okTxtGO.GetComponent<RectTransform>();
        okTxtRect.anchorMin = Vector2.zero;
        okTxtRect.anchorMax = Vector2.one;
        okTxtRect.offsetMin = Vector2.zero;
        okTxtRect.offsetMax = Vector2.zero;

        popupRoot.SetActive(false);
    }

    void BuildMenu(string[] labels)
    {
        currentLabels = labels;
        itemCount = labels.Length;

        var contRect = menuContainer.GetComponent<RectTransform>();
        contRect.sizeDelta = new Vector2(360f, itemCount * itemSpacing);

        itemRects = new RectTransform[itemCount];
        menuTexts = new TextMeshProUGUI[itemCount];
        itemBGs = new Image[itemCount];
        itemAccents = new Image[itemCount];
        itemButtons = new Button[itemCount];

        for (int i = 0; i < itemCount; i++)
        {
            var itemGO = new GameObject(labels[i]);
            itemGO.transform.SetParent(menuContainer.transform, false);
            var itemRect = itemGO.AddComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0f, 1f);
            itemRect.anchorMax = new Vector2(1f, 1f);
            itemRect.pivot = new Vector2(0.5f, 0.5f);
            itemRect.sizeDelta = new Vector2(0f, itemSpacing - 8f);
            itemRect.anchoredPosition = new Vector2(0f, -(i * itemSpacing));
            itemRects[i] = itemRect;

            var bg = itemGO.AddComponent<Image>();
            bg.color = i == currentIndex
                ? new Color(0.15f, 0.05f, 0.3f, 0.92f)
                : new Color(0f, 0f, 0f, 0.55f);
            itemBGs[i] = bg;

            // Bal oldali lila accent csík
            var accent = new GameObject("Accent");
            accent.transform.SetParent(itemGO.transform, false);
            var accentImg = accent.AddComponent<Image>();
            accentImg.color = new Color(0.6f, 0.2f, 1f, i == currentIndex ? 1f : 0.3f);
            var accentRect = accent.GetComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0f, 0f);
            accentRect.anchorMax = new Vector2(0f, 1f);
            accentRect.pivot = new Vector2(0f, 0.5f);
            accentRect.sizeDelta = new Vector2(4f, 0f);
            accentRect.anchoredPosition = Vector2.zero;
            itemAccents[i] = accentImg;

            // Szöveg
            var txtGO = new GameObject("Label");
            txtGO.transform.SetParent(itemGO.transform, false);
            var tmp = txtGO.AddComponent<TextMeshProUGUI>();
            tmp.text = labels[i];
            tmp.fontSize = i == currentIndex ? 32f : 22f;
            tmp.color = i == currentIndex ? colActiveDark : new Color(1f, 1f, 1f, 0.7f);
            tmp.alignment = TextAlignmentOptions.Right;
            tmp.fontStyle = FontStyles.Bold;
            tmp.enableWordWrapping = false;
            var txtRect = txtGO.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = new Vector2(-16f, 0f);
            txtRect.anchoredPosition = new Vector2(-8f, 0f);
            menuTexts[i] = tmp;

            // Button
            var btn = itemGO.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.1f);
            colors.pressedColor = new Color(0.6f, 0.2f, 1f, 0.3f);
            btn.colors = colors;
            int cap = i;
            btn.onClick.AddListener(() =>
            {
                if (popupOpen) return;
                if (currentIndex == cap) SelectCurrent();
                else currentIndex = cap;
            });
            itemButtons[i] = btn;
        }
    }

    void RebuildMenu(string[] labels)
    {
        // Régi item GO-kat pucoljuk le
        if (menuContainer != null)
        {
            for (int i = menuContainer.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(menuContainer.transform.GetChild(i).gameObject);
            }
        }
        BuildMenu(labels);
    }
}
