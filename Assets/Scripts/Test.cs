using UnityEngine;

public class Test : MonoBehaviour
{
    public GameObject plane;
    public float speed = 5.0f;
    
    // Bounds for resetting
    public float topThreshold = 10.0f;
    public float bottomThreshold = -10.0f;
    public float leftThreshold = -10.0f;
    public float rightThreshold = 10.0f;

    void Update()
    {
        // 1. Move diagonally down and right (normalized keeps speed consistent)
        Vector3 direction = (Vector3.down + (2 * Vector3.right)).normalized;
        plane.transform.Translate(direction * speed * Time.deltaTime);

        // 2. Check if object has passed BOTH the bottom AND right boundaries
        if (plane.transform.position.y <= bottomThreshold || plane.transform.position.x >= rightThreshold)
        {
            // 3. Teleport back to top-left corner
            Vector3 newPos = transform.position;
            newPos.y = topThreshold;
            newPos.x = leftThreshold;
            plane.transform.position = newPos;
        }
    }
}