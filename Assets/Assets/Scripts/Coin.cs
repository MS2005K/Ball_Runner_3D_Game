using UnityEngine;

public class Coin : MonoBehaviour
{
    float baseY, phase;
    void Start() { baseY = transform.position.y; phase = Random.value * 6.28f; }
    void Update()
    {
        transform.Rotate(0f, 220f * Time.deltaTime, 0f, Space.World);
        var p = transform.position;
        p.y = baseY + Mathf.Sin(Time.time * 3f + phase) * 0.12f;
        transform.position = p;
    }
}
