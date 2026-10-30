using UnityEngine;

/// <summary>
/// Put this on any hurdle object. Touching its red box (X/Z plane) = Game Over.
/// Move the object in the Scene view -> the danger zone moves with it.
/// Change "Half" in the Inspector to resize the danger zone.
/// </summary>
public class Hurdle : MonoBehaviour
{
    [Tooltip("Half size of the danger zone on the X (width) / Z (depth) plane")]
    public Vector2 half = Vector2.one;

    public bool Hits(Vector2 p, float r)
    {
        Vector3 c = transform.position;
        float dx = Mathf.Max(Mathf.Abs(p.x - c.x) - half.x, 0f);
        float dz = Mathf.Max(Mathf.Abs(p.y - c.z) - half.y, 0f);
        return dx * dx + dz * dz < r * r;
    }

    void OnDrawGizmos()
    {
        Vector3 c = transform.position;
        Gizmos.color = new Color(1f, 0.1f, 0.1f, 0.9f);
        Gizmos.DrawWireCube(new Vector3(c.x, 0.75f, c.z), new Vector3(half.x * 2f, 1.5f, half.y * 2f));
    }
}
