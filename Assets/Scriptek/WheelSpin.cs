using UnityEngine;
using UnityEngine.UI;

public class WheelSpin : MonoBehaviour
{
    [Header("Kerék Beállítások")]
    [Tooltip("Ezzel a csúszkával állítsd be a kerék végső méretét!")]
    [Range(0.1f, 2f)]
    public float wheelSizeSlider = 1f;

    public float rotationMultiplier = 15f;

    private PlayerControllerSideView player;
    private AICar ai; // <--- Hozzáadjuk az AI hivatkozást (Ellenőrizd az osztálynevet!)

    private float currentZ = 0f;
    private Transform parentTrans;

    void Start()
    {
        parentTrans = transform.parent;

        // Megkeressük a szülőt: lehet játékos VAGY AI
        player = GetComponentInParent<PlayerControllerSideView>();
        ai = GetComponentInParent<AICar>(); // <--- Itt keresse az AI szkriptet
    }

    void LateUpdate()
    {
        // 1. SEBESSÉG MEGHATÁROZÁSA
        float speed = 0;

        if (player != null)
        {
            // JAVÍTÁS: példány helyett az osztályon keresztül hivatkozunk
            speed = PlayerControllerSideView.speed;
        }
        else if (ai != null)
        {
            speed = ai.aiSpeed;
        }

        // 2. FORGATÁS marad a régi...
        currentZ -= speed * rotationMultiplier * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(0, 0, currentZ);

        // 3. MÉRET JAVÍTÁSA marad a régi...
        if (parentTrans != null)
        {
            float fixX = 1f / parentTrans.localScale.x;
            float fixY = 1f / parentTrans.localScale.y;
            transform.localScale = new Vector3(fixX * wheelSizeSlider, fixY * wheelSizeSlider, 1f);
        }
    }
}