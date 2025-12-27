using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI; // ÚJ: Ez kell az Image típushoz

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public Image timeBarFill; // ÚJ: Ide húzzuk be a csíkot
    public TextMeshProUGUI timerText; // Megtarthatod a számot is, ha akarod
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreValue;
    public CanvasGroup panelCanvasGroup;

    [Header("Settings")]
    public float maxTime = 60f; // ÚJ: Eltároljuk a kiinduló időt
    private float timeLeft;
    public float pointCooldown = 0.2f;

    private int score = 0;
    private bool isGameOver = false;
    private float lastPointTime = -1f;

    void Awake()
    {
        instance = this;
        Time.timeScale = 1f;
        PlayerControllerSideView.speed = 0f;
        WorldProgress.playerDistance = 0f;
        score = 0;

        timeLeft = maxTime; // Az időt a maxTime-ról indítjuk

        if (scoreText != null) scoreText.text = "Score: 0";
        if (finalScoreValue != null) finalScoreValue.text = "Score: 0";
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (panelCanvasGroup != null) panelCanvasGroup.alpha = 0;
    }

    void Update()
    {
        if (isGameOver) return;
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            UpdateTimerUI();
        }
        else GameOver();
    }

    void UpdateTimerUI()
    {
        if (timeBarFill != null)
        {
            timeBarFill.fillAmount = timeLeft / maxTime;

            // Ez az egy sor kiváltja az összes if-et: 
            // 1.0-nál fehér, ahogy fogy, úgy lesz egyre narancssárgább, majd mélyvörös
            timeBarFill.color = Color.Lerp(Color.red, Color.blue, timeBarFill.fillAmount);
        }

        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(timeLeft).ToString();
        }
    }

    public void AddPoint()
    {
        if (isGameOver) return;

        // BIZTONSÁGI IDŐKORLÁT: Ha a háttérváltás miatt több autó is "átugrana" rajtunk,
        // csak az elsőért adunk pontot, a többit elvetjük a cooldown ideje alatt.
        if (Time.time - lastPointTime < pointCooldown)
        {
            Debug.Log("Dupla pont blokkolva a biztonsági időkorlát miatt!");
            return;
        }

        lastPointTime = Time.time;
        score++;
        if (scoreText != null) scoreText.text = "Score: " + score;
    }

    void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (finalScoreValue != null)
            finalScoreValue.text = "Score: " + score.ToString();

        if (DatabaseManager.LoggedInUserId != -1)
            DatabaseManager.instance.SaveScore(score);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            StartCoroutine(FadeInPanel());
        }
        Time.timeScale = 0f;
    }

    IEnumerator FadeInPanel()
    {
        if (panelCanvasGroup == null) yield break;
        float elapsed = 0;
        while (elapsed < 0.5f)
        {
            elapsed += Time.unscaledDeltaTime;
            panelCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / 0.5f);
            yield return null;
        }
        panelCanvasGroup.alpha = 1;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}