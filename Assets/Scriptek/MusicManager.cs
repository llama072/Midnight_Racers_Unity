using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Audio Source")]
    private AudioSource musicSource;
    private AudioLowPassFilter lowPass;

    [Header("Beállítások")]
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float defaultVolume = 1f;

    [Header("Intro zene")]
    [SerializeField] private AudioClip introClip;
    [SerializeField] private string introTitle = "Ismeretlen";
    [SerializeField] private string introArtist = "";

    private List<TrackData> currentPlaylist = new List<TrackData>();
    private List<int> remainingIndices = new List<int>();
    private int currentIndex = -1;
    private bool introPlayed = false;
    private Coroutine fadeCoroutine;

    public TrackData CurrentTrack => currentIndex >= 0 && currentIndex < currentPlaylist.Count
        ? currentPlaylist[currentIndex] : null;

    [System.Serializable]
    public class TrackData
    {
        public AudioClip clip;
        public string title;
        public string artist;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = false;
        musicSource.volume = defaultVolume;

        lowPass = gameObject.AddComponent<AudioLowPassFilter>();
        lowPass.cutoffFrequency = 22000f;
        lowPass.enabled = false;
    }

    void Update()
    {
        if (introPlayed && !musicSource.isPlaying && currentPlaylist.Count > 0)
            PlayNext();
    }

    // --- Külső hívások ---



    public void SetPlaylistWithoutPlaying(List<TrackData> tracks, bool withLowPass = false)
    {
        currentPlaylist = new List<TrackData>(tracks);
        ShuffleIndices();
        SetLowPass(withLowPass);
        // Nem indít el semmit — az intro fog szólni
    }

    public void PlayIntro()
    {
        if (introClip == null) { introPlayed = true; return; }
        musicSource.clip = introClip;
        musicSource.volume = defaultVolume;
        musicSource.Play();
        NotifyWidget(introTitle, introArtist);
        StartCoroutine(WaitForIntroEnd());
    }

    public void SetPlaylist(List<TrackData> tracks, bool withLowPass = false)
    {
        currentPlaylist = new List<TrackData>(tracks);
        ShuffleIndices();
        SetLowPass(withLowPass);

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeToNext());
    }

    public void SkipToNext()
    {
        if (currentPlaylist.Count == 0) return;
        int nextIndex = (currentIndex + 1) % currentPlaylist.Count;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeToSpecific(nextIndex));
    }

    public void SkipToPrev()
    {
        if (currentPlaylist.Count == 0) return;
        int prevIndex = (currentIndex - 1 + currentPlaylist.Count) % currentPlaylist.Count;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeToSpecific(prevIndex));
    }

    public void TogglePause()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Pause();
            isPlaying = false;
        }
        else
        {
            musicSource.UnPause();
            isPlaying = true;
        }
    }

    public bool IsPlaying => isPlaying;

    private bool isPlaying = false;


    // --- Belső logika ---

    void ShuffleIndices()
    {
        remainingIndices = new List<int>();
        for (int i = 0; i < currentPlaylist.Count; i++)
            remainingIndices.Add(i);
        // Fisher-Yates shuffle
        for (int i = remainingIndices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int tmp = remainingIndices[i];
            remainingIndices[i] = remainingIndices[j];
            remainingIndices[j] = tmp;
        }
    }

    void PlayNext()
    {
        if (remainingIndices.Count == 0) ShuffleIndices();
        currentIndex = remainingIndices[0];
        remainingIndices.RemoveAt(0);
        PlayCurrent();
    }

    IEnumerator FadeToNext()
    {
        yield return FadeOut();
        musicSource.Stop();
        PlayNext();
        yield return FadeIn();
    }

    IEnumerator FadeToSpecific(int index)
    {
        yield return FadeOut();
        musicSource.Stop();
        currentIndex = index;
        // Kivesszük a remainingIndices-ből ha benne van
        remainingIndices.Remove(index);
        PlayCurrent();
        yield return FadeIn();
    }

    void PlayCurrent()
    {
        if (currentIndex < 0 || currentIndex >= currentPlaylist.Count) return;
        var track = currentPlaylist[currentIndex];
        musicSource.clip = track.clip;
        musicSource.Play();
        NotifyWidget(track.title, track.artist);
    }

    IEnumerator WaitForIntroEnd()
    {
        yield return new WaitUntil(() => !musicSource.isPlaying);
        introPlayed = true;
    }

    IEnumerator FadeOut()
    {
        float start = musicSource.volume, t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(start, 0f, t / fadeDuration);
            yield return null;
        }
        musicSource.volume = 0f;
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, defaultVolume, t / fadeDuration);
            yield return null;
        }
        musicSource.volume = defaultVolume;
    }

    void SetLowPass(bool enabled)
    {
        if (enabled)
        {
            lowPass.enabled = true;
            StartCoroutine(AnimateLowPass(lowPass.cutoffFrequency, 800f));
        }
        else
        {
            StartCoroutine(AnimateLowPass(lowPass.cutoffFrequency, 22000f, disableAfter: true));
        }
    }

    IEnumerator AnimateLowPass(float from, float to, bool disableAfter = false)
    {
        lowPass.enabled = true;
        float t = 0f, dur = 1.2f;
        while (t < dur)
        {
            t += Time.deltaTime;
            lowPass.cutoffFrequency = Mathf.Lerp(from, to, t / dur);
            yield return null;
        }
        lowPass.cutoffFrequency = to;
        if (disableAfter) lowPass.enabled = false;
    }

    void NotifyWidget(string title, string artist)
    {
        var widget = FindObjectOfType<NowPlayingWidget>();
        if (widget != null) widget.RefreshTrack(title, artist);
    }
}