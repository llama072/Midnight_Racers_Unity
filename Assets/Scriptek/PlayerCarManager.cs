using UnityEngine;
using UnityEngine.UI;

public class PlayerCarManager : MonoBehaviour
{
    public CarData[] carList;
    public RectTransform frontWheel;
    public RectTransform backWheel;

    private Image carImage;
    private RectTransform rectTransform;

    void Awake()
    {
        // 1. Alap komponensek lekérése
        carImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        // 2. Adatok betöltése
        int selectedID = PlayerPrefs.GetInt("SelectedCarID", 0);
        if (carList == null || selectedID >= carList.Length) return;

        CarData data = carList[selectedID];

        // 3. Offset elmentése a Controller számára
        PlayerPrefs.SetFloat("CarOffset", data.carBodyY);
        PlayerPrefs.Save(); // Biztosítsuk, hogy elmentődik

        // 4. Kinézet beállítása
        carImage.sprite = data.carSprite;
        rectTransform.localScale = data.gameScale;

        // 5. Autó magasságának beállítása a sávokhoz képest
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, data.carBodyY);

        // 6. Kerekek helyére tétele
        if (frontWheel != null && backWheel != null)
        {
            frontWheel.anchoredPosition = new Vector2(data.frontWheelX, data.wheelsY);
            backWheel.anchoredPosition = new Vector2(data.backWheelX, data.wheelsY);
        }

        Debug.Log(data.carName + " betöltve, eltolás: " + data.carBodyY);
    }
}