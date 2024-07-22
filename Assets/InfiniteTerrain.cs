using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
    public float scale = 1f;

    public static float maxViewDistance;
    public LODInfo[] detailLevels;

    public Transform viewer;
    public static Vector2 viewerPosition;
    Vector2 prevViewerPosition;
    const float viewerPositionOffsetToUpdateChunks = 25f;
    float sqrViewerPositionOffsetToUpdateChunks = Mathf.Pow(viewerPositionOffsetToUpdateChunks, 2);

    int chunkSize;
    int chunksVisibleInViewDistance;

    public Material testMaterial;

    //to keep track of the terrain chunks and prevent duplicates
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> lastVisibleTerrainChunks = new List<TerrainChunk>();

    static ProceduralMeshTerrain meshTerrainGenerator;

    void Start()
    {
        meshTerrainGenerator = GetComponent<ProceduralMeshTerrain>();
        if (!meshTerrainGenerator.useThreading)
        {
            return;
        }

        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistThreshold;

        chunkSize = ProceduralMeshTerrain.mapChunkSize - 1; //because the mesh size is 1 less than the map size
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);

        prevViewerPosition = viewerPosition;

        UpdateVisibleChunks();
    }

    // Update is called once per frame
    void Update()
    {
        if (!meshTerrainGenerator.useThreading)
        {
            return;
        }

        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;
        float viewerPositionOffset = (prevViewerPosition - viewerPosition).sqrMagnitude;
        if(viewerPositionOffset > sqrViewerPositionOffsetToUpdateChunks)
        {
            prevViewerPosition = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks()
    {
        for(int i = 0; i < lastVisibleTerrainChunks.Count; i++)
        {
            lastVisibleTerrainChunks[i].SetVisible(false);
        }
        lastVisibleTerrainChunks.Clear();

        int currChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currChunkCoordX + xOffset, currChunkCoordY + yOffset);

                if(terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    terrainChunkDictionary.Add(
                        viewedChunkCoord, 
                        new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, testMaterial, scale)
                        );
                }
            }
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        LODMesh collisionLODMesh;

        float[,] noiseMap;
        bool hasReceivedMapData;

        int previousLODIndex = -1;

        public TerrainChunk(Vector2 coord, int size,LODInfo[] LODDetails, Transform parent, Material material, float scale)
        {
            detailLevels = LODDetails;
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.material = material;

            meshObject.transform.position = positionV3 * scale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * scale;
            SetVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for(int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].levelOfDetail, UpdateTerrainChunk);
                if(detailLevels[i].useForCollider)
                {
                    collisionLODMesh = lodMeshes[i];
                }
            }

            meshTerrainGenerator.RequestMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(float[,] noiseMap)
        {
            this.noiseMap = noiseMap;
            hasReceivedMapData = true;
            //create the texture for noise map
            int size = ProceduralMeshTerrain.mapChunkSize + 2;
            Texture2D noiseMapTexture = new Texture2D(size, size);
            Color[] colors = TextureGenerator.CreateColorMap(size, size, 
                noiseMap, Color.black, Color.white);
            noiseMapTexture.SetPixels(colors);
            noiseMapTexture.Apply();

            //then update the shader graph 
            meshRenderer.material.SetTexture("_HeightMap", noiseMapTexture);

            //region heights
            for (int i = 0; i < meshTerrainGenerator.regions.Count; i++)
            {
                meshRenderer.material.SetFloat("_" + meshTerrainGenerator.regions[i].regionName + "Height", 
                    meshTerrainGenerator.regions[i].height);
            }

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            if (!hasReceivedMapData)
            {
                return;
            }

            //smallest distance from the viewer to the bounds's edge
            float nearestDistToViewer = Mathf.Sqrt(bounds.SqrDistance(viewerPosition)); 
            bool isVisible = nearestDistToViewer <= maxViewDistance;

            if (isVisible)
            {
                int lodIndex = 0;
                //not going to check the last level of detail because it is going to be greater than the max view distance
                for (int i = 0; i < detailLevels.Length - 1; i++)
                {
                    //if the distance to the viewer is greater than the visible distance threshold of the current level of detail
                    if (nearestDistToViewer > detailLevels[i].visibleDistThreshold)
                    {
                        lodIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                if (lodIndex != previousLODIndex)
                {
                    LODMesh lodMesh = lodMeshes[lodIndex];
                    if (lodMesh.hasReceivedMesh)
                    {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                    }
                    else if (!lodMesh.hasRequestedMesh)
                    {
                        lodMesh.RequestMeshData(noiseMap);
                    }
                }

                if (lodIndex == 0)  //only add the collider for the lowest level of detail (highest resolution)
                {
                    if (collisionLODMesh.hasReceivedMesh)
                    {
                        meshCollider.sharedMesh = collisionLODMesh.mesh;
                    }
                    else if (!collisionLODMesh.hasRequestedMesh)
                    {
                        collisionLODMesh.RequestMeshData(noiseMap);
                    }
                }
                else
                {
                    //remove the collider if the level of detail is not the highest resolution
                    meshCollider.sharedMesh = null;
                }

                lastVisibleTerrainChunks.Add(this);
            }

            SetVisible(isVisible);
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }

    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasReceivedMesh;
        int levelOfDetail;
        Action updateCallback;
        public LODMesh(int levelOfDetail, Action callback)
        {
            mesh = new Mesh();
            this.levelOfDetail = levelOfDetail;
            updateCallback = callback;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            MeshGenerator.CreateMesh(mesh, meshData);
            hasReceivedMesh = true;
            updateCallback();
        }

        public void RequestMeshData(float[,] noiseMap)
        {
            meshTerrainGenerator.RequestMeshData(noiseMap, levelOfDetail, OnMeshDataReceived);
            hasRequestedMesh = true;
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int levelOfDetail;
        public float visibleDistThreshold; //higher the level of detail, the less the mesh has details
        //hence when visibleDistThreshold is reached, max out the level of detail

        public bool useForCollider;
    }
}
