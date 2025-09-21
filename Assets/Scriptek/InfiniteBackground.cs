using UnityEngine;

public class InfiniteBackgroundSideView : MonoBehaviour
{
    public RectTransform bg1;
    public RectTransform bg2;
    public PlayerControllerSideView player;

    private float backgroundWidth;

    private void Start()
    {
        backgroundWidth = bg1.rect.width; // UI Image szélessége
    }

    void Update()
    {
        float move = player.speed * Time.deltaTime;

        bg1.anchoredPosition -= new Vector2(move, 0);
        bg2.anchoredPosition -= new Vector2(move, 0);

        // Ha kilép balra, visszadobjuk jobbra
        if (bg1.anchoredPosition.x <= -backgroundWidth)
        {
            bg1.anchoredPosition += new Vector2(backgroundWidth * 2, 0);
        }
        if (bg2.anchoredPosition.x <= -backgroundWidth)
        {
            bg2.anchoredPosition += new Vector2(backgroundWidth * 2, 0);
        }


        // Jobbra kilépett
        if (bg1.anchoredPosition.x >= backgroundWidth)
        {
            bg1.anchoredPosition -= new Vector2(backgroundWidth * 2, 0);
        }
        if (bg2.anchoredPosition.x >= backgroundWidth)
        {
            bg2.anchoredPosition -= new Vector2(backgroundWidth * 2, 0);
        }
    }
}
