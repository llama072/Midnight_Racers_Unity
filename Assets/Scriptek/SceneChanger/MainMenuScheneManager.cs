using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuSceneManager : MonoBehaviour
{
    [Header("Scene Nevek")]
    [SerializeField] private string playSceneName = "Garage";
    [SerializeField] private string profileSceneName = "Profile";
    [SerializeField] private string settingsSceneName = "Settings";

    [Header("Beállítások")]
    [SerializeField] private float itemSpacing = 90f;
    [SerializeField] private float flickerSpeed = 3.5f;

    private readonly Color colActiveDark = new Color(0.55f, 0.1f, 1f, 1f);
    private readonly Color colActiveLight = new Color(1f, 1f, 1f, 1f);

    private readonly string[] menuLabels = { "PLAY", "PROFILE", "SETTINGS", "EXIT" };
    private int itemCount;
    private int currentIndex = 0;

    private RectTransform[] itemRects;
    private TextMeshProUGUI[] menuTexts;
    private Image[] itemBGs;
    private Image[] accentLines;
    private CanvasGroup navGroup;
    private bool isTransitioning = false;
    private float flickerTimer = 0f;

    void Start()
    {
        itemCount = menuLabels.Length;
        BuildUI();
        StartCoroutine(FadeIn());
    }

    void Update()
    {
        if (isTransitioning) return;
        HandleInput();
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
    }

    void SelectCurrent()
    {
        isTransitioning = true;
        StartCoroutine(SelectFlash(currentIndex));
    }

    void AnimateItems()
    {
        if (menuTexts == null) return;
        flickerTimer += Time.deltaTime;

        for (int i = 0; i < itemCount; i++)
        {
            float dist = Mathf.Abs(i - currentIndex);
            bool active = (i == currentIndex);

            if (active)
            {
                float pulse = (Mathf.Sin(flickerTimer * flickerSpeed) + 1f) / 2f;
                menuTexts[i].color = Color.Lerp(colActiveDark, colActiveLight, pulse);
                menuTexts[i].fontSize = Mathf.Lerp(menuTexts[i].fontSize, 32f, Time.deltaTime * 10f);
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
                if (itemBGs[i] != null)
                    itemBGs[i].color = Color.Lerp(itemBGs[i].color,
                        new Color(0f, 0f, 0f, Mathf.Lerp(0.15f, 0.4f, 1f - Mathf.Clamp01(dist))),
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

        // Fade out mielőtt scene vált
        float t = 0f;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            navGroup.alpha = Mathf.Lerp(1f, 0f, t / 0.4f);
            yield return null;
        }
        navGroup.alpha = 0f;

        switch (index)
        {
            case 0:
                SceneTransitionManager.Instance.LoadScene(playSceneName,
                    SceneTransitionManager.TransitionType.LoadingScreen);
                break;
            case 1:
                SceneTransitionManager.Instance.LoadScene(profileSceneName);
                break;
            case 2:
                SceneTransitionManager.Instance.LoadScene(settingsSceneName);
                break;
            case 3:
                Application.Quit();
                break;
        }

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

    void BuildUI()
    {
        Canvas existingCanvas = FindObjectOfType<Canvas>();
        Transform canvasTransform = existingCanvas != null ? existingCanvas.transform : transform;

        var wrapper = new GameObject("MainMenuNav");
        wrapper.transform.SetParent(canvasTransform, false);
        var wrapperRect = wrapper.AddComponent<RectTransform>();
        wrapperRect.anchorMin = Vector2.zero;
        wrapperRect.anchorMax = Vector2.one;
        wrapperRect.offsetMin = Vector2.zero;
        wrapperRect.offsetMax = Vector2.zero;
        navGroup = wrapper.AddComponent<CanvasGroup>();

        // Sötétített panel a gombok mögé
        var panel = new GameObject("BackdropPanel");
        panel.transform.SetParent(wrapper.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(380f, itemCount * itemSpacing + 60f);
        panelRect.anchoredPosition = new Vector2(0f, -50f);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.02f, 0.15f, 0.75f);

        // Container a gombok számára
        var container = new GameObject("MenuContainer");
        container.transform.SetParent(wrapper.transform, false);
        var contRect = container.AddComponent<RectTransform>();
        contRect.anchorMin = new Vector2(0.5f, 0.5f);
        contRect.anchorMax = new Vector2(0.5f, 0.5f);
        contRect.pivot = new Vector2(0.5f, 0.5f);
        contRect.sizeDelta = new Vector2(350f, itemCount * itemSpacing);
        contRect.anchoredPosition = new Vector2(0f, -50f);

        itemRects = new RectTransform[itemCount];
        menuTexts = new TextMeshProUGUI[itemCount];
        itemBGs = new Image[itemCount];
        accentLines = new Image[itemCount];

        for (int i = 0; i < itemCount; i++)
        {
            var itemGO = new GameObject(menuLabels[i]);
            itemGO.transform.SetParent(container.transform, false);
            var itemRect = itemGO.AddComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0f, 1f);
            itemRect.anchorMax = new Vector2(1f, 1f);
            itemRect.pivot = new Vector2(0.5f, 1f);
            itemRect.sizeDelta = new Vector2(0f, itemSpacing - 8f);
            itemRect.anchoredPosition = new Vector2(0f, -(i * itemSpacing));
            itemRects[i] = itemRect;

            var bg = itemGO.AddComponent<Image>();
            bg.color = i == 0
                ? new Color(0.15f, 0.05f, 0.3f, 0.92f)
                : new Color(0f, 0f, 0f, 0.3f);
            itemBGs[i] = bg;

            var txtGO = new GameObject("Label");
            txtGO.transform.SetParent(itemGO.transform, false);
            var tmp = txtGO.AddComponent<TextMeshProUGUI>();
            tmp.text = menuLabels[i];
            tmp.fontSize = i == 0 ? 32f : 22f;
            tmp.color = i == 0 ? colActiveDark : new Color(1f, 1f, 1f, 0.7f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.enableWordWrapping = false;
            var txtRect = txtGO.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = Vector2.zero;
            txtRect.anchoredPosition = Vector2.zero;
            menuTexts[i] = tmp;

            // accentLines tömb kitöltése null-lal (nincs csík)
            accentLines[i] = null;

            var btn = itemGO.AddComponent<Button>();
            int cap = i;
            btn.onClick.AddListener(() => { currentIndex = cap; });
        }

        // Hint alul
        var hint = new GameObject("Hint");
        hint.transform.SetParent(wrapper.transform, false);
        var ht = hint.AddComponent<TextMeshProUGUI>();
        ht.text = "W ▲  /  S ▼  —  NAVIGATE          ENTER  —  SELECT";
        ht.fontSize = 16f;
        ht.color = new Color(0.75f, 0.75f, 0.85f, 0.85f);
        ht.alignment = TextAlignmentOptions.Center;
        ht.enableWordWrapping = false;
        var hr = hint.GetComponent<RectTransform>();
        hr.anchorMin = new Vector2(0f, 0f);
        hr.anchorMax = new Vector2(1f, 0f);
        hr.pivot = new Vector2(0.5f, 0f);
        hr.sizeDelta = new Vector2(0f, 40f);
        hr.anchoredPosition = new Vector2(0f, 20f);
    }
}