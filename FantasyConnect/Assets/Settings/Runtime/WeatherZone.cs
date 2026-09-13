using System.Collections.Generic;
using UnityEngine;

// A box area that requests a Stylized Weather preset while the camera is inside it.
// No physics: the switcher just tests the camera position against the registered boxes.
public class WeatherZone : MonoBehaviour {
    public static readonly List<WeatherZone> All = new List<WeatherZone>();

    [Tooltip("Name of the weather preset under the controller's Weather container")]
    public string weather = "Calm";
    public Vector3 size = new Vector3(100f, 400f, 100f);
    [Tooltip("Higher wins where zones overlap")]
    public int priority;

    void OnEnable () { All.Add(this); }
    void OnDisable () { All.Remove(this); }

    public bool Contains (Vector3 worldPoint) {
        var local = transform.InverseTransformPoint(worldPoint);
        var half = size * 0.5f;
        return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z;
    }

#if UNITY_EDITOR
    void OnDrawGizmos () {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.08f);
        Gizmos.DrawCube(Vector3.zero, size);
        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.8f);
        Gizmos.DrawWireCube(Vector3.zero, size);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 5f, "☁ " + weather);
    }
#endif
}
