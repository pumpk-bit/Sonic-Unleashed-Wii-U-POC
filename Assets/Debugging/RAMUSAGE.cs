using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RAMUSAGE : MonoBehaviour {

    void Start()
    {
        TextureCalc();
        MeshCalc();
    }

    private void TextureCalc()
    {
        Texture[] textures = Resources.FindObjectsOfTypeAll<Texture>();
        long totalBytes = 0;

        foreach (var tex in textures)
        {
            if (tex == null) continue;

            int width = tex.width;
            int height = tex.height;

            // Very rough fallback estimate: assume 4 bytes per pixel
            long bytes = (long)width * height * 4;

            totalBytes += bytes;

            //Debug.Log(tex.name + " ~ " + (bytes / 1024f / 1024f) + " MB");
        }

        Debug.Log("Estimated texture memory: " + (totalBytes / 1024f / 1024f) + " MB");
    }


    private void MeshCalc()
    {
        MeshFilter[] meshFilters = FindObjectsOfType<MeshFilter>();
        SkinnedMeshRenderer[] skinned = FindObjectsOfType<SkinnedMeshRenderer>();

        long totalBytes = 0;
        HashSet<Mesh> countedMeshes = new HashSet<Mesh>();

        foreach (var mf in meshFilters)
        {
            Mesh mesh = mf.sharedMesh;
            if (mesh == null) continue;
            if (!countedMeshes.Add(mesh)) continue;

            long meshBytes = EstimateMesh(mesh);
            totalBytes += meshBytes;

            //Debug.Log(mesh.name + " ~ " + (meshBytes / 1024f / 1024f) + " MB");
        }

        foreach (var smr in skinned)
        {
            Mesh mesh = smr.sharedMesh;
            if (mesh == null) continue;
            if (!countedMeshes.Add(mesh)) continue;

            long meshBytes = EstimateMesh(mesh);
            totalBytes += meshBytes;

           // Debug.Log(mesh.name + " ~ " + (meshBytes / 1024f / 1024f) + " MB");
        }

        Debug.Log("Estimated UNIQUE mesh memory: " + (totalBytes / 1024f / 1024f) + " MB");
        Debug.Log("Unique meshes counted: " + countedMeshes.Count);
    }

    private long EstimateMesh(Mesh mesh)
    {
        long vertexCount = mesh.vertexCount;

        // Lower and safer rough estimate for static meshes
        long vertexBytes = vertexCount * 32;

        long indexBytes;

        if (mesh.isReadable)
        {
            try
            {
                long indexCount = 0;
                for (int i = 0; i < mesh.subMeshCount; i++)
                    indexCount += mesh.GetIndices(i).Length;

                indexBytes = indexCount * 2;
            }
            catch
            {
                indexBytes = vertexCount * 3;
            }
        }
        else
        {
            indexBytes = vertexCount * 3;
        }

        return vertexBytes + indexBytes;
    }
}

