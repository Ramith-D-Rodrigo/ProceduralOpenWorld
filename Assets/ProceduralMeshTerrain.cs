using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ProceduralMeshTerrain : MonoBehaviour
{
    const int mapChunkSize = 241;

    [Range(0,6)]
    public int levelOfDetail;
    public int depth = 20;

    public float scale = 20f;
    public float startFrequency = 0.1f;
    public float startAmplitude = 1.0f;
    [Range(0,1)]
    public float gain = 0.5f;
    public float lacunarity = 2.0f;
    public int octaveCount = 4;

    public float xOffSet = 0.0f;
    public float yOffSet = 0.0f;
    public int seed = 203;

    public List<Octave> octaves = new List<Octave>();

    public List<TerrainType> regions;

    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    public AnimationCurve regionHeightCurve;

    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;
    float[,] noiseMap;
    Texture2D noiseMapTexture;

    void Start()
    {
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter.mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        octaves = OctaveGenerator.GenerateOctaves(octaveCount, gain, startAmplitude, startFrequency, lacunarity);
        noiseMap = Noise.CreateNoiseMap(mapChunkSize, mapChunkSize, seed, new Vector2(xOffSet, yOffSet), scale, octaves);
        CreateNoiseMapTexture();       
        CreateVerticesAndTriangles();
        SetShaderGraphVariables();
        CreateMesh();
    }

    private void OnValidate()
    {
        if (octaveCount < 1)
        {
            octaveCount = 1;
        }
    }

    private void CreateVerticesAndTriangles()
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        int levelOfDetailSimplicationIndex = (levelOfDetail == 0) ?  1 : levelOfDetail * 2;
        int vertPerLine = (width - 1) / levelOfDetailSimplicationIndex + 1;

        vertices = new Vector3[vertPerLine * vertPerLine];
        triangles = new int[(vertPerLine - 1) * (vertPerLine - 1) * 6];
        uvs = new Vector2[vertPerLine * vertPerLine];

        int vertIndex = 0;
        int triIndex = 0;

        for (int z = 0; z < height; z+= levelOfDetailSimplicationIndex)
        {
            for (int x = 0; x < width; x+= levelOfDetailSimplicationIndex)
            {
                float finalElevation = noiseMap[x, z];
                if (regions != null && regions.Count > 0)
                {
                    finalElevation = regionHeightCurve.Evaluate(finalElevation);
                }
                vertices[vertIndex] = new Vector3(x, finalElevation * depth, z);
                uvs[vertIndex] = new Vector2(x /(float)width, z /(float)height);

                if(x < width - 1 && z < height - 1)
                {
                    triangles[triIndex] = vertIndex;
                    triangles[triIndex + 1] = vertIndex + vertPerLine;
                    triangles[triIndex + 2] = vertIndex + 1;

                    triangles[triIndex + 3] = vertIndex + 1;
                    triangles[triIndex + 4] = vertIndex + vertPerLine;
                    triangles[triIndex + 5] = vertIndex + vertPerLine + 1;

                    triIndex += 6;
                }
                vertIndex++;
            }
        }
    }

    private void CreateNoiseMapTexture()
    {
        noiseMapTexture = new Texture2D(mapChunkSize, mapChunkSize);
        Color[] colors = TextureGenerator.CreateColorMap(mapChunkSize, mapChunkSize, noiseMap, Color.black, Color.white);
        noiseMapTexture.SetPixels(colors);
        noiseMapTexture.Apply();
    }

    private void SetShaderGraphVariables()
    {
        meshRenderer.sharedMaterial.SetTexture("_HeightMap", noiseMapTexture);

        //region heights
        for(int i = 0; i < regions.Count; i++)
        {
            meshRenderer.sharedMaterial.SetFloat("_" + regions[i].regionName + "Height", regions[i].height);
        }
    }

    private void CreateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
    }
}
