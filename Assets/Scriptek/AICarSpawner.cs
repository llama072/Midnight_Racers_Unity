using UnityEngine;

public class AICarSpawner : MonoBehaviour
{
    [Header("AI Autók Listája")]
    public GameObject[] aiCarPrefabs; // Itt állítsd be a méretet 4-re az Inspectorban!

    [Header("Spawn Beállítások")]
    public float minSpawnTime = 1.0f;
    public float maxSpawnTime = 2.5f;
    public float spawnX = 25f;

    public float[] laneYPositions = { -3.5f, -2.5f, -1.4f, -0.4f };
    private float nextSpawnTime;

    void Start()
    {
        nextSpawnTime = Time.time + Random.Range(minSpawnTime, maxSpawnTime);
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnAICar();
            nextSpawnTime = Time.time + Random.Range(minSpawnTime, maxSpawnTime);
        }
    }

    void SpawnAICar()
    {
        if (aiCarPrefabs == null || aiCarPrefabs.Length == 0) return;

        int randomLane = Random.Range(0, laneYPositions.Length);
        if (IsAreaOccupied(spawnX, laneYPositions[randomLane])) return;

        int randomCarIndex = Random.Range(0, aiCarPrefabs.Length);

        // 1. LÉPÉS: A Spawner (this.transform) alá hozzuk létre az autót!
        GameObject newCar = Instantiate(aiCarPrefabs[randomCarIndex], this.transform);

        // 2. LÉPÉS: TÖRÖLD VAGY KOMMENTELD KI ezt a sort:
        // newCar.transform.SetParent(null); 

        RectTransform rt = newCar.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = new Vector2(7f, 2.3f);

            // 3. LÉPÉS: UI esetén localPosition-t érdemes használni a szülőn belül
            rt.localPosition = new Vector3(spawnX, laneYPositions[randomLane], 0);
        }
        else
        {
            // Ha nem UI, akkor sima localPosition
            newCar.transform.localPosition = new Vector3(spawnX, laneYPositions[randomLane], 0);
        }

        newCar.transform.localScale = new Vector3(1f, 1f, 1f);

        AICar ai = newCar.GetComponent<AICar>();
        if (ai != null)
        {
            ai.currentLane = randomLane;
            ai.aiWorldDistance = WorldProgress.playerDistance + spawnX;
        }
    }

    bool IsAreaOccupied(float x, float y)
    {
        float newCarWorldX = WorldProgress.playerDistance + x;
        GameObject[] cars = GameObject.FindGameObjectsWithTag("AICar");

        foreach (GameObject car in cars)
        {
            if (car == null) continue;
            AICar script = car.GetComponent<AICar>();
            if (script == null) continue;

            if (Mathf.Abs(car.transform.position.y - y) < 0.5f)
            {
                if (Mathf.Abs(script.aiWorldDistance - newCarWorldX) < 8f)
                {
                    return true;
                }
            }
        }
        return false;
    }
}