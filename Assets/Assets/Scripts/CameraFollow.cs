using UnityEngine;

/// <summary>Smooth third-person camera behind and above the ball. Never moves the ball.</summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    Vector3 look;

    Vector3 Desired() { return new Vector3(target.position.x * 0.8f, 4.2f, target.position.z - 8f); }
    Vector3 Focus()   { return new Vector3(target.position.x * 0.9f, 0.8f, target.position.z + 5f); }

    public void Snap()
    {
        if (target == null) return;
        transform.position = Desired();
        look = Focus();
        transform.LookAt(look);
    }

    void LateUpdate()
    {
        if (target == null) return;
        float k = 1f - Mathf.Exp(-6f * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, Desired(), k);
        look = Vector3.Lerp(look, Focus(), Mathf.Min(1f, k * 1.5f));
        transform.LookAt(look);
    }
}
