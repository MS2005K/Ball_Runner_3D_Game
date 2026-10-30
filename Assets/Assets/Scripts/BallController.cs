using UnityEngine;

/// <summary>
/// Manual ball control. The ball NEVER moves on its own - only while a key is held.
/// Smooth acceleration / deceleration and realistic rolling rotation.
/// </summary>
public class BallController : MonoBehaviour
{
    public const float Radius = 0.5f;
    const float MaxSpeed = 9f;
    const float Accel = 32f;
    const float Decel = 22f;
    public const float MinX = -4.7f, MaxX = 4.7f, MinZ = -3f, MaxZ = 200f;

    public Transform visual;
    Vector3 vel;

    Vector3 startPos;

    void Awake() { startPos = transform.position; }

    /// <summary>Used by Restart: ball back to the start, stopped and visible.</summary>
    public void ResetBall()
    {
        transform.position = startPos;
        vel = Vector3.zero;
        visual.localRotation = Quaternion.identity;
        visual.gameObject.SetActive(true);
    }

    public void Tick(float dt)
    {
        Vector2 inp = GameInput.Move();
        if (inp.sqrMagnitude > 1f) inp.Normalize();

        Vector3 target = new Vector3(inp.x, 0f, inp.y) * MaxSpeed;
        float rate = inp.sqrMagnitude > 0.001f ? Accel : Decel;
        vel = Vector3.MoveTowards(vel, target, rate * dt);

        Vector3 p = transform.position + vel * dt;
        if (p.x < MinX) { p.x = MinX; vel.x = 0f; }
        if (p.x > MaxX) { p.x = MaxX; vel.x = 0f; }
        if (p.z < MinZ) { p.z = MinZ; vel.z = 0f; }
        if (p.z > MaxZ) { p.z = MaxZ; vel.z = 0f; }

        float dist = (p - transform.position).magnitude;
        if (dist > 1e-5f)
        {
            Vector3 dir = (p - transform.position) / dist;
            Vector3 axis = Vector3.Cross(Vector3.up, dir);   // rolling axis
            visual.Rotate(axis, dist / Radius * Mathf.Rad2Deg, Space.World);
        }
        transform.position = p;
    }
}
