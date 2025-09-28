using UnityEngine;

public class CircularMovement : MonoBehaviour
{
    [Header("Circle Settings")]
    public float radius = 2f;        // Radius of the circle
    public float speed = 1f;         // Speed of rotation (in radians/sec)

    private Vector3 centerPoint;     // Center of the circle
    private float angle;             // Current angle

    void Start()
    {
        // Save the starting position as the circle center
        centerPoint = transform.position;
    }

    void Update()
    {
        // Increase angle over time
        angle += speed * Time.deltaTime;

        // Calculate new x,y position
        float x = Mathf.Cos(angle) * radius;
        float y = Mathf.Sin(angle) * radius;

        // Apply offset to keep circle around the centerPoint
        transform.position = centerPoint + new Vector3(x, y, 0f);
    }
}
