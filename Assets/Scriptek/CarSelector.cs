using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CarSelector : MonoBehaviour
{
    [Header("UI References")]
    public Image carDisplayImage;
    public TextMeshProUGUI nameText;

    [Header("Car List")]
    public CarData[] carList;
    private int currentIndex = 0;

    void Start()
    {
        UpdateUI();
    }

    public void NextCar()
    {
        currentIndex++;
        if (currentIndex >= carList.Length) currentIndex = 0;
        UpdateUI();
    }

    public void PreviousCar()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = carList.Length - 1;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (carList.Length == 0) return;

        carDisplayImage.sprite = carList[currentIndex].carSprite;
        nameText.text = carList[currentIndex].carName;

        // FIGYELEM: Itt a 'garageScale'-t használjuk, mert a Garázsban vagyunk!
        carDisplayImage.rectTransform.localScale = carList[currentIndex].garageScale;
    }

    public void SelectCar()
    {
        PlayerPrefs.SetInt("SelectedCarID", currentIndex);
        PlayerPrefs.Save();
        Debug.Log("Autó elmentve: " + carList[currentIndex].carName);
    }
}