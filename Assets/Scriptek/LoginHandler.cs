using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoginHandler : MonoBehaviour
{
    [Header("Scene nevek")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private string registerUrl = "https://midnightracers.netlify.app";

    [Header("Beállítások")]
    [SerializeField] private float flickerSpeed = 3.5f;

    // ---- SZÍNEK ----
    private readonly Color colActiveDark = new Color(0.55f, 0.1f, 1f, 1f);
    private readonly Color colActiveLight = new Color(1f, 1f, 1f, 1f);
    private readonly Color colPink = new Color(1f, 0.42f, 0.82f, 1f);
    private readonly Color colActiveBG = new Color(0.45f, 0.18f, 0.75f, 0.8f);
    private readonly Color colIdleBG = new Color(0f, 0f, 0f, 0.35f);
    private readonly Color colErr = new Color(1f, 0.35f, 0.35f, 1f);
    private readonly Color colOk = new Color(0.45f, 1f, 0.5f, 1f);
    private readonly Color colInfo = new Color(0.85f, 0.85f, 0.9f, 1f);

    // ---- NAVIGATION INDEXEK ----
    // Login view
    private const int IDX_USER = 0;
    private const int IDX_PASS = 1;
    private const int IDX_LOGIN = 2;
    private const int IDX_REG = 3;
    private const int IDX_BACK = 4;
    private const int LOGIN_COUNT = 5;

    // Profile view
    private const int IDX_LOGOUT = 0;
    private const int IDX_PROFILE_BACK = 1;
    private const int PROFILE_COUNT = 2;

    private int currentIndex = 0;
    private int itemCount = LOGIN_COUNT;

    // ---- UI REFERENCIÁK ----
    private GameObject uiRoot;
    private CanvasGroup navGroup;

    // Login view refs
    private TMP_InputField usernameInput;
    private TMP_InputField passwordInput;
    private Image usernameBG, passwordBG;
    private TextMeshProUGUI usernameLabel, passwordLabel;
    private Image loginBtnBG, registerBtnBG, backBtnBG;
    private TextMeshProUGUI loginBtnTxt, registerBtnTxt, backBtnTxt;
    private TextMeshProUGUI feedbackText;

    // Profile view refs
    private Image logoutBtnBG, profileBackBtnBG;
    private TextMeshProUGUI logoutBtnTxt, profileBackBtnTxt;
    private TextMeshProUGUI profileFeedbackText;

    // Panel accent-ek (mindkét view-ban)
    private Image panelTopAccent;
    private Image panelBottomAccent;

    // ---- ÁLLAPOT ----
    private enum Mode { Login, Profile }
    private Mode currentMode = Mode.Login;

    private bool isBusy = false;
    private bool isTransitioning = false;
    private float flickerTimer = 0f;

    void Start()
    {
        // Ha már van DatabaseManager és be van lépve → profile view
        if (DatabaseManager.IsLoggedIn)
            ShowProfileView();
        else
            ShowLoginView();

        StartCoroutine(FadeIn());
    }

    void Update()
    {
        if (isTransitioning) return;
        HandleInput();
        AnimateItems();
    }

    // ========================================
    //          INPUT KEZELÉS
    // ========================================
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            int dir = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -1 : 1;
            Navigate(dir);
            return;
        }

        bool typingInField = currentMode == Mode.Login && (currentIndex == IDX_USER || currentIndex == IDX_PASS);

        if (!typingInField)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) Navigate(-1);
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) Navigate(1);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) Navigate(-1);
            if (Input.GetKeyDown(KeyCode.DownArrow)) Navigate(1);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            ActivateCurrent();

        if (Input.GetKeyDown(KeyCode.Escape))
            OnBackClick();
    }

    void Navigate(int dir)
    {
        if (isBusy) return;
        int newIdx = Mathf.Clamp(currentIndex + dir, 0, itemCount - 1);
        if (newIdx == currentIndex) return;
        currentIndex = newIdx;
        UpdateFieldFocus();
    }

    void UpdateFieldFocus()
    {
        if (currentMode != Mode.Login) return;

        if (currentIndex == IDX_USER && usernameInput != null)
        {
            usernameInput.Select();
            usernameInput.ActivateInputField();
        }
        else if (currentIndex == IDX_PASS && passwordInput != null)
        {
            passwordInput.Select();
            passwordInput.ActivateInputField();
        }
        else
        {
            if (usernameInput != null && usernameInput.isFocused) usernameInput.DeactivateInputField();
            if (passwordInput != null && passwordInput.isFocused) passwordInput.DeactivateInputField();
        }
    }

    void ActivateCurrent()
    {
        if (currentMode == Mode.Login)
        {
            switch (currentIndex)
            {
                case IDX_USER: Navigate(1); break;
                case IDX_PASS: OnLoginClick(); break;
                case IDX_LOGIN: OnLoginClick(); break;
                case IDX_REG: OnRegisterClick(); break;
                case IDX_BACK: OnBackClick(); break;
            }
        }
        else // Profile
        {
            switch (currentIndex)
            {
                case IDX_LOGOUT: OnLogoutClick(); break;
                case IDX_PROFILE_BACK: OnBackClick(); break;
            }
        }
    }

    // ========================================
    //          ANIMÁCIÓ
    // ========================================
    void AnimateItems()
    {
        flickerTimer += Time.deltaTime;
        float pulse = (Mathf.Sin(flickerTimer * flickerSpeed) + 1f) / 2f;

        if (currentMode == Mode.Login)
        {
            LerpBG(usernameBG, currentIndex == IDX_USER);
            LerpBG(passwordBG, currentIndex == IDX_PASS);
            LerpBG(loginBtnBG, currentIndex == IDX_LOGIN);
            LerpBG(registerBtnBG, currentIndex == IDX_REG);
            LerpBG(backBtnBG, currentIndex == IDX_BACK);

            LerpLabel(usernameLabel, currentIndex == IDX_USER, pulse);
            LerpLabel(passwordLabel, currentIndex == IDX_PASS, pulse);

            LerpButtonText(loginBtnTxt, currentIndex == IDX_LOGIN, pulse, 24f, 22f);
            LerpButtonText(registerBtnTxt, currentIndex == IDX_REG, pulse, 18f, 16f);
            LerpButtonText(backBtnTxt, currentIndex == IDX_BACK, pulse, 15f, 13f);
        }
        else
        {
            LerpBG(logoutBtnBG, currentIndex == IDX_LOGOUT);
            LerpBG(profileBackBtnBG, currentIndex == IDX_PROFILE_BACK);
            LerpButtonText(logoutBtnTxt, currentIndex == IDX_LOGOUT, pulse, 24f, 22f);
            LerpButtonText(profileBackBtnTxt, currentIndex == IDX_PROFILE_BACK, pulse, 15f, 13f);
        }

        // Panel accent-ek
        if (panelTopAccent != null)
        {
            float a = Mathf.Lerp(0.5f, 0.9f, pulse);
            var c = colPink; c.a = a;
            panelTopAccent.color = c;
        }
        if (panelBottomAccent != null)
        {
            float a = Mathf.Lerp(0.3f, 0.6f, 1f - pulse);
            var c = colPink; c.a = a;
            panelBottomAccent.color = c;
        }
    }

    void LerpBG(Image img, bool active)
    {
        if (img == null) return;
        Color target = active ? colActiveBG : colIdleBG;
        img.color = Color.Lerp(img.color, target, Time.deltaTime * 8f);
    }

    void LerpLabel(TextMeshProUGUI txt, bool active, float pulse)
    {
        if (txt == null) return;
        if (active)
            txt.color = Color.Lerp(colPink, colActiveLight, pulse);
        else
            txt.color = Color.Lerp(txt.color, new Color(1f, 1f, 1f, 0.5f), Time.deltaTime * 5f);
    }

    void LerpButtonText(TextMeshProUGUI txt, bool active, float pulse, float activeSize, float idleSize)
    {
        if (txt == null) return;
        if (active)
        {
            txt.color = Color.Lerp(colActiveLight, colPink, pulse);
            txt.fontSize = Mathf.Lerp(txt.fontSize, activeSize, Time.deltaTime * 10f);
        }
        else
        {
            txt.color = Color.Lerp(txt.color, new Color(1f, 1f, 1f, 0.75f), Time.deltaTime * 5f);
            txt.fontSize = Mathf.Lerp(txt.fontSize, idleSize, Time.deltaTime * 10f);
        }
    }

    // ========================================
    //          AKCIÓK
    // ========================================
    public void OnLoginClick()
    {
        if (isBusy) return;

        string user = usernameInput != null ? usernameInput.text.Trim() : "";
        string pass = passwordInput != null ? passwordInput.text : "";

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            ShowFeedback("ADD MEG A FELHASZNÁLÓNEVET ÉS A JELSZÓT!", colErr);
            return;
        }

        isBusy = true;
        ShowFeedback("BEJELENTKEZÉS FOLYAMATBAN...", colInfo);

        if (DatabaseManager.instance == null)
        {
            GameObject dbGO = new GameObject("DatabaseManager (Auto)");
            dbGO.AddComponent<DatabaseManager>();
        }

        if (DatabaseManager.instance == null)
        {
            ShowFeedback("HIBA: DATABASEMANAGER NEM JÖTT LÉTRE!", colErr);
            isBusy = false;
            return;
        }

        DatabaseManager.instance.Login(user, pass, (success) =>
        {
            isBusy = false;
            if (success)
            {
                ShowFeedback("SIKERES BELÉPÉS!", colOk);
                StartCoroutine(SwitchToProfileAfterDelay());
            }
            else
            {
                ShowFeedback("HIBÁS FELHASZNÁLÓNÉV VAGY JELSZÓ!", colErr);
            }
        });
    }

    IEnumerator SwitchToProfileAfterDelay()
    {
        yield return new WaitForSeconds(0.6f);
        ShowProfileView();
    }

    public void OnRegisterClick()
    {
        ShowFeedback("MEGNYITOM A REGISZTRÁCIÓS OLDALT...", colPink);
        Application.OpenURL(registerUrl);
    }

    public void OnLogoutClick()
    {
        if (isBusy) return;
        isBusy = true;

        if (DatabaseManager.instance != null)
            DatabaseManager.instance.Logout();

        ShowProfileFeedback("KIJELENTKEZVE!", colOk);
        StartCoroutine(SwitchToLoginAfterDelay());
    }

    IEnumerator SwitchToLoginAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        isBusy = false;
        ShowLoginView();
    }

    public void OnBackClick()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        StartCoroutine(FadeOutAndLoad(mainMenuSceneName));
    }

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        float t = 0f;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            if (navGroup != null) navGroup.alpha = Mathf.Lerp(1f, 0f, t / 0.4f);
            yield return null;
        }
        if (navGroup != null) navGroup.alpha = 0f;

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
        UpdateFieldFocus();
    }

    // ========================================
    //          VIEW VÁLTÁSOK
    // ========================================
    void ShowLoginView()
    {
        ClearUI();
        currentMode = Mode.Login;
        itemCount = LOGIN_COUNT;
        currentIndex = IDX_USER;
        BuildLoginUI();
        UpdateFieldFocus();
    }

    void ShowProfileView()
    {
        ClearUI();
        currentMode = Mode.Profile;
        itemCount = PROFILE_COUNT;
        currentIndex = IDX_LOGOUT;
        BuildProfileUI();
    }

    void ClearUI()
    {
        if (uiRoot != null)
        {
            Destroy(uiRoot);
            uiRoot = null;
        }
        // Clear all refs
        usernameInput = null; passwordInput = null;
        usernameBG = null; passwordBG = null;
        usernameLabel = null; passwordLabel = null;
        loginBtnBG = null; registerBtnBG = null; backBtnBG = null;
        loginBtnTxt = null; registerBtnTxt = null; backBtnTxt = null;
        feedbackText = null;
        logoutBtnBG = null; profileBackBtnBG = null;
        logoutBtnTxt = null; profileBackBtnTxt = null;
        profileFeedbackText = null;
        panelTopAccent = null; panelBottomAccent = null;
    }

    // ========================================
    //          LOGIN VIEW ÉPÍTÉS
    // ========================================
    void BuildLoginUI()
    {
        Transform canvasT = GetCanvasTransform();
        uiRoot = BuildWrapper(canvasT, "LoginUI");

        BuildText(uiRoot.transform, "Title", "LOGIN", 96f,
            Color.white, FontStyles.Bold, TextAlignmentOptions.Center,
            new Vector2(800f, 120f), new Vector2(0f, 280f));

        BuildText(uiRoot.transform, "Subtitle", "-- ENTER YOUR CREDENTIALS --", 20f,
            colPink, FontStyles.Bold, TextAlignmentOptions.Center,
            new Vector2(500f, 30f), new Vector2(0f, 210f));

        BuildFormPanel(uiRoot.transform, new Vector2(440f, 460f), new Vector2(0f, -60f));

        (usernameBG, usernameInput, usernameLabel) = BuildInputField(
            uiRoot.transform, "UsernameField", "USERNAME", false, new Vector2(0f, 90f));

        (passwordBG, passwordInput, passwordLabel) = BuildInputField(
            uiRoot.transform, "PasswordField", "PASSWORD", true, new Vector2(0f, 15f));

        (loginBtnBG, loginBtnTxt) = BuildButton(
            uiRoot.transform, "LoginButton", "LOGIN", 22f,
            new Vector2(380f, 56f), new Vector2(0f, -70f));
        var lb = loginBtnBG.gameObject.AddComponent<Button>();
        lb.onClick.AddListener(() => { currentIndex = IDX_LOGIN; UpdateFieldFocus(); OnLoginClick(); });

        feedbackText = BuildFeedbackText(uiRoot.transform, new Vector2(0f, -115f));

        BuildText(uiRoot.transform, "RegPrompt", "DON'T HAVE AN ACCOUNT?", 13f,
            new Color(1f, 1f, 1f, 0.55f), FontStyles.Bold, TextAlignmentOptions.Center,
            new Vector2(400f, 18f), new Vector2(0f, -150f));

        (registerBtnBG, registerBtnTxt) = BuildButton(
            uiRoot.transform, "RegisterButton", "REGISTER HERE", 16f,
            new Vector2(260f, 38f), new Vector2(0f, -185f));
        var rb = registerBtnBG.gameObject.AddComponent<Button>();
        rb.onClick.AddListener(() => { currentIndex = IDX_REG; UpdateFieldFocus(); OnRegisterClick(); });

        (backBtnBG, backBtnTxt) = BuildButton(
            uiRoot.transform, "BackButton", "< BACK", 13f,
            new Vector2(180f, 32f), new Vector2(0f, -240f));
        var bb = backBtnBG.gameObject.AddComponent<Button>();
        bb.onClick.AddListener(() => { currentIndex = IDX_BACK; UpdateFieldFocus(); OnBackClick(); });

        BuildHint(uiRoot.transform, "TAB / ▲▼  —  NAVIGATE          ENTER  —  SELECT          ESC  —  BACK");
    }

    // ========================================
    //          PROFILE VIEW ÉPÍTÉS
    // ========================================
    void BuildProfileUI()
    {
        Transform canvasT = GetCanvasTransform();
        uiRoot = BuildWrapper(canvasT, "ProfileUI");

        // Cím: PROFILE
        BuildText(uiRoot.transform, "Title", "PROFILE", 96f,
            Color.white, FontStyles.Bold, TextAlignmentOptions.Center,
            new Vector2(800f, 120f), new Vector2(0f, 280f));

        // Subtitle: welcome back
        string welcomeText = "-- WELCOME BACK, " + DatabaseManager.LoggedInUsername.ToUpper() + " --";
        BuildText(uiRoot.transform, "Subtitle", welcomeText, 20f,
            colPink, FontStyles.Bold, TextAlignmentOptions.Center,
            new Vector2(700f, 30f), new Vector2(0f, 210f));

        // Panel
        BuildFormPanel(uiRoot.transform, new Vector2(460f, 440f), new Vector2(0f, -60f));

        // Stats sorok (kezdetben "loading...")
        var bestRow = BuildStatRow(uiRoot.transform, "BEST SCORE", "—", new Vector2(0f, 100f));
        var racesRow = BuildStatRow(uiRoot.transform, "TOTAL RACES", "—", new Vector2(0f, 45f));
        var avgRow = BuildStatRow(uiRoot.transform, "AVERAGE SCORE", "—", new Vector2(0f, -10f));
        var rankRow = BuildStatRow(uiRoot.transform, "LEADERBOARD RANK", "—", new Vector2(0f, -65f));

        // LOGOUT gomb
        (logoutBtnBG, logoutBtnTxt) = BuildButton(
            uiRoot.transform, "LogoutButton", "LOGOUT", 22f,
            new Vector2(380f, 56f), new Vector2(0f, -160f));
        var lo = logoutBtnBG.gameObject.AddComponent<Button>();
        lo.onClick.AddListener(() => { currentIndex = IDX_LOGOUT; OnLogoutClick(); });

        profileFeedbackText = BuildFeedbackText(uiRoot.transform, new Vector2(0f, -205f));

        // BACK gomb
        (profileBackBtnBG, profileBackBtnTxt) = BuildButton(
            uiRoot.transform, "ProfileBackButton", "< BACK TO MENU", 13f,
            new Vector2(220f, 32f), new Vector2(0f, -245f));
        var pb = profileBackBtnBG.gameObject.AddComponent<Button>();
        pb.onClick.AddListener(() => { currentIndex = IDX_PROFILE_BACK; OnBackClick(); });

        BuildHint(uiRoot.transform, "▲▼  —  NAVIGATE          ENTER  —  SELECT          ESC  —  BACK");

        // Stats lekérése
        if (DatabaseManager.instance != null)
        {
            DatabaseManager.instance.GetUserStats((stats) =>
            {
                if (stats == null || bestRow == null) return;
                bestRow.text = stats.bestScore.ToString();
                racesRow.text = stats.totalRaces.ToString();
                avgRow.text = stats.averageScore.ToString();
                rankRow.text = stats.totalRaces > 0 ? ("#" + stats.rank.ToString()) : "N/A";
            });
        }
    }

    // ========================================
    //          UI HELPER-EK
    // ========================================
    Transform GetCanvasTransform()
    {
        Canvas existingCanvas = FindObjectOfType<Canvas>();
        return existingCanvas != null ? existingCanvas.transform : transform;
    }

    GameObject BuildWrapper(Transform parent, string name)
    {
        var wrapper = new GameObject(name);
        wrapper.transform.SetParent(parent, false);
        var wrapperRect = wrapper.AddComponent<RectTransform>();
        wrapperRect.anchorMin = Vector2.zero;
        wrapperRect.anchorMax = Vector2.one;
        wrapperRect.offsetMin = Vector2.zero;
        wrapperRect.offsetMax = Vector2.zero;
        navGroup = wrapper.AddComponent<CanvasGroup>();
        return wrapper;
    }

    void BuildFormPanel(Transform parent, Vector2 size, Vector2 pos)
    {
        var panel = new GameObject("FormPanel");
        panel.transform.SetParent(parent, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = size;
        panelRect.anchoredPosition = pos;
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.04f, 0.02f, 0.12f, 0.5f);

        float halfH = size.y / 2f;
        panelTopAccent = BuildAccentLine(panel.transform, new Vector2(0f, halfH), size.x, 2f);
        panelBottomAccent = BuildAccentLine(panel.transform, new Vector2(0f, -halfH), size.x, 2f);
    }

    TextMeshProUGUI BuildStatRow(Transform parent, string label, string value, Vector2 pos)
    {
        // Row container
        var row = new GameObject("StatRow_" + label);
        row.transform.SetParent(parent, false);
        var rowRect = row.AddComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.5f, 0.5f);
        rowRect.anchorMax = new Vector2(0.5f, 0.5f);
        rowRect.pivot = new Vector2(0.5f, 0.5f);
        rowRect.sizeDelta = new Vector2(400f, 45f);
        rowRect.anchoredPosition = pos;

        // Sötét háttér a sorhoz (áttetsző)
        var bg = row.AddComponent<Image>();
        bg.color = colIdleBG;

        // Label (bal oldalt)
        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(row.transform, false);
        var lr = lblGO.AddComponent<RectTransform>();
        lr.anchorMin = new Vector2(0f, 0f);
        lr.anchorMax = new Vector2(0.6f, 1f);
        lr.offsetMin = new Vector2(16f, 0f);
        lr.offsetMax = Vector2.zero;
        var lblTmp = lblGO.AddComponent<TextMeshProUGUI>();
        lblTmp.text = label;
        lblTmp.fontSize = 13f;
        lblTmp.color = new Color(1f, 1f, 1f, 0.55f);
        lblTmp.fontStyle = FontStyles.Bold;
        lblTmp.alignment = TextAlignmentOptions.Left;
        lblTmp.enableWordWrapping = false;

        // Value (jobb oldalt)
        var valGO = new GameObject("Value");
        valGO.transform.SetParent(row.transform, false);
        var vr = valGO.AddComponent<RectTransform>();
        vr.anchorMin = new Vector2(0.5f, 0f);
        vr.anchorMax = new Vector2(1f, 1f);
        vr.offsetMin = Vector2.zero;
        vr.offsetMax = new Vector2(-16f, 0f);
        var valTmp = valGO.AddComponent<TextMeshProUGUI>();
        valTmp.text = value;
        valTmp.fontSize = 22f;
        valTmp.color = colPink;
        valTmp.fontStyle = FontStyles.Bold;
        valTmp.alignment = TextAlignmentOptions.Right;
        valTmp.enableWordWrapping = false;

        return valTmp;
    }

    TextMeshProUGUI BuildText(Transform parent, string name, string text, float size,
        Color color, FontStyles style, TextAlignmentOptions align,
        Vector2 sizeDelta, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = sizeDelta;
        rect.anchoredPosition = pos;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.fontStyle = style;
        tmp.alignment = align;
        tmp.enableWordWrapping = false;
        return tmp;
    }

    TextMeshProUGUI BuildFeedbackText(Transform parent, Vector2 pos)
    {
        var fb = new GameObject("Feedback");
        fb.transform.SetParent(parent, false);
        var fbRect = fb.AddComponent<RectTransform>();
        fbRect.anchorMin = new Vector2(0.5f, 0.5f);
        fbRect.anchorMax = new Vector2(0.5f, 0.5f);
        fbRect.pivot = new Vector2(0.5f, 0.5f);
        fbRect.sizeDelta = new Vector2(500f, 22f);
        fbRect.anchoredPosition = pos;
        var tmp = fb.AddComponent<TextMeshProUGUI>();
        tmp.text = "";
        tmp.fontSize = 13f;
        tmp.color = colErr;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableWordWrapping = false;
        return tmp;
    }

    void BuildHint(Transform parent, string hintText)
    {
        var hint = new GameObject("Hint");
        hint.transform.SetParent(parent, false);
        var hr = hint.AddComponent<RectTransform>();
        hr.anchorMin = new Vector2(0f, 0f);
        hr.anchorMax = new Vector2(1f, 0f);
        hr.pivot = new Vector2(0.5f, 0f);
        hr.sizeDelta = new Vector2(0f, 30f);
        hr.anchoredPosition = new Vector2(0f, 20f);
        var ht = hint.AddComponent<TextMeshProUGUI>();
        ht.text = hintText;
        ht.fontSize = 14f;
        ht.color = new Color(0.75f, 0.75f, 0.85f, 0.85f);
        ht.alignment = TextAlignmentOptions.Center;
        ht.enableWordWrapping = false;
    }

    Image BuildAccentLine(Transform parent, Vector2 pos, float width, float height)
    {
        var go = new GameObject("Accent");
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = pos;
        var img = go.AddComponent<Image>();
        img.color = colPink;
        return img;
    }

    (Image bg, TMP_InputField inp, TextMeshProUGUI label) BuildInputField(
        Transform parent, string name, string labelText, bool isPassword, Vector2 pos)
    {
        var fieldGO = new GameObject(name);
        fieldGO.transform.SetParent(parent, false);
        var fieldRect = fieldGO.AddComponent<RectTransform>();
        fieldRect.anchorMin = new Vector2(0.5f, 0.5f);
        fieldRect.anchorMax = new Vector2(0.5f, 0.5f);
        fieldRect.pivot = new Vector2(0.5f, 0.5f);
        fieldRect.sizeDelta = new Vector2(380f, 60f);
        fieldRect.anchoredPosition = pos;
        var bg = fieldGO.AddComponent<Image>();
        bg.color = colIdleBG;

        var label = BuildText(fieldGO.transform, "Label", labelText, 11f,
            new Color(1f, 1f, 1f, 0.5f), FontStyles.Bold, TextAlignmentOptions.Left,
            new Vector2(0f, 14f), new Vector2(0f, 18f));
        var lblR = label.GetComponent<RectTransform>();
        lblR.anchorMin = new Vector2(0f, 1f);
        lblR.anchorMax = new Vector2(1f, 1f);
        lblR.pivot = new Vector2(0.5f, 1f);
        lblR.sizeDelta = new Vector2(-24f, 14f);
        lblR.anchoredPosition = new Vector2(0f, -6f);

        var taGO = new GameObject("TextArea");
        taGO.transform.SetParent(fieldGO.transform, false);
        var taRect = taGO.AddComponent<RectTransform>();
        taRect.anchorMin = new Vector2(0f, 0f);
        taRect.anchorMax = new Vector2(1f, 1f);
        taRect.offsetMin = new Vector2(14f, 6f);
        taRect.offsetMax = new Vector2(-14f, -22f);
        taGO.AddComponent<RectMask2D>();

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(taGO.transform, false);
        var txtRect = txtGO.AddComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;
        var txtTMP = txtGO.AddComponent<TextMeshProUGUI>();
        txtTMP.fontSize = 18f;
        txtTMP.color = Color.white;
        txtTMP.alignment = TextAlignmentOptions.Left;
        txtTMP.enableWordWrapping = false;
        txtTMP.text = "";

        var inp = fieldGO.AddComponent<TMP_InputField>();
        inp.textViewport = taRect;
        inp.textComponent = txtTMP;
        inp.contentType = isPassword
            ? TMP_InputField.ContentType.Password
            : TMP_InputField.ContentType.Standard;
        inp.caretColor = colPink;
        inp.customCaretColor = true;
        inp.selectionColor = new Color(0.55f, 0.1f, 1f, 0.4f);
        inp.text = "";

        inp.onSubmit.AddListener((_) => { ActivateCurrent(); });

        inp.onSelect.AddListener((_) =>
        {
            if (labelText == "USERNAME") currentIndex = IDX_USER;
            else if (labelText == "PASSWORD") currentIndex = IDX_PASS;
        });

        return (bg, inp, label);
    }

    (Image bg, TextMeshProUGUI txt) BuildButton(
        Transform parent, string name, string labelText, float fontSize,
        Vector2 size, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = pos;
        var img = go.AddComponent<Image>();
        img.color = colIdleBG;

        var txtGO = new GameObject("Label");
        txtGO.transform.SetParent(go.transform, false);
        var tr = txtGO.AddComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = labelText;
        tmp.fontSize = fontSize;
        tmp.color = new Color(1f, 1f, 1f, 0.75f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableWordWrapping = false;

        return (img, tmp);
    }

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackText == null) return;
        feedbackText.text = message;
        feedbackText.color = color;
    }

    private void ShowProfileFeedback(string message, Color color)
    {
        if (profileFeedbackText == null) return;
        profileFeedbackText.text = message;
        profileFeedbackText.color = color;
    }
}
