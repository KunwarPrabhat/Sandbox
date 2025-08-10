using UnityEngine;

[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private LightingPreset Preset;

    [Header("Time of Day")]
    [SerializeField, Range(0, 24)] private float TimeOfDay;

    [Header("Fog Settings")]
    // Switched to ExponentialSquared fog for more natural falloff.
    // This curve now controls density directly. Lower values = less fog.
    [SerializeField]
    private AnimationCurve FogDensityCurve = new AnimationCurve(
        new Keyframe(0, 0.02f), new Keyframe(0.5f, 0.002f), new Keyframe(1, 0.02f));

    // NEW: Control the intensity of the time-based fog tint.
    [SerializeField, Range(0, 1)] private float FogTintIntensity = 0.6f;

    [Header("Sun Settings")]
    // NEW: Added a slider to control the sun's East/West direction (azimuth).
    [SerializeField, Range(0f, 360f)] private float SunAzimuth = 170f;

    // Hardcoded tint colors are fine, but you could also move these to your LightingPreset for more control.
    [SerializeField] private Color SunriseFogTint = new Color(1f, 0.6f, 0.4f); // warm orange
    [SerializeField] private Color SunsetFogTint = new Color(1f, 0.5f, 0.3f); // warm reddish
    [SerializeField] private Color NightFogTint = new Color(0.4f, 0.5f, 0.8f); // cool blue


    private void Update()
    {
        if (Preset == null || DirectionalLight == null)
            return;

        // In Play mode, advance time automatically.
        if (Application.isPlaying)
        {
            TimeOfDay += Time.deltaTime;
            TimeOfDay %= 24; // Loop day cycle
        }

        // Update lighting in both Play mode and the Editor.
        UpdateLighting(TimeOfDay / 24f);
    }

    private void UpdateLighting(float timePercent)
    {
        // Set scene lighting properties
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fog = true;

        // --- FOG IMPROVEMENT ---
        // Using ExponentialSquared is often more visually appealing than Linear fog.
        // It's controlled by a single 'density' value, which simplifies the logic.
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = FogDensityCurve.Evaluate(timePercent);

        // Tint the fog color based on the time of day
        Color baseFogColor = Preset.FogColor.Evaluate(timePercent);
        Color tintedFogColor = baseFogColor; // Start with the preset color

        // These time thresholds determine when to apply tints.
        // 0.25 = sunrise, 0.5 = noon, 0.75 = sunset
        if (timePercent > 0.2f && timePercent < 0.3f) // Sunrise window
            tintedFogColor = Color.Lerp(baseFogColor, SunriseFogTint, FogTintIntensity);
        else if (timePercent > 0.7f && timePercent < 0.8f) // Sunset window
            tintedFogColor = Color.Lerp(baseFogColor, SunsetFogTint, FogTintIntensity);
        else if (timePercent >= 0.8f || timePercent <= 0.2f) // Night
            tintedFogColor = Color.Lerp(baseFogColor, NightFogTint, FogTintIntensity);

        RenderSettings.fogColor = tintedFogColor;

        // --- SUN ROTATION IMPROVEMENT ---
        // The sun's rotation now uses the adjustable 'SunAzimuth' property.
        DirectionalLight.color = Preset.DirectionalColor.Evaluate(timePercent);
        float sunAngleX = (timePercent * 360f) - 90f; // Tilts sun up and down
        DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3(sunAngleX, SunAzimuth, 0));
    }

    // OnValidate is a helper to automatically find the main sun light in the scene.
    private void OnValidate()
    {
        if (DirectionalLight != null)
            return;

        if (RenderSettings.sun != null)
        {
            DirectionalLight = RenderSettings.sun;
        }
        else
        {
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    DirectionalLight = light;
                    return;
                }
            }
        }
    }
}