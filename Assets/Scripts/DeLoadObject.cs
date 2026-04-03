using UnityEngine;
using System.Collections;

public class DeLoadObject : MonoBehaviour
{
    public enum Mode
    {
        Load,
        Unload
    }

    [SerializeField] Mode mode;

    [SerializeField] GameObject[] objects;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        bool setActive = (mode == Mode.Load);

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
                objects[i].SetActive(setActive);
        }
    }
}