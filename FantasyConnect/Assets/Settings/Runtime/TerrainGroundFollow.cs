using UnityEngine;

// Keeps a walking creature on the terrain surface. Root-motion walks and recorded Timeline paths were authored on flat
// ground; this runs after animation and moves only the height, so the creature follows hills instead of cutting through.
[DefaultExecutionOrder(1000)]
public class TerrainGroundFollow : MonoBehaviour {
    [Tooltip("Height above the terrain surface (pivot at the feet = 0)")]
    public float heightOffset;
    [Tooltip("How quickly the height catches up with the ground (0 = snap)")]
    public float smoothing = 15f;

    bool initialized;

    void OnEnable () {
        initialized = false;
    }

    void LateUpdate () {
        var p = transform.position;
        float ground;
        if (!TryGround(p, out ground)) return;
        float target = ground + heightOffset;
        // snap on the first frame so a freshly activated creature does not glide up from underground
        if (!initialized || smoothing <= 0f) {
            p.y = target;
            initialized = true;
        } else {
            p.y = Mathf.Lerp(p.y, target, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
        }
        transform.position = p;
    }

    static bool TryGround (Vector3 p, out float ground) {
        foreach (var t in Terrain.activeTerrains) {
            var o = t.transform.position;
            var size = t.terrainData.size;
            if (p.x < o.x || p.x > o.x + size.x || p.z < o.z || p.z > o.z + size.z) continue;
            ground = t.SampleHeight(p) + o.y;
            return true;
        }
        ground = 0f;
        return false;
    }
}
