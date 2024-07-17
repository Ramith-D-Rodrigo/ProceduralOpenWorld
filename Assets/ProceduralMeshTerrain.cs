using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralMeshTerrain : MonoBehaviour
{
    // Start is called before the first frame update

    public int depth = 20;
    public int width = 256;
    public int height = 256;

    public float scale = 20f;

    public float startFrequency = 0.1f;
    public float startAmplitude = 1.0f;
    public float gain = 0.5f;
    public float lacunarity = 2.0f;
    public int octaveCount = 4;
    public float fudgeFactor = 0.1f;

    public float powerValue = 0.1f;

    public float xOffSet = 0.0f;
    public float yOffSet = 0.0f;

    public int seed = 203;

    public List<Octave> octaves = new List<Octave>();

    public List<TerrainType> regions;

    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    Vector3[] vertices;
    int[] triangles;

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
        CreateVerticesAndTriangles();
        CreateMesh();
    }

    private void CreateVerticesAndTriangles()
    {
        Vector2[] randomOffsets = Noise.GenerateRandomOffsets(seed, octaves, xOffSet, yOffSet);

        vertices = new Vector3[(width + 1) * (height + 1)];
        for (int i = 0, z = 0; z <= height; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                float finalHeight = Noise.CalculateHeight(x, z, randomOffsets, octaves, width, 
                    height, scale, fudgeFactor, powerValue);
                vertices[i] = new Vector3(x, finalHeight * depth, z);
                i++;
            }
        }

        triangles = new int[width * height * 6];
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + width + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + width + 1;
                triangles[tris + 5] = vert + width + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    private void CreateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
