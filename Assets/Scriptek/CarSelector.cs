using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CarSelector : MonoBehaviour
{
    [Header("UI References")]
    public Image carDisplayImage;
    public TextMeshProUGUI nameText;

    [Header("Car List")]
    public CarData[] carList;
    private int currentIndex = 0;

    [Header("Animation Beállítások")]
    [SerializeField] private float slideDistance = 700f;
    [SerializeField] private float slideDuration = 0.32f;
    [SerializeField] private float fadeLevel = 0.2f;

    [Header("Navigáció")]
    [SerializeField] private string backSceneName = "Garage";

    [Header("Alsó gombok")]
    [SerializeField] private float flickerSpeed = 3.5f;
    [SerializeField] private float selectButtonBottomOffset = 60f;
    [SerializeField] private float returnButtonBottomOffset = 20f;
    [SerializeField] private Vector2 selectSize = new Vector2(240f, 44f);
    [SerializeField] private Vector2 returnSize = new Vector2(180f, 32f);

    [Header("Oldalsó nyíl gombok")]
    [SerializeField] private Vector2 arrowSize = new Vector2(70f, 90f);
    [SerializeField] private float arrowSideOffset = 60f;

    private readonly Color colActiveDark = new Color(0.55f, 0.1f, 1f, 1f);
    private readonly Color colActiveLight = new Color(1f, 1f, 1f, 1f);
    private readonly Color colActiveBG = new Color(0.15f, 0.05f, 0.3f, 0.92f);
    private readonly Color colIdleBG = new Color(0f, 0f, 0f, 0.55f);
    private readonly Color colIdleTxt = new Color(1f, 1f, 1f, 0.55f);

    private Vector2 carHomePosition;
    private bool isAnimating = false;
    private RectTransform carRect;
    private CanvasGroup carGroup;

    // Dinamikus elemek
    private TextMeshProUGUI selectLabel;
    private Image selectAccent;
    private Image selectBgImage;
    private TextMeshProUGUI returnLabel;
    private Image returnAccent;
    private Image returnBgImage;
    private Image leftArrowBg, rightArrowBg;
    private TextMeshProUGUI leftArrowChar, rightArrowChar;
    private float flickerTimer = 0f;

    // 0 = SELECT, 1 = RETURN
    private int focusedButton = 0;

    void Start()
    {
        if (carDisplayImage != null)
        {
            carRect = carDisplayImage.rectTransform;
            carHomePosition = carRect.anchoredPosition;
            carGroup = carDisplayImage.GetComponent<CanvasGroup>();
            if (carGroup == null)
                carGroup = carDisplayImage.gameObject.AddComponent<CanvasGroup>();
        }

        BuildBottomButtons();
        BuildSideArrows();
        UpdateUI();
    }

    void Update()
    {
        AnimateBottomButtons();

        if (isAnimating) return;

        // Bal / jobb → autóváltás
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            PreviousCar();
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            NextCar();

        // Le / fel → fókusz váltása SELECT és RETURN között
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            SetFocus(1);
        else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            SetFocus(0);

        // ENTER / SPACE → aktiválja a fókuszált gombot
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (focusedButton == 0) SelectCar();
            else GoBack();
        }

        // ESC → mindig vissza
        else if (Input.GetKeyDown(KeyCode.Escape))
            GoBack();
    }

    void SetFocus(int index)
    {
        focusedButton = Mathf.Clamp(index, 0, 1);
    }

    // ---- ALSÓ GOMBOK PULZÁLÁSA A FÓKUSZ SZERINT ----
    void AnimateBottomButtons()
    {
        if (selectLabel == null || returnLabel == null) return;

        flickerTimer += Time.deltaTime;
        float pulse = (Mathf.Sin(flickerTimer * flickerSpeed) + 1f) / 2f;

        // SELECT
        if (focusedButton == 0)
        {
            selectLabel.color = Color.Lerp(colActiveDark, colActiveLight, pulse);
            selectLabel.fontSize = Mathf.Lerp(selectLabel.fontSize, 26f, Time.deltaTime * 10f);
            if (selectBgImage != null)
                selectBgImage.color = Color.Lerp(selectBgImage.color, colActiveBG, Time.deltaTime * 8f);
            if (selectAccent != null)
                selectAccent.color = new Color(0.6f, 0.2f, 1f, Mathf.Lerp(0.6f, 1f, pulse));
        }
        else
        {
            selectLabel.color = Color.Lerp(selectLabel.color, colIdleTxt, Time.deltaTime * 8f);
            selectLabel.fontSize = Mathf.Lerp(selectLabel.fontSize, 22f, Time.deltaTime * 10f);
            if (selectBgImage != null)
                selectBgImage.color = Color.Lerp(selectBgImage.color, colIdleBG, Time.deltaTime * 8f);
            if (selectAccent != null)
                selectAccent.color = Color.Lerp(selectAccent.color, new Color(0.6f, 0.2f, 1f, 0.4f), Time.deltaTime * 8f);
        }

        // RETURN
        if (focusedButton == 1)
        {
            returnLabel.color = Color.Lerp(colActiveDark, colActiveLight, pulse);
            returnLabel.fontSize = Mathf.Lerp(returnLabel.fontSize, 22f, Time.deltaTime * 10f);
            if (returnBgImage != null)
                returnBgImage.color = Color.Lerp(returnBgImage.color, colActiveBG, Time.deltaTime * 8f);
            if (returnAccent != null)
                returnAccent.color = new Color(0.6f, 0.2f, 1f, Mathf.Lerp(0.6f, 1f, pulse));
        }
        else
        {
            returnLabel.color = Color.Lerp(returnLabel.color, colIdleTxt, Time.deltaTime * 8f);
            returnLabel.fontSize = Mathf.Lerp(returnLabel.fontSize, 18f, Time.deltaTime * 10f);
            if (returnBgImage != null)
                returnBgImage.color = Color.Lerp(returnBgImage.color, colIdleBG, Time.deltaTime * 8f);
            if (returnAccent != null)
                returnAccent.color = Color.Lerp(returnAccent.color, new Color(0.6f, 0.2f, 1f, 0.4f), Time.deltaTime * 8f);
        }
    }

    public void NextCar()
    {
        if (isAnimating) return;
        StartCoroutine(SwitchCar(1));
    }

    public void PreviousCar()
    {
        if (isAnimating) return;
        StartCoroutine(SwitchCar(-1));
    }

    IEnumerator SwitchCar(int direction)
    {
        if (carRect == null || carList == null || carList.Length == 0) yield break;

        isAnimating = true;

        Vector2 exitTarget = carHomePosition + new Vector2(-direction * slideDistance, 0f);
        yield return SlideOut(carHomePosition, exitTarget);

        currentIndex += direction;
        if (currentIndex >= carList.Length) currentIndex = 0;
        else if (currentIndex < 0) currentIndex = carList.Length - 1;

        carDisplayImage.sprite = carList[currentIndex].carSprite;
        if (nameText != null) nameText.text = carList[currentIndex].carName;
        carRect.localScale = carList[currentIndex].garageScale;

        Vector2 enterStart = carHomePosition + new Vector2(direction * slideDistance, 0f);
        carRect.anchoredPosition = enterStart;
        yield return SlideIn(enterStart, carHomePosition);

        isAnimating = false;
    }

    IEnumerator SlideOut(Vector2 from, Vector2 to)
    {
        float t = 0f;
        while (t < slideDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / slideDuration);
            float eased = p * p;
            carRect.anchoredPosition = Vector2.Lerp(from, to, eased);
            if (carGroup != null)
                carGroup.alpha = Mathf.Lerp(1f, fadeLevel, eased);
            yield return null;
        }
        carRect.anchoredPosition = to;
        if (carGroup != null) carGroup.alpha = fadeLevel;
    }

    IEnumerator SlideIn(Vector2 from, Vector2 to)
    {
        float t = 0f;
        while (t < slideDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / slideDuration);
            float eased = 1f - (1f - p) * (1f - p);
            carRect.anchoredPosition = Vector2.Lerp(from, to, eased);
            if (carGroup != null)
                carGroup.alpha = Mathf.Lerp(fadeLevel, 1f, eased);
            yield return null;
        }
        carRect.anchoredPosition = to;
        if (carGroup != null) carGroup.alpha = 1f;
    }

    void UpdateUI()
    {
        if (carList == null || carList.Length == 0) return;
        if (carDisplayImage != null)
        {
            carDisplayImage.sprite = carList[currentIndex].carSprite;
            carRect.localScale = carList[currentIndex].garageScale;
        }
        if (nameText != null)
            nameText.text = carList[currentIndex].carName;
    }

    public void SelectCar()
    {
        if (isAnimating) return;
        if (carList == null || carList.Length == 0) return;

        PlayerPrefs.SetInt("SelectedCarID", currentIndex);
        PlayerPrefs.Save();
        Debug.Log("Autó elmentve: " + carList[currentIndex].carName);

        isAnimating = true; // tiltjuk a többi inputot a visszatérés alatt
        StartCoroutine(SelectAndReturn());
    }

    IEnumerator SelectAndReturn()
    {
        // Villogtató feedback, hogy érezze a user hogy ment
        if (carDisplayImage != null)
        {
            Color orig = carDisplayImage.color;
            for (int i = 0; i < 3; i++)
            {
                carDisplayImage.color = Color.white;
                yield return new WaitForSeconds(0.07f);
                carDisplayImage.color = orig;
                yield return new WaitForSeconds(0.07f);
            }
        }
        else
        {
            yield return new WaitForSeconds(0.4f);
        }

        // Vissza a Garage scene-be
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(backSceneName);
        else
            SceneManager.LoadScene(backSceneName);
    }

    public void GoBack()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(backSceneName);
        else
            SceneManager.LoadScene(backSceneName);
    }

    // ---- ALSÓ GOMBOK ÉPÍTÉSE ----
    void BuildBottomButtons()
    {
        Canvas canvas = EnsureCanvas();

        var container = new GameObject("BottomButtonsContainer");
        container.transform.SetParent(canvas.transform, false);
        var contRect = container.AddComponent<RectTransform>();
        contRect.anchorMin = new Vector2(0.5f, 0f);
        contRect.anchorMax = new Vector2(0.5f, 0f);
        contRect.pivot = new Vector2(0.5f, 0f);
        contRect.sizeDelta = new Vector2(400f, 200f);
        contRect.anchoredPosition = Vector2.zero;

        var selectGO = BuildBottomButton(container.transform, "SELECT",
            selectSize.x, selectSize.y, selectButtonBottomOffset, true,
            () => { focusedButton = 0; SelectCar(); });
        selectLabel = selectGO.GetComponentInChildren<TextMeshProUGUI>();
        selectAccent = selectGO.transform.Find("Accent").GetComponent<Image>();
        selectBgImage = selectGO.GetComponent<Image>();

        var returnGO = BuildBottomButton(container.transform, "< RETURN",
            returnSize.x, returnSize.y, returnButtonBottomOffset, false,
            () => { focusedButton = 1; GoBack(); });
        returnLabel = returnGO.GetComponentInChildren<TextMeshProUGUI>();
        returnAccent = returnGO.transform.Find("Accent").GetComponent<Image>();
        returnBgImage = returnGO.GetComponent<Image>();
    }

    GameObject BuildBottomButton(Transform parent, string label, float width, float height,
                                 float bottomOffset, bool isActive, System.Action onClick)
    {
        var btnGO = new GameObject(label + "_Btn");
        btnGO.transform.SetParent(parent, false);
        var rt = btnGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(0f, bottomOffset);

        var bg = btnGO.AddComponent<Image>();
        bg.color = isActive ? colActiveBG : colIdleBG;

        var accent = new GameObject("Accent");
        accent.transform.SetParent(btnGO.transform, false);
        var accImg = accent.AddComponent<Image>();
        accImg.color = new Color(0.6f, 0.2f, 1f, isActive ? 1f : 0.5f);
        var accRect = accent.GetComponent<RectTransform>();
        accRect.anchorMin = new Vector2(0f, 0f);
        accRect.anchorMax = new Vector2(0f, 1f);
        accRect.pivot = new Vector2(0f, 0.5f);
        accRect.sizeDelta = new Vector2(4f, 0f);
        accRect.anchoredPosition = Vector2.zero;

        var txtGO = new GameObject("Label");
        txtGO.transform.SetParent(btnGO.transform, false);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = isActive ? 26f : 18f;
        tmp.color = isActive ? colActiveDark : colIdleTxt;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableWordWrapping = false;
        var txtRect = txtGO.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = new Vector2(8f, 0f);
        txtRect.offsetMax = new Vector2(-8f, 0f);

        var btn = btnGO.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.15f);
        colors.pressedColor = new Color(0.6f, 0.2f, 1f, 0.4f);
        btn.colors = colors;
        btn.onClick.AddListener(() => onClick?.Invoke());

        return btnGO;
    }

    // ---- OLDALSÓ NYÍL GOMBOK ÉPÍTÉSE ----
    void BuildSideArrows()
    {
        Canvas canvas = EnsureCanvas();

        var leftGO = BuildArrow(canvas.transform, "LeftArrow", "‹", false);
        leftArrowBg = leftGO.GetComponent<Image>();
        leftArrowChar = leftGO.GetComponentInChildren<TextMeshProUGUI>();

        var rightGO = BuildArrow(canvas.transform, "RightArrow", "›", true);
        rightArrowBg = rightGO.GetComponent<Image>();
        rightArrowChar = rightGO.GetComponentInChildren<TextMeshProUGUI>();
    }

    GameObject BuildArrow(Transform parent, string name, string chevron, bool isRight)
    {
        var arrowGO = new GameObject(name);
        arrowGO.transform.SetParent(parent, false);
        var rt = arrowGO.AddComponent<RectTransform>();

        if (isRight)
        {
            rt.anchorMin = new Vector2(1f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(1f, 0.5f);
            rt.sizeDelta = arrowSize;
            rt.anchoredPosition = new Vector2(-arrowSideOffset, 0f);
        }
        else
        {
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.sizeDelta = arrowSize;
            rt.anchoredPosition = new Vector2(arrowSideOffset, 0f);
        }

        var bg = arrowGO.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.05f, 0.3f, 0.55f);

        var edge = new GameObject("Edge");
        edge.transform.SetParent(arrowGO.transform, false);
        var edgeImg = edge.AddComponent<Image>();
        edgeImg.color = new Color(0.6f, 0.2f, 1f, 0.85f);
        var edgeRect = edge.GetComponent<RectTransform>();
        if (isRight)
        {
            edgeRect.anchorMin = new Vector2(1f, 0f);
            edgeRect.anchorMax = new Vector2(1f, 1f);
            edgeRect.pivot = new Vector2(1f, 0.5f);
        }
        else
        {
            edgeRect.anchorMin = new Vector2(0f, 0f);
            edgeRect.anchorMax = new Vector2(0f, 1f);
            edgeRect.pivot = new Vector2(0f, 0.5f);
        }
        edgeRect.sizeDelta = new Vector2(4f, 0f);
        edgeRect.anchoredPosition = Vector2.zero;

        var txtGO = new GameObject("Chevron");
        txtGO.transform.SetParent(arrowGO.transform, false);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = chevron;
        tmp.fontSize = 56f;
        tmp.color = new Color(1f, 1f, 1f, 0.9f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableWordWrapping = false;
        var txtRect = txtGO.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        var btn = arrowGO.AddComponent<Button>();
        btn.targetGraphic = bg;
        var colors = btn.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 1f);
        colors.highlightedColor = new Color(0.7f, 0.5f, 1f, 1f);
        colors.pressedColor = new Color(0.5f, 0.2f, 1f, 1f);
        colors.selectedColor = new Color(1f, 1f, 1f, 1f);
        btn.colors = colors;

        if (isRight) btn.onClick.AddListener(NextCar);
        else btn.onClick.AddListener(PreviousCar);

        return arrowGO;
    }

    Canvas EnsureCanvas()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var cgo = new GameObject("CarSelectCanvas");
            cgo.transform.SetParent(transform, false);
            canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var sc = cgo.AddComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1920, 1080);
            cgo.AddComponent<GraphicRaycaster>();
        }
        return canvas;
    }
}
