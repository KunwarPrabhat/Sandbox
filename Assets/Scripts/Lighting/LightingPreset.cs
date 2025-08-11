using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Lighting Preset", menuName = "Scriptable Objects/Lighting Preset", order = 1)]
public class LightingPreset : ScriptableObject
{
    public Gradient AmbientColor;
    public Gradient DirectionalColor;
    public Gradient FogColor;

    [Header("Skybox Colors")]
    public Gradient SkyColorTop;
    public Gradient SkyColorBottom;
    public Gradient HorizonColor;
}
