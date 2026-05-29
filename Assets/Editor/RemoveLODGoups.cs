using UnityEngine;
using UnityEditor;

public class RemoveLODGroups : EditorWindow
{
    [MenuItem("Tools/Remove LOD")]
    public static void ShowWindow()
    {
        GetWindow<RemoveLODGroups>("Remove LOD");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Remove LODs From Selected"))
        {
            RemoveLODs();
        }
    }

    private void RemoveLODs()
    {
        GameObject[] objs = Selection.gameObjects;

        foreach (GameObject obj in objs)
        {
            // Gets LODGroups on this object AND all children
            LODGroup[] groups = obj.GetComponentsInChildren<LODGroup>(true);

            foreach (LODGroup group in groups)
            {
                Debug.Log("Removed LODGroup from: " + group.gameObject.name);

                DestroyImmediate(group);
            }
        }
    }
}
