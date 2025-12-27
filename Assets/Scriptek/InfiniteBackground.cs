using UnityEngine;

public class InfiniteBackgroundSideView : MonoBehaviour
{
    public RectTransform bg1;
    public RectTransform bg2;

    private float backgroundWidth;
    private float currentVisualSpeed = 0f;

    private void Start()
    {
        if (bg1 == null || bg2 == null)
        {
            Debug.LogError("Az InfiniteBackgroundSideView szkripten a bg1 vagy bg2 nincs beállítva!");
            enabled = false;
            return;
        }
        backgroundWidth = bg1.rect.width;
    }

    public void SetScrollSpeed(float speed)
    {
        currentVisualSpeed = speed;
    }

    void Update()
    {
        float move = currentVisualSpeed * Time.deltaTime;
        bg1.anchoredPosition -= new Vector2(move, 0);
        bg2.anchoredPosition -= new Vector2(move, 0);

        if (bg1.anchoredPosition.x <= -backgroundWidth) bg1.anchoredPosition += new Vector2(backgroundWidth * 2, 0);
        if (bg2.anchoredPosition.x <= -backgroundWidth) bg2.anchoredPosition += new Vector2(backgroundWidth * 2, 0);
        if (bg1.anchoredPosition.x >= backgroundWidth) bg1.anchoredPosition -= new Vector2(backgroundWidth * 2, 0);
        if (bg2.anchoredPosition.x >= backgroundWidth) bg2.anchoredPosition -= new Vector2(backgroundWidth * 2, 0);
    }
}
