using UnityEngine;

[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private LightingPreset Preset;

    [Header("Time of Day")]
    [SerializeField, Range(0, 24)] private float TimeOfDay;

    // --- REPLACE THE OLD SKYBOX FIELDS WITH THIS ONE ---
    [Header("Skybox Settings")]
    [SerializeField] private Material DynamicSkyMaterial;
    // ---------------------------------------------------

    // Your other fields (Fog, Sun Settings) remain the same
    [Header("Fog Settings")]
    [SerializeField]
    private AnimationCurve FogDensityCurve = new AnimationCurve(
        new Keyframe(0, 0.008f), new Keyframe(0.5f, 0.002f), new Keyframe(1, 0.008f));
    [SerializeField, Range(0, 1)] private float FogTintIntensity = 0.6f;
    [Header("Sun Settings")]
    [SerializeField, Range(0f, 360f)] private float SunAzimuth = 170f;
    [SerializeField] private Color SunriseFogTint = new Color(1f, 0.6f, 0.4f);
    [SerializeField] private Color SunsetFogTint = new Color(1f, 0.5f, 0.3f);
    [SerializeField] private Color NightFogTint = new Color(0.4f, 0.5f, 0.8f);

    private void Update()
    {
        if (Preset == null || DirectionalLight == null)
            return;

        if (Application.isPlaying)
        {
            TimeOfDay += Time.deltaTime;
            TimeOfDay %= 24;
        }

        UpdateLighting(TimeOfDay / 24f);
    }

    private void UpdateLighting(float timePercent)
    {
        // Set scene lighting properties (Ambient, Fog, etc.)
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = FogDensityCurve.Evaluate(timePercent);
        // ... (The rest of your existing fog and tinting logic)

        // --- ADD THIS BLOCK TO ANIMATE THE SKYBOX ---
        if (DynamicSkyMaterial != null)
        {
            // Set the material properties using the preset gradients
            DynamicSkyMaterial.SetColor("_SkyGradientTop", Preset.SkyColorTop.Evaluate(timePercent));
            DynamicSkyMaterial.SetColor("_SkyGradientBottom", Preset.SkyColorBottom.Evaluate(timePercent));
            DynamicSkyMaterial.SetColor("_HorizonLineColor", Preset.HorizonColor.Evaluate(timePercent));

            // Assign the updated material to the scene's skybox
            RenderSettings.skybox = DynamicSkyMaterial;
        }
        // ---------------------------------------------

        // Directional Light
        if (DirectionalLight != null)
        {
            DirectionalLight.color = Preset.DirectionalColor.Evaluate(timePercent);
            float sunAngleX = (timePercent * 360f) - 90f;
            DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3(sunAngleX, SunAzimuth, 0));
        }
    }

    // OnValidate remains the same
    private void OnValidate()
    {
        if (DirectionalLight != null) return;
        if (RenderSettings.sun != null) DirectionalLight = RenderSettings.sun;
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