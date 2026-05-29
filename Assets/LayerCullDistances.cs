using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerCullDistances : MonoBehaviour {

    [SerializeField] float SmallProps;
    [SerializeField] float MediumDetail;
    [SerializeField] float LargeBuildings;
    [SerializeField] float Foliage;
    [SerializeField] float GameObjects;

    void Start()
    {
        Camera cam = GetComponent<Camera>();

        // There are 32 possible layers in Unity.
        float[] distances = new float[32];

        // Assign distances (in meters) to your specific layers.
        // Replace '10' and '11' with your actual layer indices.
        distances[10] = SmallProps;  // SmallProps disappear at 15m
        distances[11] = MediumDetail;  // MediumDetail disappears at 50m
        distances[12] = LargeBuildings; // LargeBuildings disappear at 200m
        distances[13] = Foliage; // Foliage disappear at 200m
        distances[15] = GameObjects; 

        // Any layer set to 0 will use the Camera's default 'Far Clip Plane'.
        cam.layerCullDistances = distances;
    }

    void Update()
    {
#if UNITY_EDITOR
        Camera cam = GetComponent<Camera>();

        // There are 32 possible layers in Unity.
        float[] distances = new float[32];

        // Assign distances (in meters) to your specific layers.
        // Replace '10' and '11' with your actual layer indices.
        distances[10] = SmallProps;  // SmallProps disappear at 15m
        distances[11] = MediumDetail;  // MediumDetail disappears at 50m
        distances[12] = LargeBuildings; // LargeBuildings disappear at 200m
        distances[13] = Foliage; // Foliage disappear at 200m
        distances[15] = GameObjects; // Foliage disappear at 200m

        // Any layer set to 0 will use the Camera's default 'Far Clip Plane'.
        cam.layerCullDistances = distances;
#endif
    }

       
}
