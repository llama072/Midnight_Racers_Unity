using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GarageNavigationManager : MonoBehaviour
{
    [Header("Scene Nevek")]
    [SerializeField] private string playSceneName = "DriveSelect";
    [SerializeField] private string carSelectSceneName = "Garage";
    [SerializeField] private string settingsSceneName = "Settings";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Beállítások")]
    [SerializeField] private float itemSpacing = 70f;
    [SerializeField] private float rotateSpeed = 7f;
    [SerializeField] private float flickerSpeed = 3.5f;

    private readonly Color colActiveDark = new Color(0.55f, 0.1f, 1f, 1f);   // sötét lila
    private readonly Color colActiveLight = new Color(1f, 1f, 1f, 1f);         // fehér

    private readonly string[] menuLabels = { "PLAY", "CAR SELECT", "MAIN MENU", "SETTINGS" };
    private int itemCount;
    private int currentIndex = 0;
    private float currentOffset = 0f;
    private float targetOffset = 0f;

    private RectTransform[] itemRects;
    private TextMeshProUGUI[] menuTexts;
    private Image[] itemBGs;
    private CanvasGroup navGroup;
    private bool isTransitioning = false;
    private float flickerTimer = 0f;

    void Start() { itemCount = menuLabels.Length; BuildUI(); StartCoroutine(FadeIn()); }

    void Update()
    {
        if (isTransitioning) return;
        HandleInput();
        AnimateScroll();
        AnimateItems();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) Navigate(-1);
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) Navigate(1);
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) SelectCurrent();
    }

    void Navigate(int dir)
    {
        currentIndex = Mathf.Clamp(currentIndex + dir, 0, itemCount - 1);
        targetOffset = currentIndex * itemSpacing;
    }

    void SelectCurrent() { isTransitioning = true; StartCoroutine(SelectFlash(currentIndex)); }

    void AnimateScroll() { } // scroll nincs, pozíciók fixek

    void AnimateItems()
    {
        if (menuTexts == null) return;
        flickerTimer += Time.deltaTime;

        for (int i = 0; i < itemCount; i++)
        {
            float dist = Mathf.Abs(i - currentIndex);
            bool active = (i == currentIndex);

            // Pozíció FIX — nem scrolloz
            if (itemRects[i] != null)
                itemRects[i].anchoredPosition = new Vector2(0f, -(i * itemSpacing));

            if (active)
            {
                // Lila ↔ fehér pulzálás
                float pulse = (Mathf.Sin(flickerTimer * flickerSpeed) + 1f) / 2f;
                menuTexts[i].color = Color.Lerp(colActiveDark, colActiveLight, pulse);
                menuTexts[i].fontSize = Mathf.Lerp(menuTexts[i].fontSize, 32f, Time.deltaTime * 10f);
                // Aktív gomb háttér: sötét lila
                if (itemBGs[i] != null)
                    itemBGs[i].color = Color.Lerp(itemBGs[i].color,
                        new Color(0.15f, 0.05f, 0.3f, 0.92f), Time.deltaTime * 8f);
            }
            else
            {
                float fade = Mathf.Lerp(0.2f, 0.7f, 1f - Mathf.Clamp01(dist / 2f));
                menuTexts[i].color = new Color(1f, 1f, 1f, fade);
                menuTexts[i].fontSize = Mathf.Lerp(menuTexts[i].fontSize,
                    Mathf.Lerp(18f, 24f, 1f - Mathf.Clamp01(dist)), Time.deltaTime * 10f);
                // Inaktív gomb háttér: sötétebb, kevésbé látható
                if (itemBGs[i] != null)
                    itemBGs[i].color = Color.Lerp(itemBGs[i].color,
                        new Color(0f, 0f, 0f, Mathf.Lerp(0.15f, 0.55f, 1f - Mathf.Clamp01(dist))),
                        Time.deltaTime * 8f);
            }
        }
    }

    IEnumerator SelectFlash(int index)
    {
        for (int i = 0; i < 4; i++)
        {
            menuTexts[index].color = Color.white;
            yield return new WaitForSeconds(0.06f);
            menuTexts[index].color = colActiveDark;
            yield return new WaitForSeconds(0.06f);
        }
        string scene = GetSceneName(index);
        if (!string.IsNullOrEmpty(scene) && SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(scene);
        isTransitioning = false;
    }

    IEnumerator FadeIn()
    {
        if (navGroup == null) yield break;
        navGroup.alpha = 0f;
        float e = 0f;
        while (e < 0.6f) { e += Time.deltaTime; navGroup.alpha = e / 0.6f; yield return null; }
        navGroup.alpha = 1f;
    }

    string GetSceneName(int i)
    {
        switch (i)
        {
            case 0: return playSceneName;
            case 1: return carSelectSceneName;
            case 2: return mainMenuSceneName;
            case 3: return settingsSceneName;
            default: return "";
        }
    }

    void BuildUI()
    {
        // Canvas
        var cgo = new GameObject("GarageNavCanvas");
        cgo.transform.SetParent(transform, false);
        var cv = cgo.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 200;
        var sc = cgo.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        cgo.AddComponent<GraphicRaycaster>();
        navGroup = cgo.AddComponent<CanvasGroup>();

        // Jobb oldali container — középmagasságban horgonyozva
        var container = new GameObject("MenuContainer");
        container.transform.SetParent(cgo.transform, false);
        var contRect = container.AddComponent<RectTransform>();
        contRect.anchorMin = new Vector2(1f, 0.5f);
        contRect.anchorMax = new Vector2(1f, 0.5f);
        contRect.pivot = new Vector2(1f, 0.5f);
        contRect.sizeDelta = new Vector2(320f, itemCount * itemSpacing);
        contRect.anchoredPosition = new Vector2(-24f, 0f);

        itemRects = new RectTransform[itemCount];
        menuTexts = new TextMeshProUGUI[itemCount];
        itemBGs = new Image[itemCount];

        for (int i = 0; i < itemCount; i++)
        {
            // Item GO
            var itemGO = new GameObject(menuLabels[i]);
            itemGO.transform.SetParent(container.transform, false);
            var itemRect = itemGO.AddComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0f, 1f);
            itemRect.anchorMax = new Vector2(1f, 1f);
            itemRect.pivot = new Vector2(0.5f, 0.5f);
            itemRect.sizeDelta = new Vector2(0f, itemSpacing - 8f);
            itemRect.anchoredPosition = new Vector2(0f, -(i * itemSpacing));
            itemRects[i] = itemRect;

            // Háttér panel
            var bg = itemGO.AddComponent<Image>();
            bg.color = i == 0
                ? new Color(0.15f, 0.05f, 0.3f, 0.92f)   // aktív: lila
                : new Color(0f, 0f, 0f, 0.55f);            // inaktív: sötét
            // Bal oldali lila csík
            var accent = new GameObject("Accent");
            accent.transform.SetParent(itemGO.transform, false);
            var accentImg = accent.AddComponent<Image>();
            accentImg.color = new Color(0.6f, 0.2f, 1f, i == 0 ? 1f : 0.3f);
            var accentRect = accent.GetComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0f, 0f); accentRect.anchorMax = new Vector2(0f, 1f);
            accentRect.pivot = new Vector2(0f, 0.5f);
            accentRect.sizeDelta = new Vector2(4f, 0f);
            accentRect.anchoredPosition = Vector2.zero;

            itemBGs[i] = bg;

            // Szöveg
            var txtGO = new GameObject("Label");
            txtGO.transform.SetParent(itemGO.transform, false);
            var tmp = txtGO.AddComponent<TextMeshProUGUI>();
            tmp.text = menuLabels[i];
            tmp.fontSize = i == 0 ? 32f : 22f;
            tmp.color = i == 0 ? colActiveDark : new Color(1f, 1f, 1f, 0.7f);
            tmp.alignment = TextAlignmentOptions.Right;
            tmp.fontStyle = FontStyles.Bold;
            tmp.enableWordWrapping = false;
            var txtRect = txtGO.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = new Vector2(-16f, 0f);
            txtRect.anchoredPosition = new Vector2(-8f, 0f);
            menuTexts[i] = tmp;

            // Gomb
            var btn = itemGO.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.1f);
            colors.pressedColor = new Color(0.6f, 0.2f, 1f, 0.3f);
            btn.colors = colors;
            int cap = i;
            btn.onClick.AddListener(() => {
                currentIndex = cap;
                targetOffset = currentIndex * itemSpacing;
            });
        }

        // Hint
        var hint = new GameObject("Hint");
        hint.transform.SetParent(cgo.transform, false);
        var ht = hint.AddComponent<TextMeshProUGUI>();
        ht.text = "W ▲  /  S ▼  —  NAVIGATE          ENTER  —  SELECT";
        ht.fontSize = 16f;
        ht.color = new Color(0.75f, 0.75f, 0.85f, 0.85f);
        ht.alignment = TextAlignmentOptions.Center;
        ht.enableWordWrapping = false;
        var hr = hint.GetComponent<RectTransform>();
        hr.anchorMin = new Vector2(0.5f, 0f); hr.anchorMax = new Vector2(0.5f, 0f);
        hr.pivot = new Vector2(0.5f, 0f);
        hr.sizeDelta = new Vector2(800f, 40f);
        hr.anchoredPosition = new Vector2(0f, 24f);
    }
}