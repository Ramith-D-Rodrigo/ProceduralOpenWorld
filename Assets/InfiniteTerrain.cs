using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{

    public const float maxViewDistance = 300f;
    public Transform viewer;

    public static Vector2 viewerPosition;
    int chunkSize;
    int chunksVisibleInViewDistance;

    public Material testMaterial;

    //to keep track of the terrain chunks and prevent duplicates
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> lastVisibleTerrainChunks = new List<TerrainChunk>();

    static ProceduralMeshTerrain meshTerrainGenerator;

    void Start()
    {
        chunkSize = ProceduralMeshTerrain.mapChunkSize - 1; //because the mesh size is 1 less than the map size
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);
        meshTerrainGenerator = GetComponent<ProceduralMeshTerrain>();
    }

    // Update is called once per frame
    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
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
                    if(terrainChunkDictionary[viewedChunkCoord].IsVisible())
                    {
                        lastVisibleTerrainChunks.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                }
                else
                {
                    terrainChunkDictionary.Add(
                        viewedChunkCoord, 
                        new TerrainChunk(viewedChunkCoord, chunkSize, transform, testMaterial)
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

        public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
        {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;

            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;
            SetVisible(false);

            meshTerrainGenerator.RequestMapData(OnMapDataReceived);
        }

        void OnMapDataReceived(float[,] noiseMap)
        {
            meshTerrainGenerator.RequestMeshData(noiseMap, OnMeshDataReceived);
        }

        void OnMeshDataReceived(MeshData meshData)
        {
             MeshGenerator.CreateMesh(meshFilter.mesh, meshData);
        }

        public void UpdateTerrainChunk()
        {
            //smallest distance from the viewer to the bounds's edge
            float nearestDistToViewer = Mathf.Sqrt(bounds.SqrDistance(viewerPosition)); 
            bool isVisible = nearestDistToViewer <= maxViewDistance;
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
}
