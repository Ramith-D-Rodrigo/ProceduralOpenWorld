using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateMeshData(float[,] noiseMap, int levelOfDetail, List<TerrainType> regions,
        AnimationCurve _regionHeightCurve, int depth)
    {
        AnimationCurve regionHeightCurve = new AnimationCurve(_regionHeightCurve.keys);

        int levelOfDetailSimplicationIndex = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int borderedSize = noiseMap.GetLength(0);
        int meshSize = borderedSize - 2 * levelOfDetailSimplicationIndex;
        int meshSizeUnsimplified = borderedSize - 2;

        float topLeftX = (meshSizeUnsimplified - 1) / -2f;
        float topLeftZ = (meshSizeUnsimplified - 1) / 2f;


        int vertPerLine = (meshSize - 1) / levelOfDetailSimplicationIndex + 1;

        MeshData meshData = new MeshData(vertPerLine);

        int[,] vertexIndicesMap = new int[borderedSize, borderedSize];

        int meshVertexIndex = 0;
        int borderVertexIndex = -1;

        //filling the vertexIndicesMap to keep track of the vertices and their indices
        for (int z = 0; z < borderedSize; z += levelOfDetailSimplicationIndex)
        {
            for (int x = 0; x < borderedSize; x += levelOfDetailSimplicationIndex)
            {
                bool isBorderVertex = z == 0 || z == borderedSize - 1 || x == 0 || x == borderedSize - 1;
                if (isBorderVertex)
                {
                    vertexIndicesMap[x, z] = borderVertexIndex;
                    borderVertexIndex--;
                }
                else
                {
                    vertexIndicesMap[x, z] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        for (int z = 0; z < borderedSize; z += levelOfDetailSimplicationIndex)
        {
            for (int x = 0; x < borderedSize; x += levelOfDetailSimplicationIndex)
            {
                int vertexIndex = vertexIndicesMap[x, z];
                float finalElevation = noiseMap[x, z];
                if (regions != null && regions.Count > 0)
                {
                    finalElevation = regionHeightCurve.Evaluate(finalElevation);
                }
                Vector2 percent = new Vector2((x - levelOfDetailSimplicationIndex) / (float)meshSize,
                        (z - levelOfDetailSimplicationIndex) / (float)meshSize);
                Vector3 vertexPosition = new Vector3(topLeftX + percent.x * meshSizeUnsimplified, 
                    finalElevation * depth, 
                    topLeftZ - percent.y * meshSizeUnsimplified);

                meshData.AddVertex(vertexPosition, percent, vertexIndex);

                if (x < borderedSize - 1 && z < borderedSize - 1)
                {
                    int a = vertexIndicesMap[x, z];
                    int b = vertexIndicesMap[x + levelOfDetailSimplicationIndex, z];
                    int c = vertexIndicesMap[x, z + levelOfDetailSimplicationIndex];
                    int d = vertexIndicesMap[x + levelOfDetailSimplicationIndex, z + levelOfDetailSimplicationIndex];

                    meshData.AddTriangle(a, d, c);
                    meshData.AddTriangle(d, a, b);
                }
            }
        }

        meshData.BakeNormals();

        return meshData;
    }

    public static void CreateMesh(Mesh mesh, MeshData meshData)
    {
        mesh.Clear();
        mesh.vertices = meshData.vertices;
        mesh.triangles = meshData.triangles;
        mesh.uv = meshData.uvs;
        mesh.normals = meshData.bakedNormals;
    }
}

public struct MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    public Vector3[] bakedNormals;

    Vector3[] borderVertices;
    int[] borderTriangles;

    int triangleIndex;
    int borderTriangleIndex;

    public MeshData(int verticesPerLine)
    {
        this.vertices = new Vector3[verticesPerLine * verticesPerLine];
        this.uvs = new Vector2[verticesPerLine * verticesPerLine];
        this.triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];

        this.borderVertices = new Vector3[verticesPerLine * 4 + 4]; //4 sides and 4 corners
        this.borderTriangles = new int[24 * verticesPerLine]; //4 sides * 6 triangles per side * verticesPerLine

        triangleIndex = 0;
        borderTriangleIndex = 0;

        bakedNormals = null;
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
    {
        if(vertexIndex < 0)
        {
            borderVertices[-vertexIndex - 1] = vertexPosition;
        }
        else
        {
            vertices[vertexIndex] = vertexPosition;
            uvs[vertexIndex] = uv;
        }
    }

    public void AddTriangle(int a, int b, int c)
    {
        if (a < 0 || b < 0 || c < 0)
        {
            borderTriangles[borderTriangleIndex] = a;
            borderTriangles[borderTriangleIndex + 1] = b;
            borderTriangles[borderTriangleIndex + 2] = c;
            borderTriangleIndex += 3;
        }
        else
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }
    }

    public void BakeNormals()
    {
       bakedNormals = CalculateNormals();
    }

    Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertices, vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        int borderTriangleCount = borderTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = borderTriangles[normalTriangleIndex];
            int vertexIndexB = borderTriangles[normalTriangleIndex + 1];
            int vertexIndexC = borderTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertices, vertexIndexA, vertexIndexB, vertexIndexC);
            if(vertexIndexA >= 0)
            {
                vertexNormals[vertexIndexA] += triangleNormal;
            }
            if(vertexIndexB >= 0)
            {
                vertexNormals[vertexIndexB] += triangleNormal;
            }
            if(vertexIndexC >= 0)
            {
                vertexNormals[vertexIndexC] += triangleNormal;
            }
        }

        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(Vector3[] vertices, int indexA, int indexB, int indexC)
    {
        Vector3 pointA = (indexA < 0) ? borderVertices[-indexA - 1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? borderVertices[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? borderVertices[-indexC - 1] : vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;

        return Vector3.Cross(sideAB, sideAC).normalized; //gets the vector that is perpendicular to the two vectors
    }
}