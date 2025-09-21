using UnityEngine;

public class PlayerControllerSideView : MonoBehaviour
{
    public float[] laneYPositions; // pl.: [-2.5f, 0f, 2.5f]
    public int currentLane = 1;
    public float laneMoveSpeed = 10f;

    public float speed = 0f;          // sebesség
    public float maxSpeed = 600f;
    public float acceleration = 50f;
    public float backwardSpeed = 100f;

    void Update()
    {
        // Lane váltás
        if (Input.GetKeyDown(KeyCode.W) && currentLane < laneYPositions.Length - 1)
            currentLane++;
        if (Input.GetKeyDown(KeyCode.S) && currentLane > 0)
            currentLane--;

        // Lane pozíció Y
        float targetY = laneYPositions[currentLane];
        Vector3 targetPos = transform.localPosition;
        targetPos.y = Mathf.Lerp(transform.localPosition.y, targetY, Time.deltaTime * laneMoveSpeed);

        // Sebesség változtatás (előre/hátra)
        if (Input.GetKey(KeyCode.D))
        {
            speed += acceleration * Time.deltaTime;
            if (speed > maxSpeed) speed = maxSpeed;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            speed -= acceleration * Time.deltaTime;
            if (speed < -backwardSpeed) speed = -backwardSpeed;
        }
        else
        {
            speed = Mathf.MoveTowards(speed, 0, acceleration * Time.deltaTime);
        }

        // **X pozíció FIX**, ne mozogjon a player a képernyőn
        targetPos.x = 0;

        transform.localPosition = targetPos;
    }
}
