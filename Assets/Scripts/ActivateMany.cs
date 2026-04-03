using UnityEngine;

public class ActivateMany : MonoBehaviour
{
    public static void SetHierarchyActive(Transform root, bool state)
    {

        root.gameObject.SetActive(state);

        for (int i = 0; i < root.childCount; i++)
        {

            SetHierarchyActive(root.GetChild(i), state);
        }
    }
}
