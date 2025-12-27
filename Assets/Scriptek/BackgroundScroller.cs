using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public Transform playerCarTransform;
    public InfiniteBackgroundSideView backgroundVisuals;

    [HideInInspector] public float finalWorldScrollSpeed;
    public float resetX = -20f;
    public float startX = 20f;

    void Update()
    {
        if (playerCarTransform == null || backgroundVisuals == null)
        {
            Debug.LogError("Hiányzó referenciák a BackgroundScroller-ben!");
            return;
        }

        // JAVÍTÁS: Nem kell GetComponent, közvetlenül az osztályból kérjük el a statikus sebességet
        float playerCarSpeed = PlayerControllerSideView.speed;
        finalWorldScrollSpeed = playerCarSpeed;

        transform.Translate(Vector3.left * finalWorldScrollSpeed * Time.deltaTime);
        backgroundVisuals.SetScrollSpeed(finalWorldScrollSpeed);

        if (transform.position.x <= resetX)
        {
            Vector3 newPos = transform.position;
            newPos.x = startX;
            transform.position = newPos;
        }
    }

}
