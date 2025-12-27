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
        // Ellenőrizzük, hogy vannak-e autók a listában
        if (aiCarPrefabs == null || aiCarPrefabs.Length == 0) return;

        int randomLane = Random.Range(0, laneYPositions.Length);
        if (IsAreaOccupied(spawnX, laneYPositions[randomLane])) return;

        // VÉLETLENSZERŰ választás a listából:
        int randomCarIndex = Random.Range(0, aiCarPrefabs.Length);
        GameObject newCar = Instantiate(aiCarPrefabs[randomCarIndex]);

        newCar.transform.SetParent(null);

        // Méret fixálás (ahogy korábban kérted)
        RectTransform rt = newCar.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = new Vector2(7f, 2.3f);
        }

        newCar.transform.localScale = new Vector3(1f, 1f, 1f);
        newCar.transform.position = new Vector3(spawnX, laneYPositions[randomLane], 0);

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