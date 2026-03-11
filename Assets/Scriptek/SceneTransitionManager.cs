using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// SCENE TRANSITION MANAGER
/// Singleton - egyszer kell létrehozni, minden scene-ben elérhető.
///
/// BEÁLLÍTÁS:
/// 1. Hozz létre egy üres GameObject-et "SceneTransitionManager" névvel
/// 2. Csatold ezt a scriptet
/// 3. Mentsd el Prefab-ként (húzd a Project ablakba)
/// 4. Helyezd el az ELSŐ scene-ben (pl. IntroScene vagy MainMenu)
/// 5. A script DontDestroyOnLoad-dal gondoskodik arról, hogy minden scene-ben meglegyen
///
/// HASZNÁLAT bárhonnan:
///   SceneTransitionManager.Instance.LoadScene("GameScene");
///   SceneTransitionManager.Instance.LoadScene("GameScene", TransitionType.Fade);
///   SceneTransitionManager.Instance.LoadScene("GameScene", TransitionType.LoadingScreen);
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    public enum TransitionType { Fade, LoadingScreen }

    [Header("Fade Beállítások")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private Color fadeColor = Color.black;

    [Header("Loading Screen Beállítások")]
    [SerializeField] private Sprite loadingBackgroundSprite; // Opcionális háttérkép
    [SerializeField] private string[] loadingTips;           // Opcionális tippek (rotálva jelennek meg)
    [SerializeField] private float minimumLoadTime = 1.0f;   // Minimum megjelenési idő

    // Runtime UI referenciák
    private Canvas overlayCanvas;
    private Image fadePanel;
    private GameObject loadingScreenRoot;
    private Slider progressBar;
    private Text progressText;
    private Text tipText;
    private Image loadingBgImg;

    private bool isTransitioning = false;

    // -------------------------------------------------------------------------
    // Singleton init
    // -------------------------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildOverlayUI();
    }

    // -------------------------------------------------------------------------
    // Publikus API
    // -------------------------------------------------------------------------

    /// <summary>Scene betöltése fade átmenettel (alapértelmezett)</summary>
    public void LoadScene(string sceneName)
    {
        LoadScene(sceneName, TransitionType.Fade);
    }

    /// <summary>Scene betöltése megadott átmenet típussal</summary>
    public void LoadScene(string sceneName, TransitionType type)
    {
        if (isTransitioning) return;
        isTransitioning = true;

        if (type == TransitionType.LoadingScreen)
            StartCoroutine(LoadWithLoadingScreen(sceneName));
        else
            StartCoroutine(LoadWithFade(sceneName));
    }

    // -------------------------------------------------------------------------
    // Fade átmenet
    // -------------------------------------------------------------------------
    private IEnumerator LoadWithFade(string sceneName)
    {
        // Fade IN (elsötétül)
        yield return StartCoroutine(FadeTo(1f));

        // Async betöltés indítása
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        // Várakozás a betöltés végéig
        while (op.progress < 0.9f)
            yield return null;

        // Scene aktiválása
        op.allowSceneActivation = true;
        yield return null; // egy frame várakozás

        // Fade OUT (kivilágosodik)
        yield return StartCoroutine(FadeTo(0f));

        isTransitioning = false;
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        fadePanel.gameObject.SetActive(true);
        float startAlpha = fadePanel.color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / fadeDuration);
            SetFadeAlpha(Mathf.Lerp(startAlpha, targetAlpha, t));
            yield return null;
        }

        SetFadeAlpha(targetAlpha);

        if (targetAlpha <= 0f)
            fadePanel.gameObject.SetActive(false);
    }

    private void SetFadeAlpha(float alpha)
    {
        Color c = fadePanel.color;
        c.a = alpha;
        fadePanel.color = c;
    }

    // -------------------------------------------------------------------------
    // Loading Screen átmenet
    // -------------------------------------------------------------------------
    private IEnumerator LoadWithLoadingScreen(string sceneName)
    {
        // 1. Fade el a loading screen-re
        yield return StartCoroutine(FadeTo(1f));

        // 2. Loading screen megjelenítése
        ShowLoadingScreen(true);
        UpdateProgress(0f);
        ShowRandomTip();

        // 3. Fade vissza (loading screen látható lesz)
        yield return StartCoroutine(FadeTo(0f));

        // 4. Async betöltés
        float startTime = Time.time;
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            // Progress 0-90% → vizuálisan 0-100%-ra skálázva
            float displayProgress = op.progress / 0.9f;
            UpdateProgress(displayProgress);
            yield return null;
        }

        // 5. Minimum megjelenési idő betartása
        float elapsed = Time.time - startTime;
        if (elapsed < minimumLoadTime)
            yield return new WaitForSeconds(minimumLoadTime - elapsed);

        UpdateProgress(1f);
        yield return new WaitForSeconds(0.3f); // Rövid szünet 100%-nál

        // 6. Fade el, scene aktiválás
        yield return StartCoroutine(FadeTo(1f));
        op.allowSceneActivation = true;
        yield return null;

        // 7. Loading screen elrejtése, fade vissza
        ShowLoadingScreen(false);
        yield return StartCoroutine(FadeTo(0f));

        isTransitioning = false;
    }

    private void UpdateProgress(float progress)
    {
        if (progressBar != null) progressBar.value = progress;
        if (progressText != null) progressText.text = Mathf.RoundToInt(progress * 100f) + "%";
    }

    private void ShowRandomTip()
    {
        if (tipText == null || loadingTips == null || loadingTips.Length == 0) return;
        tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
    }

    private void ShowLoadingScreen(bool show)
    {
        if (loadingScreenRoot != null)
        {
            // Sprite alkalmazása itt - ekkor már biztosan be van töltve az Inspector értéke
            if (show && loadingBgImg != null)
            {
                if (loadingBackgroundSprite != null)
                {
                    loadingBgImg.sprite = loadingBackgroundSprite;
                    loadingBgImg.color = Color.white;
                }
                else
                {
                    loadingBgImg.sprite = null;
                    loadingBgImg.color = Color.black;
                }
            }
            loadingScreenRoot.SetActive(show);
        }
    }

    // -------------------------------------------------------------------------
    // UI felépítése futásidőben
    // -------------------------------------------------------------------------
    private void BuildOverlayUI()
    {
        // Fő Canvas
        GameObject canvasGO = new GameObject("TransitionCanvas");
        canvasGO.transform.SetParent(transform);
        overlayCanvas = canvasGO.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 9999;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // --- FADE PANEL ---
        GameObject fadePanelGO = new GameObject("FadePanel");
        fadePanelGO.transform.SetParent(canvasGO.transform, false);
        fadePanel = fadePanelGO.AddComponent<Image>();
        fadePanel.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        StretchToFill(fadePanelGO.GetComponent<RectTransform>());
        fadePanelGO.SetActive(false);

        // --- LOADING SCREEN ---
        loadingScreenRoot = new GameObject("LoadingScreen");
        loadingScreenRoot.transform.SetParent(canvasGO.transform, false);
        loadingScreenRoot.SetActive(false);

        // Háttér
        loadingBgImg = loadingScreenRoot.AddComponent<Image>();
        loadingBgImg.color = Color.white; // white hogy a sprite eredeti színei látszanak
        loadingBgImg.preserveAspect = false;
        StretchToFill(loadingScreenRoot.GetComponent<RectTransform>());

        // "LOADING..." felirat
        CreateText(loadingScreenRoot.transform, "LoadingLabel", "LOADING...",
            new Vector2(0f, 80f), new Vector2(400f, 60f), 36, FontStyle.Bold, Color.white);

        // Progress bar háttér
        GameObject barBg = CreatePanel(loadingScreenRoot.transform, "BarBG",
            new Vector2(0f, 0f), new Vector2(800f, 24f), new Color(0.15f, 0.15f, 0.15f));

        // Progress bar kitöltés
        GameObject barFill = CreatePanel(barBg.transform, "BarFill",
            Vector2.zero, Vector2.zero, new Color(0.2f, 0.6f, 1f));
        RectTransform fillRect = barFill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.sizeDelta = new Vector2(0f, 0f);
        fillRect.anchoredPosition = Vector2.zero;

        // Slider a progress bar vezérlésére
        progressBar = barBg.AddComponent<Slider>();
        progressBar.fillRect = fillRect;
        progressBar.direction = Slider.Direction.LeftToRight;
        progressBar.minValue = 0f;
        progressBar.maxValue = 1f;
        progressBar.value = 0f;
        progressBar.interactable = false;

        // Progress % szöveg
        progressText = CreateText(loadingScreenRoot.transform, "ProgressText", "0%",
            new Vector2(0f, -40f), new Vector2(200f, 40f), 22, FontStyle.Normal,
            new Color(0.7f, 0.7f, 0.7f));

        // Tipp szöveg
        tipText = CreateText(loadingScreenRoot.transform, "TipText", "",
            new Vector2(0f, -120f), new Vector2(900f, 60f), 18, FontStyle.Italic,
            new Color(0.6f, 0.6f, 0.6f));
    }

    // -------------------------------------------------------------------------
    // UI segédmetódusok
    // -------------------------------------------------------------------------
    private void StretchToFill(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }

    private GameObject CreatePanel(Transform parent, string name, Vector2 pos,
        Vector2 size, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        return go;
    }

    private Text CreateText(Transform parent, string name, string content,
        Vector2 pos, Vector2 size, int fontSize, FontStyle style, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Text txt = go.AddComponent<Text>();
        txt.text = content;
        txt.fontSize = fontSize;
        txt.fontStyle = style;
        txt.color = color;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        return txt;
    }
}