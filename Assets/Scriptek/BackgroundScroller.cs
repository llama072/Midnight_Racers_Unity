using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public float baseScrollSpeed = 5f; // max sebesség
    public float resetX = -20f; // amikor újrapozicionál
    public float startX = 20f;  // hova kerüljön vissza
    public Transform carTransform; // a kocsi referenciája
    public float maxCarSpeed = 100f; // állítsd be a játékod szerint

    void Update()
    {
        // lekérjük az autó sebességét (itt csak példa, majd testre szabod)
        float carSpeed = carTransform.GetComponent<CarHandling>().currentSpeed;

        // kiszámoljuk a scroll sebességet
        float speedFactor = Mathf.Clamp01(carSpeed / maxCarSpeed);
        float scrollSpeed = baseScrollSpeed * speedFactor;

        // mozgás
        transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

        // újrahelyezés ha kiért
        if (transform.position.x <= resetX)
        {
            Vector3 newPos = transform.position;
            newPos.x = startX;
            transform.position = newPos;
        }
    }
}