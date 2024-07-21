using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateMeshData(float[,] noiseMap, int levelOfDetail, List<TerrainType> regions,
        AnimationCurve _regionHeightCurve, int depth)
    {
        AnimationCurve regionHeightCurve = new AnimationCurve(_regionHeightCurve.keys);

        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int levelOfDetailSimplicationIndex = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int vertPerLine = (width - 1) / levelOfDetailSimplicationIndex + 1;

        Vector3[] vertices = new Vector3[vertPerLine * vertPerLine];
        int[] triangles = new int[(vertPerLine - 1) * (vertPerLine - 1) * 6];
        Vector2[] uvs = new Vector2[vertPerLine * vertPerLine];

        int vertIndex = 0;
        int triIndex = 0;

        for (int z = 0; z < height; z += levelOfDetailSimplicationIndex)
        {
            for (int x = 0; x < width; x += levelOfDetailSimplicationIndex)
            {
                float finalElevation = noiseMap[x, z];
                if (regions != null && regions.Count > 0)
                {
                    finalElevation = regionHeightCurve.Evaluate(finalElevation);
                }
                vertices[vertIndex] = new Vector3(x + topLeftX, finalElevation * depth, topLeftZ - z);
                uvs[vertIndex] = new Vector2(x / (float)width, z / (float)height);

                if (x < width - 1 && z < height - 1)
                {
                    triangles[triIndex] = vertIndex;
                    triangles[triIndex + 1] = vertIndex + vertPerLine + 1;
                    triangles[triIndex + 2] = vertIndex + vertPerLine;

                    triangles[triIndex + 3] = vertIndex + vertPerLine + 1;
                    triangles[triIndex + 4] = vertIndex;
                    triangles[triIndex + 5] = vertIndex + 1;

                    triIndex += 6;
                }
                vertIndex++;
            }
        }
        return new MeshData(vertices, triangles, uvs);
    }

    public static void CreateMesh(Mesh mesh, MeshData meshData)
    {
        mesh.Clear();
        mesh.vertices = meshData.vertices;
        mesh.triangles = meshData.triangles;
        mesh.uv = meshData.uvs;
        mesh.RecalculateNormals();
    }
}

public struct MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    public MeshData(Vector3[] vertices, int[] triangles, Vector2[] uvs)
    {
        this.vertices = vertices;
        this.triangles = triangles;
        this.uvs = uvs;
    }
}