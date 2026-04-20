using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

// Pause menu a Drive (gameplay) scene-hez.
// ESC megnyitja/bezárja. W/S vagy fel/le nyilakkal lehet navigálni, ENTER/Space választ.
// Opciók: RESUME, RESTART, SETTINGS, EXIT (vissza a Garázsba).
// A scriptet rakd rá egy üres GameObject-re a DayDrive és NightDrive scene-ekben.
// A régi EXIT gombot pedig nyugodtan töröld a UI-ról.
public class PauseMenu : MonoBehaviour
{
    [Header("Beállítások")]
    [Tooltip("Annak a scene-nek a neve, ahova az EXIT gomb visszadob.")]
    public string exitScene = "Garage";

    [Tooltip("Ha van Settings scene, ide írd a nevét. Ha üresen hagyod, 'COMING SOON' popup jön be.")]
    public string settingsScene = "";

    [Header("Stílus")]
    public Color colActiveText = new Color(1f, 1f, 1f, 1f);
    public Color colActiveBG = new Color(0.15f, 0.05f, 0.3f, 0.92f);
    public Color colActiveAccent = new Color(0.55f, 0.1f, 1f, 1f);
    public Color colIdleText = new Color(0.8f, 0.8f, 0.85f, 0.85f);
    public Color colIdleBG = new Color(0f, 0f, 0f, 0.55f);
    public Color colIdleAccent = new Color(0.35f, 0.35f, 0.45f, 0.7f);
    public Color colPinkAccent = new Color(1f, 0.2f, 0.6f, 1f);

    [Header("Animáció")]
    public float flickerSpeed = 4f;
    public float flickerStrength = 0.25f;

    private Canvas canvas;
    private GameObject pausePanel;
    private GameObject popupPanel;
    private CanvasGroup pauseGroup;

    private string[] options = { "RESUME", "RESTART", "SETTINGS", "EXIT" };
    private GameObject[] buttonObjects;
    private Image[] buttonBGs;
    private TextMeshProUGUI[] buttonTexts;
    private Image[] buttonAccents; // bal oldali pink vonal

    private int focused = 0;
    private bool paused = false;
    private bool popupOpen = false;
    private float flickerTimer = 0f;

    void Awake()
    {
        BuildCanvas();
        BuildPauseMenu();
        pausePanel.SetActive(false);
    }

    void Update()
    {
        // ESC kezelése
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (popupOpen)
            {
                ClosePopup();
            }
            else if (paused)
            {
                Resume();
            }
            else
            {
                OpenPause();
            }
            return;
        }

        if (!paused) return;
        if (popupOpen)
        {
            // Popup-ban ENTER/Space bezár
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                ClosePopup();
            }
            return;
        }

        // Navigáció
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            focused = (focused + 1) % options.Length;
            flickerTimer = 0f;
        }
        else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            focused = (focused - 1 + options.Length) % options.Length;
            flickerTimer = 0f;
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
        {
            Activate(focused);
        }

        // Animáció (unscaled idővel, mert Time.timeScale = 0)
        flickerTimer += Time.unscaledDeltaTime;
        AnimateButtons();
    }

    void AnimateButtons()
    {
        float pulse = (Mathf.Sin(flickerTimer * flickerSpeed) + 1f) * 0.5f; // 0..1
        for (int i = 0; i < options.Length; i++)
        {
            if (i == focused)
            {
                Color bg = Color.Lerp(colActiveBG * 0.85f, colActiveBG, pulse);
                Color accent = Color.Lerp(colActiveAccent * 0.8f, colActiveAccent, pulse);
                if (buttonBGs[i] != null) buttonBGs[i].color = bg;
                if (buttonAccents[i] != null) buttonAccents[i].color = accent;
                if (buttonTexts[i] != null) buttonTexts[i].color = colActiveText;
            }
            else
            {
                if (buttonBGs[i] != null) buttonBGs[i].color = colIdleBG;
                if (buttonAccents[i] != null) buttonAccents[i].color = colIdleAccent;
                if (buttonTexts[i] != null) buttonTexts[i].color = colIdleText;
            }
        }
    }

    void Activate(int idx)
    {
        switch (idx)
        {
            case 0: Resume(); break;
            case 1: Restart(); break;
            case 2: OpenSettings(); break;
            case 3: ExitToGarage(); break;
        }
    }

    public void OpenPause()
    {
        paused = true;
        focused = 0;
        flickerTimer = 0f;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    public void Resume()
    {
        paused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OpenSettings()
    {
        if (!string.IsNullOrEmpty(settingsScene))
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            SceneManager.LoadScene(settingsScene);
        }
        else
        {
            BuildPopup("SETTINGS", "COMING SOON");
        }
    }

    public void ExitToGarage()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(exitScene);
    }

    // ====================================================================
    //  UI BUILDERS
    // ====================================================================

    void BuildCanvas()
    {
        // Saját Canvas, hogy mindig felül legyen a többi UI-on
        GameObject canvasGO = new GameObject("PauseMenuCanvas");
        canvasGO.transform.SetParent(transform, false);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();
    }

    void BuildPauseMenu()
    {
        pausePanel = new GameObject("PausePanel");
        pausePanel.transform.SetParent(canvas.transform, false);
        RectTransform prt = pausePanel.AddComponent<RectTransform>();
        StretchFull(prt);

        pauseGroup = pausePanel.AddComponent<CanvasGroup>();
        pauseGroup.alpha = 1f;

        // Sötétítő háttér
        GameObject dim = new GameObject("Dim");
        dim.transform.SetParent(pausePanel.transform, false);
        Image dimImg = dim.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.7f);
        StretchFull(dim.GetComponent<RectTransform>());

        // Központi doboz
        GameObject box = new GameObject("Box");
        box.transform.SetParent(pausePanel.transform, false);
        Image boxImg = box.AddComponent<Image>();
        boxImg.color = new Color(0.05f, 0.02f, 0.1f, 0.95f);
        RectTransform boxRT = box.GetComponent<RectTransform>();
        boxRT.anchorMin = boxRT.anchorMax = new Vector2(0.5f, 0.5f);
        boxRT.pivot = new Vector2(0.5f, 0.5f);
        boxRT.sizeDelta = new Vector2(540f, 520f);
        boxRT.anchoredPosition = Vector2.zero;

        // Felső pink vonal
        GameObject topLine = new GameObject("TopLine");
        topLine.transform.SetParent(box.transform, false);
        Image topImg = topLine.AddComponent<Image>();
        topImg.color = colPinkAccent;
        RectTransform topRT = topLine.GetComponent<RectTransform>();
        topRT.anchorMin = new Vector2(0f, 1f);
        topRT.anchorMax = new Vector2(1f, 1f);
        topRT.pivot = new Vector2(0.5f, 1f);
        topRT.sizeDelta = new Vector2(0f, 4f);
        topRT.anchoredPosition = Vector2.zero;

        // Alsó pink vonal
        GameObject botLine = new GameObject("BottomLine");
        botLine.transform.SetParent(box.transform, false);
        Image botImg = botLine.AddComponent<Image>();
        botImg.color = colPinkAccent;
        RectTransform botRT = botLine.GetComponent<RectTransform>();
        botRT.anchorMin = new Vector2(0f, 0f);
        botRT.anchorMax = new Vector2(1f, 0f);
        botRT.pivot = new Vector2(0.5f, 0f);
        botRT.sizeDelta = new Vector2(0f, 4f);
        botRT.anchoredPosition = Vector2.zero;

        // Cím
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(box.transform, false);
        TextMeshProUGUI title = titleGO.AddComponent<TextMeshProUGUI>();
        title.text = "PAUSED";
        title.fontSize = 56f;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.color = colActiveAccent;
        RectTransform titleRT = title.rectTransform;
        titleRT.anchorMin = new Vector2(0f, 1f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.sizeDelta = new Vector2(0f, 90f);
        titleRT.anchoredPosition = new Vector2(0f, -30f);

        // Gombok
        buttonObjects = new GameObject[options.Length];
        buttonBGs = new Image[options.Length];
        buttonTexts = new TextMeshProUGUI[options.Length];
        buttonAccents = new Image[options.Length];

        float btnW = 420f;
        float btnH = 64f;
        float spacing = 18f;
        float startY = -150f;

        for (int i = 0; i < options.Length; i++)
        {
            GameObject btn = new GameObject("Btn_" + options[i]);
            btn.transform.SetParent(box.transform, false);
            Image bg = btn.AddComponent<Image>();
            bg.color = colIdleBG;
            RectTransform rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(btnW, btnH);
            rt.anchoredPosition = new Vector2(0f, startY - i * (btnH + spacing));

            // Bal oldali accent vonal
            GameObject accent = new GameObject("Accent");
            accent.transform.SetParent(btn.transform, false);
            Image accentImg = accent.AddComponent<Image>();
            accentImg.color = colIdleAccent;
            RectTransform accRT = accent.GetComponent<RectTransform>();
            accRT.anchorMin = new Vector2(0f, 0f);
            accRT.anchorMax = new Vector2(0f, 1f);
            accRT.pivot = new Vector2(0f, 0.5f);
            accRT.sizeDelta = new Vector2(6f, 0f);
            accRT.anchoredPosition = Vector2.zero;

            // Szöveg
            GameObject txtGO = new GameObject("Text");
            txtGO.transform.SetParent(btn.transform, false);
            TextMeshProUGUI t = txtGO.AddComponent<TextMeshProUGUI>();
            t.text = options[i];
            t.fontSize = 32f;
            t.fontStyle = FontStyles.Bold;
            t.alignment = TextAlignmentOptions.Center;
            t.color = colIdleText;
            RectTransform txtRT = t.rectTransform;
            StretchFull(txtRT);

            // Klikkelhetőség (egér is működjön)
            Button bComp = btn.AddComponent<Button>();
            int captured = i;
            bComp.onClick.AddListener(() =>
            {
                focused = captured;
                Activate(captured);
            });

            buttonObjects[i] = btn;
            buttonBGs[i] = bg;
            buttonTexts[i] = t;
            buttonAccents[i] = accentImg;
        }

        // Hint alul
        GameObject hintGO = new GameObject("Hint");
        hintGO.transform.SetParent(box.transform, false);
        TextMeshProUGUI hint = hintGO.AddComponent<TextMeshProUGUI>();
        hint.text = "W/S vagy ↑/↓  •  ENTER kiválaszt  •  ESC bezár";
        hint.fontSize = 18f;
        hint.alignment = TextAlignmentOptions.Center;
        hint.color = new Color(0.7f, 0.7f, 0.8f, 0.7f);
        RectTransform hintRT = hint.rectTransform;
        hintRT.anchorMin = new Vector2(0f, 0f);
        hintRT.anchorMax = new Vector2(1f, 0f);
        hintRT.pivot = new Vector2(0.5f, 0f);
        hintRT.sizeDelta = new Vector2(0f, 30f);
        hintRT.anchoredPosition = new Vector2(0f, 18f);
    }

    void BuildPopup(string title, string message)
    {
        if (popupPanel != null) Destroy(popupPanel);

        popupOpen = true;
        popupPanel = new GameObject("Popup");
        popupPanel.transform.SetParent(canvas.transform, false);
        RectTransform prt = popupPanel.AddComponent<RectTransform>();
        StretchFull(prt);

        // Sötétítő
        GameObject dim = new GameObject("Dim");
        dim.transform.SetParent(popupPanel.transform, false);
        Image dimImg = dim.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.6f);
        StretchFull(dim.GetComponent<RectTransform>());

        // Doboz
        GameObject box = new GameObject("Box");
        box.transform.SetParent(popupPanel.transform, false);
        Image boxImg = box.AddComponent<Image>();
        boxImg.color = new Color(0.05f, 0.02f, 0.1f, 0.95f);
        RectTransform boxRT = box.GetComponent<RectTransform>();
        boxRT.anchorMin = boxRT.anchorMax = new Vector2(0.5f, 0.5f);
        boxRT.pivot = new Vector2(0.5f, 0.5f);
        boxRT.sizeDelta = new Vector2(440f, 230f);
        boxRT.anchoredPosition = Vector2.zero;

        // Felső pink vonal
        GameObject topLine = new GameObject("TopLine");
        topLine.transform.SetParent(box.transform, false);
        Image topImg = topLine.AddComponent<Image>();
        topImg.color = colPinkAccent;
        RectTransform topRT = topLine.GetComponent<RectTransform>();
        topRT.anchorMin = new Vector2(0f, 1f);
        topRT.anchorMax = new Vector2(1f, 1f);
        topRT.pivot = new Vector2(0.5f, 1f);
        topRT.sizeDelta = new Vector2(0f, 3f);
        topRT.anchoredPosition = Vector2.zero;

        // Cím
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(box.transform, false);
        TextMeshProUGUI t = titleGO.AddComponent<TextMeshProUGUI>();
        t.text = title;
        t.fontSize = 36f;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color = colActiveAccent;
        RectTransform tRT = t.rectTransform;
        tRT.anchorMin = new Vector2(0f, 1f);
        tRT.anchorMax = new Vector2(1f, 1f);
        tRT.pivot = new Vector2(0.5f, 1f);
        tRT.sizeDelta = new Vector2(0f, 60f);
        tRT.anchoredPosition = new Vector2(0f, -20f);

        // Üzenet
        GameObject msgGO = new GameObject("Msg");
        msgGO.transform.SetParent(box.transform, false);
        TextMeshProUGUI m = msgGO.AddComponent<TextMeshProUGUI>();
        m.text = message;
        m.fontSize = 24f;
        m.alignment = TextAlignmentOptions.Center;
        m.color = colActiveText;
        RectTransform mRT = m.rectTransform;
        mRT.anchorMin = new Vector2(0f, 0.5f);
        mRT.anchorMax = new Vector2(1f, 0.5f);
        mRT.pivot = new Vector2(0.5f, 0.5f);
        mRT.sizeDelta = new Vector2(0f, 60f);
        mRT.anchoredPosition = new Vector2(0f, 0f);

        // OK gomb
        GameObject ok = new GameObject("OK");
        ok.transform.SetParent(box.transform, false);
        Image okBG = ok.AddComponent<Image>();
        okBG.color = colActiveBG;
        RectTransform okRT = ok.GetComponent<RectTransform>();
        okRT.anchorMin = new Vector2(0.5f, 0f);
        okRT.anchorMax = new Vector2(0.5f, 0f);
        okRT.pivot = new Vector2(0.5f, 0f);
        okRT.sizeDelta = new Vector2(160f, 44f);
        okRT.anchoredPosition = new Vector2(0f, 22f);

        GameObject okTxtGO = new GameObject("Text");
        okTxtGO.transform.SetParent(ok.transform, false);
        TextMeshProUGUI okTxt = okTxtGO.AddComponent<TextMeshProUGUI>();
        okTxt.text = "OK";
        okTxt.fontSize = 26f;
        okTxt.fontStyle = FontStyles.Bold;
        okTxt.alignment = TextAlignmentOptions.Center;
        okTxt.color = colActiveText;
        StretchFull(okTxt.rectTransform);

        Button okBtn = ok.AddComponent<Button>();
        okBtn.onClick.AddListener(ClosePopup);
    }

    void ClosePopup()
    {
        if (popupPanel != null) Destroy(popupPanel);
        popupOpen = false;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void OnDisable()
    {
        // Biztonság — ha valamiért disable-ölődik a script, ne maradjon a játék pause-ban
        if (paused)
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }
}
