using UnityEngine;

/// <summary>Small particle-like burst made of tiny cubes (coin pickup / crash effect).</summary>
public class CoinBurst : MonoBehaviour
{
    Transform[] pieces; Vector3[] vel; float life; const float Duration = 0.6f;

    public static void Spawn(Vector3 pos, Color color, int count)
    {
        var go = new GameObject("Burst");
        go.transform.position = pos;
        var b = go.AddComponent<CoinBurst>();
        b.pieces = new Transform[count]; b.vel = new Vector3[count];
        var mat = Mats.Make(color, 0.5f, 0f, 0.6f);
        for (int i = 0; i < count; i++)
        {
            var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(c.GetComponent<Collider>());
            c.GetComponent<Renderer>().sharedMaterial = mat;
            c.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            c.transform.SetParent(go.transform, false);
            c.transform.localScale = Vector3.one * 0.18f;
            b.pieces[i] = c.transform;
            b.vel[i] = (Random.onUnitSphere + Vector3.up * 0.6f) * Random.Range(2f, 4.5f);
        }
    }

    void Update()
    {
        life += Time.deltaTime;
        float k = 1f - life / Duration;
        if (k <= 0f) { Destroy(gameObject); return; }
        for (int i = 0; i < pieces.Length; i++)
        {
            vel[i] += Vector3.down * 6f * Time.deltaTime;
            pieces[i].position += vel[i] * Time.deltaTime;
            pieces[i].localScale = Vector3.one * 0.18f * k;
            pieces[i].Rotate(300f * Time.deltaTime, 200f * Time.deltaTime, 0);
        }
    }
}
