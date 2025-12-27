using UnityEngine;

public class AICar : MonoBehaviour
{
    private bool wasOvertaken = false;
    public float aiWorldDistance;
    public float aiSpeed = 8f;
    public int currentLane;

    private Canvas myCanvas;
    private float baseSpeed;

    void Start()
    {
        myCanvas = GetComponent<Canvas>();
        baseSpeed = aiSpeed;
        aiSpeed = baseSpeed + Random.Range(-1.5f, 3f);

        // Ha spawnoláskor már mögöttünk van a képernyőn, eleve true
        float startRelX = aiWorldDistance - WorldProgress.playerDistance;
        if (startRelX < -1f) wasOvertaken = true;

        UpdateSorting();
    }

    void Update()
    {
        aiWorldDistance += aiSpeed * Time.deltaTime;
        float relativeX = aiWorldDistance - WorldProgress.playerDistance;

        if (!wasOvertaken)
        {
            // A játékos fixen -3-on van. Ha az AI -3.8 alá megy, megelőztük.
            if (relativeX < -3.8f)
            {
                // Csak akkor pont, ha a kocsi nem "ugrott" egyből a semmibe (-15 mögé)
                // és a játékos tényleg halad.
                if (relativeX > -15f && PlayerControllerSideView.speed > 5f)
                {
                    wasOvertaken = true;
                    ScoreManager.instance.AddPoint();
                }
                else
                {
                    wasOvertaken = true; // Pont nélkül jelöljük késznek
                }
            }
        }

        transform.position = new Vector3(relativeX, transform.position.y, 0);

        CheckAIConsistency();
        UpdateSorting();

        if (relativeX < -30f) Destroy(gameObject);
    }

    void CheckAIConsistency()
    {
        GameObject[] allCars = GameObject.FindGameObjectsWithTag("AICar");
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) return;
        float myWidth = rt.rect.width * transform.localScale.x;

        foreach (GameObject otherCar in allCars)
        {
            if (otherCar == gameObject) continue;
            AICar otherScript = otherCar.GetComponent<AICar>();
            if (otherScript != null && otherScript.currentLane == currentLane)
            {
                float dist = otherScript.aiWorldDistance - aiWorldDistance;
                float otherWidth = otherCar.GetComponent<RectTransform>().rect.width * otherCar.transform.localScale.x;
                float threshold = (myWidth / 2f + otherWidth / 2f);

                if (dist > 0 && dist < threshold + 1f)
                {
                    aiSpeed = Mathf.MoveTowards(aiSpeed, otherScript.aiSpeed, Time.deltaTime * 10f);
                    if (dist < threshold) aiWorldDistance = otherScript.aiWorldDistance - threshold - 0.01f;
                }
            }
        }
    }

    void UpdateSorting()
    {
        if (myCanvas != null)
            myCanvas.sortingOrder = Mathf.RoundToInt(10f - (transform.position.y * 10f));
    }
}