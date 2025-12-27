using UnityEngine;

[CreateAssetMenu(fileName = "New Car", menuName = "Car System/Car Data")]
public class CarData : ScriptableObject
{
    public string carName;
    public Sprite carSprite;
    public Vector3 garageScale = Vector3.one;
    public Vector3 gameScale = Vector3.one;

    [Header("Wheel Positions")]
    public float frontWheelX; // Első kerék távolsága a középponttól
    public float backWheelX;  // Hátsó kerék távolsága
    public float wheelsY;     // Kerekek magassága az autóhoz képest
    public float carBodyY;    // Az egész autótest magassága az úthoz képest
}