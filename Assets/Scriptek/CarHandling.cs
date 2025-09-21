using UnityEngine;

public class CarHandling : MonoBehaviour
{
    [Header("Lane Movement")]
    public float laneOffset = 2f;  // távolság a sávok között
    public float laneChangeSpeed = 10f;
    public int currentLane = 1;  // 0,1,2,3 -> 4 sávos

    private Vector3 targetPosition;

    [Header("Speed Settings")]
    public float baseAcceleration = 5f;
    public float currentSpeed = 0f;
    public float maxSpeed = 100f;

    void Start()
    {
        SetTargetLane(currentLane);
    }

    void Update()
    {
        HandleInput();
        MoveToLane();
        Accelerate();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.A) && currentLane > 0)
        {
            currentLane--;
            SetTargetLane(currentLane);
        }
        else if (Input.GetKeyDown(KeyCode.D) && currentLane < 3)
        {
            currentLane++;
            SetTargetLane(currentLane);
        }
    }

    void SetTargetLane(int lane)
    {
        targetPosition = new Vector3((lane - 1.5f) * laneOffset, transform.position.y, transform.position.z);
    }

    void MoveToLane()
    {
        Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, laneChangeSpeed * Time.deltaTime);
        transform.position = newPosition;
    }

    void Accelerate()
    {
        currentSpeed += baseAcceleration * Time.deltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
    }
}
