using System.Collections.Generic;
using UnityEngine;
using UnluckSoftware;

// Changes the weather when the camera moves into a different WeatherZone.
// The current zone is kept while the camera is still inside it, so overlapping borders do not flicker.
public class WeatherZoneSwitcher : MonoBehaviour {
    public StylizedWeatherController controller;
    public float checkInterval = 0.5f;
    [Tooltip("Seconds for particles of the previous weather to die out after a change")]
    public float fadeOutTime = 1.5f;

    WeatherZone current;
    float nextCheck;
    Camera cachedCamera;
    readonly HashSet<ParticleSystem> used = new HashSet<ParticleSystem>();
    ParticleSystem.Particle[] buffer = new ParticleSystem.Particle[256];

    void Update () {
        if (controller == null || Time.time < nextCheck) return;
        nextCheck = Time.time + checkInterval;
        if (cachedCamera == null || !cachedCamera.isActiveAndEnabled) {
            cachedCamera = Camera.main;
            // cutscene/title cameras are often not tagged MainCamera
            if (cachedCamera == null && Camera.allCamerasCount > 0) cachedCamera = Camera.allCameras[0];
        }
        if (cachedCamera == null) return;

        var p = cachedCamera.transform.position;
        if (current != null && current.isActiveAndEnabled && current.Contains(p)) return;

        WeatherZone best = null;
        foreach (var zone in WeatherZone.All)
            if (zone.Contains(p) && (best == null || zone.priority > best.priority)) best = zone;
        if (best == null || (current != null && best.weather == current.weather)) { if (best != null) current = best; return; }

        current = best;
        controller.ChangeWeather(best.weather);
        FadeOutUnused(best.weather);
    }

    // snow flakes live for several seconds; without this they keep falling over the next area
    void FadeOutUnused (string weather) {
        used.Clear();
        foreach (var ws in controller.weatherSettings) {
            if (ws == null || ws.name != weather) continue;
            foreach (var e in ws.elements)
                if (e.particleSystem != null && e.emission.constantMax > 0f) used.Add(e.particleSystem);
        }
        if (controller.allParticles == null) return;
        foreach (var ps in controller.allParticles) {
            if (ps == null || !ps.gameObject.activeInHierarchy || used.Contains(ps) || ps.particleCount == 0) continue;
            // reuse one buffer instead of allocating per system
            if (buffer.Length < ps.particleCount) buffer = new ParticleSystem.Particle[Mathf.NextPowerOfTwo(ps.particleCount)];
            int n = ps.GetParticles(buffer);
            for (int i = 0; i < n; i++) buffer[i].remainingLifetime = Mathf.Min(buffer[i].remainingLifetime, Random.Range(fadeOutTime * 0.3f, fadeOutTime));
            ps.SetParticles(buffer, n);
        }
    }
}
