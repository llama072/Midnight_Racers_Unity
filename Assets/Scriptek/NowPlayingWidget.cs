using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// NOW PLAYING WIDGET v2 — Hover interakcióval
/// </summary>
public class NowPlayingWidget : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("Zeneszámok listája")]
    [SerializeField]
    private List<TrackInfo> tracks = new List<TrackInfo>
    {
        new TrackInfo { title = "Zoom Noir",   artist = "i9incher"  },
        new TrackInfo { title = "ugh",         artist = "DJ Kuroneko"},
        new TrackInfo { title = "u lose",      artist = "Dazegxd"   },
        new TrackInfo { title = "stray - VIP", artist = "DJ Kuroneko"},
    };

    [Header("Vinyl")]
    [SerializeField] private Sprite vinylSprite;
    [SerializeField] private float vinylRotateSpeed = 45f;

    [Header("Animáció időzítés")]
    [SerializeField] private float slideInDuration = 0.5f;
    [SerializeField] private float holdDuration = 4f;
    [SerializeField] private float slideOutDuration = 0.4f;
    [SerializeField] private float delayBeforeShow = 1.5f;
    [SerializeField] private float controlsFadeDuration = 0.2f;

    [Header("Ismétlés")]
    [SerializeField] private bool loop = true;
    [SerializeField] private float loopInterval = 14f;

    private RectTransform widgetRect;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI artistText;
    private TextMeshProUGUI playPauseLbl;
    private RectTransform vinylRect;
    private CanvasGroup canvasGroup;
    private CanvasGroup controlsGroup;

    private readonly float widgetWidth = 320f;
    private readonly float widgetHeight = 72f;

    private Vector2 hiddenPos;
    private Vector2 visiblePos;

    private bool isPlaying = true;

    private int currentTrack = 0;
    private bool isHovered = false;
    private bool isVisible = false;
    private Coroutine slideCoroutine;
    private Coroutine holdCoroutine;

    [System.Serializable]
    public class TrackInfo { public string title; public string artist; }

    private void Start()
    {
        BuildWidget();
        SetControlsAlpha(0f);
        StartCoroutine(ShowSequence(delayBeforeShow));
    }

    private void BuildWidget()
    {
        widgetRect = gameObject.AddComponent<RectTransform>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Láthatatlan Image a root-on — nélküle a hover eventek nem érnek ide
        var rootImg = gameObject.AddComponent<Image>();
        rootImg.color = new Color(0, 0, 0, 0);
        rootImg.raycastTarget = true;

        widgetRect.anchorMin = new Vector2(1f, 0f);
        widgetRect.anchorMax = new Vector2(1f, 0f);
        widgetRect.pivot = new Vector2(1f, 0f);
        widgetRect.sizeDelta = new Vector2(widgetWidth, widgetHeight);

        visiblePos = new Vector2(-24f, 24f);
        hiddenPos = new Vector2(widgetWidth + 34f, 24f);
        widgetRect.anchoredPosition = hiddenPos;

        // Háttér
        var bg = MakeGO("BG", transform);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.04f, 0.06f, 0.18f, 0.93f);
        bgImg.raycastTarget = true;  // ← ez fogja az egér eventeket
        Stretch(bg.GetComponent<RectTransform>());
        bg.AddComponent<Outline>().effectColor = new Color(0f, 0.7f, 1f, 0.45f);

        // Narancssárga bal csík
        var accent = MakeGO("Accent", transform);
        var accentImg = accent.AddComponent<Image>();
        accentImg.color = new Color(1f, 0.42f, 0f, 1f);
        var ar = accent.GetComponent<RectTransform>();
        ar.anchorMin = new Vector2(0f, 0f); ar.anchorMax = new Vector2(0f, 1f);
        ar.pivot = new Vector2(0f, 0.5f);
        ar.sizeDelta = new Vector2(4f, 0f);
        ar.anchoredPosition = Vector2.zero;

        // Vinyl
        float vs = 48f;
        var vinylGO = MakeGO("Vinyl", transform);
        var vinylImg = vinylGO.AddComponent<Image>();
        vinylImg.color = vinylSprite != null ? Color.white : new Color(0.15f, 0.15f, 0.25f);
        if (vinylSprite != null) vinylImg.sprite = vinylSprite;
        vinylRect = vinylGO.GetComponent<RectTransform>();
        vinylRect.anchorMin = new Vector2(0f, 0.5f); vinylRect.anchorMax = new Vector2(0f, 0.5f);
        vinylRect.pivot = new Vector2(0.5f, 0.5f);
        vinylRect.sizeDelta = new Vector2(vs, vs);
        vinylRect.anchoredPosition = new Vector2(16f + vs / 2f, 0f);

        // Szövegek
        float tx = 16f + vs + 16f;

        var nowLbl = MakeTMP("NowLbl", transform, new Vector2(tx, 14f), new Vector2(-tx - 8f, 14f), 7f, new Color(1f, 0.42f, 0f, 0.9f));
        nowLbl.text = "* NOW PLAYING"; nowLbl.fontStyle = FontStyles.Bold;

        titleText = MakeTMP("Title", transform, new Vector2(tx, -1f), new Vector2(-tx - 8f, 18f), 13f, new Color(0.9f, 0.93f, 1f));
        titleText.fontStyle = FontStyles.Bold; titleText.overflowMode = TextOverflowModes.Ellipsis;

        artistText = MakeTMP("Artist", transform, new Vector2(tx, -18f), new Vector2(-tx - 8f, 16f), 10f, new Color(0.55f, 0.65f, 0.85f, 0.85f));
        artistText.overflowMode = TextOverflowModes.Ellipsis;

        UpdateTrackDisplay();

        // Kontroll gombok — jobb oldalon
        var ctrlGO = MakeGO("Controls", transform);
        controlsGroup = ctrlGO.AddComponent<CanvasGroup>();
        Stretch(ctrlGO.GetComponent<RectTransform>());

        var prev = MakeControlBtn("Prev", ctrlGO.transform, new Vector2(-88f, 0f), "<<");
        var playPause = MakeControlBtn("PlayPause", ctrlGO.transform, new Vector2(-52f, 0f), "||");
        var next = MakeControlBtn("Next", ctrlGO.transform, new Vector2(-16f, 0f), ">>");

        // Jobb oldali horgony mindháromra
        foreach (var btn in new[] { prev, playPause, next })
        {
            var r = btn.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(1f, 0.5f);
            r.anchorMax = new Vector2(1f, 0.5f);
            r.pivot = new Vector2(1f, 0.5f);
        }

        // PlayPause gomb szöveg referencia tárolása
        playPauseLbl = playPause.GetComponentInChildren<TextMeshProUGUI>();

        prev.onClick.AddListener(PrevTrack);
        next.onClick.AddListener(NextTrack);
        playPause.onClick.AddListener(TogglePlayPause);
    }

    public void OnPointerEnter(PointerEventData _)
    {
        isHovered = true;
        if (slideCoroutine != null) { StopCoroutine(slideCoroutine); slideCoroutine = null; }
        if (holdCoroutine != null) { StopCoroutine(holdCoroutine); holdCoroutine = null; }
        // Smooth visszacsúszás a jelenlegi pozícióból
        slideCoroutine = StartCoroutine(SlideTo(widgetRect.anchoredPosition, visiblePos, slideInDuration, true));
        StartCoroutine(FadeControls(1f));
    }

    public void OnPointerExit(PointerEventData _)
    {
        isHovered = false;
        StartCoroutine(FadeControls(0f));
        // Rövid várakozás után visszacsúszik, majd loopol
        holdCoroutine = StartCoroutine(HoldThenHide());
    }

    private void NextTrack() { currentTrack = (currentTrack + 1) % tracks.Count; UpdateTrackDisplay(); }
    private void PrevTrack() { currentTrack = (currentTrack - 1 + tracks.Count) % tracks.Count; UpdateTrackDisplay(); }

    private void TogglePlayPause()
    {
        isPlaying = !isPlaying;
        if (playPauseLbl != null)
            playPauseLbl.text = isPlaying ? "||" : ">";
    }

    private void UpdateTrackDisplay()
    {
        if (tracks == null || tracks.Count == 0) return;
        var t = tracks[currentTrack];
        if (titleText != null) titleText.text = t.title;
        if (artistText != null) artistText.text = t.artist;
    }

    private IEnumerator ShowSequence(float delay)
    {
        yield return new WaitForSeconds(delay);

        while (true)  // végtelen loop, a feltétel belül kezelt
        {
            // Slide IN
            widgetRect.anchoredPosition = hiddenPos;
            slideCoroutine = StartCoroutine(SlideTo(hiddenPos, visiblePos, slideInDuration, true));
            yield return slideCoroutine;
            slideCoroutine = null;
            isVisible = true;

            // Hold — de ha hover megszakítja, várunk amíg az pointer exit újraindítja
            if (!isHovered)
            {
                holdCoroutine = StartCoroutine(HoldThenHide());
                yield return holdCoroutine;
                holdCoroutine = null;
            }
            else
            {
                // Hover van — várunk amíg elmegy az egér és HoldThenHide lefut
                while (isHovered) yield return null;
                if (holdCoroutine != null)
                {
                    yield return holdCoroutine;
                    holdCoroutine = null;
                }
            }

            isVisible = false;

            if (!loop) yield break;
            yield return new WaitForSeconds(loopInterval);
        }
    }

    private IEnumerator HoldThenHide()
    {
        yield return new WaitForSeconds(holdDuration);

        if (!isHovered)
        {
            slideCoroutine = StartCoroutine(SlideTo(visiblePos, hiddenPos, slideOutDuration, false));
            yield return slideCoroutine;
            slideCoroutine = null;
        }
    }

    private IEnumerator SlideTo(Vector2 from, Vector2 to, float dur, bool easeOut)
    {
        float elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dur);
            float e = easeOut ? 1f - Mathf.Pow(1f - t, 3f) : Mathf.Pow(t, 2f);
            widgetRect.anchoredPosition = Vector2.Lerp(from, to, e);
            yield return null;
        }
        widgetRect.anchoredPosition = to;
    }

    private IEnumerator FadeControls(float target)
    {
        float start = controlsGroup.alpha, elapsed = 0f;
        while (elapsed < controlsFadeDuration)
        {
            elapsed += Time.deltaTime;
            controlsGroup.alpha = Mathf.Lerp(start, target, elapsed / controlsFadeDuration);
            yield return null;
        }
        controlsGroup.alpha = target;
        controlsGroup.interactable = controlsGroup.blocksRaycasts = target > 0.5f;
    }

    private void SetControlsAlpha(float a)
    {
        if (controlsGroup == null) return;
        controlsGroup.alpha = a;
        controlsGroup.interactable = controlsGroup.blocksRaycasts = a > 0.5f;
    }

    private void Update()
    {
        if (vinylRect != null && isPlaying)
            vinylRect.Rotate(0f, 0f, -vinylRotateSpeed * Time.deltaTime);
    }

    public void RefreshTrack(string title, string artist)
    {
        tracks[currentTrack] = new TrackInfo { title = title, artist = artist };
        UpdateTrackDisplay();
    }

    // ── UI helpers ──────────────────────────────────────────
    private GameObject MakeGO(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private TextMeshProUGUI MakeTMP(string name, Transform parent, Vector2 pos, Vector2 size, float fs, Color col)
    {
        var go = MakeGO(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = fs; tmp.color = col; tmp.alignment = TextAlignmentOptions.Left;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0f, 0.5f); r.anchorMax = new Vector2(1f, 0.5f);
        r.pivot = new Vector2(0f, 0.5f); r.sizeDelta = size; r.anchoredPosition = pos;
        return tmp;
    }

    private Button MakeControlBtn(string name, Transform parent, Vector2 pos, string label)
    {
        var go = MakeGO(name, parent);
        var img = go.AddComponent<Image>(); img.color = new Color(0, 0, 0, 0);
        var btn = go.AddComponent<Button>();
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0f, 0.5f); r.anchorMax = new Vector2(0f, 0.5f);
        r.pivot = new Vector2(0.5f, 0.5f); r.sizeDelta = new Vector2(28f, 28f); r.anchoredPosition = pos;

        var lgo = MakeGO("Lbl", go.transform);
        var lt = lgo.AddComponent<TextMeshProUGUI>();
        lt.text = label; lt.fontSize = 11f;
        lt.color = new Color(1f, 0.42f, 0f, 1f);
        lt.alignment = TextAlignmentOptions.Center;
        Stretch(lgo.GetComponent<RectTransform>());

        var c = btn.colors;
        c.highlightedColor = new Color(1f, 0.42f, 0f, 0.2f);
        c.pressedColor = new Color(1f, 0.42f, 0f, 0.4f);
        btn.colors = c;
        return btn;
    }

    private void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.sizeDelta = Vector2.zero; r.anchoredPosition = Vector2.zero;
    }
}