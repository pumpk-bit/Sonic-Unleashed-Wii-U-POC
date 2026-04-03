using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AddLODGroups : EditorWindow
{
    float lod0 = 0.6f;
    float lod1 = 0.3f;
    float lod2 = 0.1f;

    [MenuItem("Tools/Add LOD Groups")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AddLODGroups));
    }

    void OnGUI()
    {
        GUILayout.Label("LOD Transition Values", EditorStyles.boldLabel);

        lod0 = EditorGUILayout.Slider("LOD0 → LOD1", lod0, 0.0f, 1.0f);
        lod1 = EditorGUILayout.Slider("LOD1 → LOD2", lod1, 0.0f, 1.0f);
        lod2 = EditorGUILayout.Slider("LOD2 → Culled", lod2, 0.0f, 1.0f);

        if (GUILayout.Button("Add LODs To Selected"))
        {
            AddLODs();
        }
    }

    void AddLODs()
    {
        GameObject[] objs = Selection.gameObjects;

        foreach (GameObject obj in objs)
        {
            MeshFilter mf = obj.GetComponent<MeshFilter>();
            MeshRenderer mr = obj.GetComponent<MeshRenderer>();

            if (mf == null || mr == null) continue;

            LODGroup group = obj.GetComponent<LODGroup>();
            if (group == null)
                group = obj.AddComponent<LODGroup>();

            // Make LOD levels
            LOD[] lods = new LOD[3];

            // Highest detail
            lods[0] = new LOD(lod0, new Renderer[] { mr });

            // Reuse mesh renderer because Unity 4 can't auto-simplify mesh for LODs
            lods[1] = new LOD(lod1, new Renderer[] { mr });

            // Last LOD (still visible)
            lods[2] = new LOD(lod2, new Renderer[] { mr });

            group.SetLODs(lods);
            group.RecalculateBounds();
        }
    }
}
