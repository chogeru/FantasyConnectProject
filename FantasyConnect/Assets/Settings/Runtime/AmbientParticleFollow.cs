using UnityEngine;

// Keeps an ambient particle emitter centred on the active camera; particles simulate in world space.
[ExecuteAlways]
public class AmbientParticleFollow : MonoBehaviour {
    public Vector3 offset = Vector3.zero;
    [Tooltip("Pushes the emitter ahead of the camera (horizontal forward) so more particles land in view.")]
    public float forwardOffset = 0f;

    Camera cachedCamera;

    void LateUpdate () {
        if (cachedCamera == null || !cachedCamera.isActiveAndEnabled) {
            cachedCamera = Camera.main;
            if (cachedCamera == null && Camera.allCamerasCount > 0) cachedCamera = Camera.allCameras[0];
        }
        if (cachedCamera == null) return;
        var t = cachedCamera.transform;
        var forward = t.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude > 0.0001f) forward.Normalize();
        transform.position = t.position + offset + forward * forwardOffset;
    }
}
