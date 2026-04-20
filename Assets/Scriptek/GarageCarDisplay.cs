using UnityEngine;
using UnityEngine.UI;

// Ez a script a Garage scene-ben lévő "Car" Image GameObject-re megy.
// Indításkor beolvassa a PlayerPrefs-ből a kiválasztott autót,
// beállítja a sprite-ot, és egységesíti a méretet (minden autó ugyanolyan magas lesz).
public class GarageCarDisplay : MonoBehaviour
{
    [Tooltip("Ugyanaz a CarData lista legyen, mint a CarSelector-ban (ugyanabban a sorrendben!)")]
    public CarData[] carList;

    [Header("Méretezés")]
    [Tooltip("Ha TRUE, minden autó ugyanolyan magas lesz (uniformHeight szerint). Ha FALSE, a CarData garageScale-je lesz használva.")]
    public bool uniformSizing = true;

    [Tooltip("Cél-magasság a canvas reference unitjában (1080p alapon). 260-300 között szokott jó lenni.")]
    [Range(50f, 600f)]
    public float uniformHeight = 260f;

    [Tooltip("Finomhangolás — ha valamelyik autó picit lejjebb/feljebb kell, per-index eltolás (Y pixelben)")]
    public float extraYOffset = 0f;

    private Image carImage;
    private RectTransform rectTransform;

    void Awake()
    {
        carImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        if (carList == null || carList.Length == 0)
        {
            Debug.LogWarning("GarageCarDisplay: nincs carList megadva!");
            return;
        }

        int selectedID = PlayerPrefs.GetInt("SelectedCarID", 0);
        if (selectedID < 0 || selectedID >= carList.Length) selectedID = 0;

        CarData data = carList[selectedID];
        if (data == null || data.carSprite == null) return;

        if (carImage != null)
            carImage.sprite = data.carSprite;

        if (rectTransform == null)
        {
            Debug.Log("Garázsban megjelenítve: " + data.carName);
            return;
        }

        if (uniformSizing)
        {
            // Auto-normalizálás: az adott sprite aspect-jét megtartva a target magasságra állítunk
            Sprite s = data.carSprite;
            float nativeW = s.rect.width;
            float nativeH = s.rect.height;
            if (nativeH <= 0f) nativeH = 1f;

            float aspect = nativeW / nativeH;
            float targetW = uniformHeight * aspect;

            rectTransform.sizeDelta = new Vector2(targetW, uniformHeight);
            rectTransform.localScale = Vector3.one;
        }
        else
        {
            // Régi működés: használjuk a CarData-ban beállított skálát
            if (carImage != null) carImage.SetNativeSize();
            rectTransform.localScale = data.garageScale;
        }

        // Y eltolás finomhangolás
        if (extraYOffset != 0f)
        {
            Vector2 p = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = new Vector2(p.x, p.y + extraYOffset);
        }

        Debug.Log("Garázsban megjelenítve: " + data.carName + "  (sizeDelta: " + rectTransform.sizeDelta + ")");
    }
}
