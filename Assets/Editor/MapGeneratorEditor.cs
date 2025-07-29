using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class NewMonoBehaviourScript : Editor
{
    public override void OnInspectorGUI()
    {
         MapGenerator mapGen = (MapGenerator)target;

        if (DrawDefaultInspector())
        {
            if(mapGen.autoUpdate)
            {
                mapGen.GenerateMap();
            }
        } 
        
        if(GUILayout.Button ("Generate"))
        {
            mapGen.GenerateMap();
        }
    }
}
