using UnityEngine;

public class PlayerControllerSideView : MonoBehaviour
{
    private int? pendingLaneRequest = null; // Tárolja, ha sávot akarunk váltani, de foglalt volt
    public float[] laneYPositions = { -3.5f, -2.5f, -1.4f, -0.4f };
    public int currentLane = 1;
    public float laneMoveSpeed = 10f;

    public static float speed = 0f;
    public float maxSpeed = 1000f;
    public float acceleration = 500f;

    [Header("Collision Settings")]
    public float pushForce = 200f;
    public float crashSlowdown = 0.5f;

    void Update()
    {
        // 1. Input és Sávváltási logika kezelése
        HandleInput();

        if (pendingLaneRequest.HasValue)
        {
            TryChangeLane(pendingLaneRequest.Value);
        }

        // 2. MOZGÁS: Itt adjuk hozzá a mentett CarOffset értéket
        Vector3 pos = transform.localPosition;

        // Lekérjük az autó egyedi eltolását
        float currentOffset = PlayerPrefs.GetFloat("CarOffset", 0);
        float targetY = laneYPositions[currentLane] + currentOffset;

        // Simított mozgás a sávok között
        pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * laneMoveSpeed);
        pos.x = -3; // Fix vízszintes pozíció
        transform.localPosition = pos;

        // 3. Egyéb rendszerek frissítése
        UpdateSortingOrder();
        WorldProgress.playerDistance += speed * Time.deltaTime;
        CheckCollisions();
    }

    void HandleInput()
    {
        // 1. SÁVVÁLTÁS KÉRÉSE (TÁROLÁSSAL)
        if (Input.GetKeyDown(KeyCode.W)) pendingLaneRequest = currentLane + 1;
        if (Input.GetKeyDown(KeyCode.S)) pendingLaneRequest = currentLane - 1;

        // 2. SEBESSÉGKEZELÉS (EZ HIÁNYZOTT!)
        float input = 0;
        if (Input.GetKey(KeyCode.D)) input = 1;      // Gáz
        else if (Input.GetKey(KeyCode.A)) input = -1; // Fék

        // Gyorsulás és lassulás számítása
        speed += input * acceleration * Time.deltaTime;

        // Sebesség korlátozása
        speed = Mathf.Clamp(speed, -1000f, maxSpeed);

        // Ha nem nyomsz semmit, guruljon lassulva (motorfék)
        if (input == 0)
        {
            speed = Mathf.MoveTowards(speed, 0, acceleration * Time.deltaTime);
        }
    }

    void TryChangeLane(int target)
    {
        // Ellenőrizzük, hogy a célsáv egyáltalán létezik-e
        if (target >= 0 && target < laneYPositions.Length)
        {
            if (!IsLaneOccupied(target))
            {
                currentLane = target;
                pendingLaneRequest = null; // Sikerült a váltás, töröljük a kérést!
            }
            // Ha foglalt, nem csinálunk semmit, a pendingLaneRequest marad az érték, 
            // így az Update következő futásakor újra megpróbálja.
        }
        else
        {
            pendingLaneRequest = null; // Ha érvénytelen sáv (pályán kívül), töröljük
        }
    }

    void UpdateSortingOrder()
    {
        Canvas playerCanvas = GetComponent<Canvas>();
        if (playerCanvas != null)
        {
            playerCanvas.sortingOrder = Mathf.RoundToInt(10f - (transform.position.y * 10f));
        }
    }

    bool IsLaneOccupied(int laneIndex)
    {
        GameObject[] cars = GameObject.FindGameObjectsWithTag("AICar");
        float myWidth = GetComponent<RectTransform>().rect.width * transform.localScale.x;

        foreach (GameObject car in cars)
        {
            AICar script = car.GetComponent<AICar>();
            if (script != null && script.currentLane == laneIndex)
            {
                float distanceX = car.transform.position.x - transform.position.x;
                float aiWidth = car.GetComponent<RectTransform>().rect.width * car.transform.localScale.x;

                // ENGEDÉKENYSÉG: Csak az autók szélességének 60%-át vesszük figyelembe a váltásnál
                // Így ha csak az orrod érne hozzá a hátuljához, még engedni fog váltani.
                float safetyMargin = (myWidth / 2f + aiWidth / 2f) * 0.9f;

                if (Mathf.Abs(distanceX) < safetyMargin)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void CheckCollisions()
{
    GameObject[] cars = GameObject.FindGameObjectsWithTag("AICar");
    float myWidth = GetComponent<RectTransform>().rect.width * transform.localScale.x;

    foreach (GameObject car in cars)
    {
        AICar script = car.GetComponent<AICar>();
        if (script == null || script.currentLane != currentLane) continue;

        float distanceX = car.transform.position.x - transform.position.x;
        float aiWidth = car.GetComponent<RectTransform>().rect.width * car.transform.localScale.x;
        float collisionThreshold = (myWidth / 2f + aiWidth / 2f);

        if (Mathf.Abs(distanceX) < collisionThreshold)
        {
            if (distanceX > 0) // Előtted lévő AI
            {
                // 1. A sebességedet az AI-hoz igazítjuk (lassulsz)
                speed = Mathf.Lerp(speed, script.aiSpeed * 0.7f, Time.deltaTime * 10f);
                
                // 2. Az AI-t toljuk el előre a világban, hogy ne legyetek egymásban
                float overlap = collisionThreshold - distanceX;
                script.aiWorldDistance += overlap + 0.05f; 
            }
            else // Mögötted lévő AI
            {
                // 1. Kapsz egy kis sebességlöketet
                speed = Mathf.MoveTowards(speed, speed + (pushForce / 50f), 2f);
                
                // 2. Az AI-t toljuk hátra a világban (ő lassul le hozzád)
                float overlap = collisionThreshold - Mathf.Abs(distanceX);
                script.aiWorldDistance -= overlap + 0.05f;
                script.aiSpeed = Mathf.Lerp(script.aiSpeed, speed * 0.8f, Time.deltaTime * 5f);
            }
        }
    }
}
    
}