using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;
using UnityEditor.Rendering;

public class InfiniteTerrain : MonoBehaviour
{
    public float scale = 1f;

    public static float maxViewDistance;
    public LODInfo[] detailLevels;

    public Transform player;
    public Transform freeView;
    public HUDOptions hUDOptions;

    Transform currentViewTransform;

    public static Vector2 viewerPosition;
    Vector2 prevViewerPosition;
    const float viewerPositionOffsetToUpdateChunks = 25f;
    float sqrViewerPositionOffsetToUpdateChunks = Mathf.Pow(viewerPositionOffsetToUpdateChunks, 2);

    public GameObject waterPrefab;

    public int chunkSize;
    int chunksVisibleInViewDistance;

    public Material testMaterial;

    //to keep track of the terrain chunks and prevent duplicates
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> lastVisibleTerrainChunks = new List<TerrainChunk>();

    static ProceduralMeshTerrain meshTerrainGenerator;

    public bool isAllInitalized = false;
    static NPCInitiator npcInitiator;

    void Start()
    {
        npcInitiator = GetComponent<NPCInitiator>();
        meshTerrainGenerator = GetComponent<ProceduralMeshTerrain>();
        if (!meshTerrainGenerator.useThreading)
        {
            return;
        }

        SwitchView();

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

        //ProcessUserInput();

        viewerPosition = new Vector2(currentViewTransform.position.x, currentViewTransform.position.z) / scale;
        float viewerPositionOffset = (prevViewerPosition - viewerPosition).sqrMagnitude;
        if(viewerPositionOffset > sqrViewerPositionOffsetToUpdateChunks)
        {
            prevViewerPosition = viewerPosition;
            UpdateVisibleChunks();
        }

        bool tempIsAllInitalized = true;
        foreach(TerrainChunk chunk in lastVisibleTerrainChunks)
        {
            if(!chunk.isInitialized)
            {
                tempIsAllInitalized = false;
                break;
            }
        }
        isAllInitalized = tempIsAllInitalized;
    }

    void ProcessUserInput()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            meshTerrainGenerator.IsInGodMode = !meshTerrainGenerator.IsInGodMode;
            SwitchView();
        }
    }

    void SwitchView()
    {
        if (meshTerrainGenerator.IsInGodMode)
        {
            hUDOptions.gameObject.SetActive(true);
            currentViewTransform = freeView;
            player.parent.gameObject.SetActive(false);
            freeView.gameObject.SetActive(true);
            ToggleCollisionsOnChunks(false);
        }
        else
        {
            hUDOptions.gameObject.SetActive(false);
            currentViewTransform = player;
            player.parent.gameObject.SetActive(true);
            freeView.gameObject.SetActive(false);
            ToggleCollisionsOnChunks(true);

        }
    }

    public void OnValuesChanged()
    {
        if (!meshTerrainGenerator.IsInGodMode)
        {
            return;
        }
        
        foreach(TerrainChunk chunk in terrainChunkDictionary.Values)
        {
            chunk.Reset();
        }
        lastVisibleTerrainChunks.Clear();
    }

    public void ToggleCollisionsOnChunks(bool enabled)
    {
        foreach (TerrainChunk chunk in terrainChunkDictionary.Values)
        {
            chunk.ToggleMeshCollision(enabled);
        }
    }

    void UpdateVisibleChunks()
    {
        isAllInitalized = false;
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
                        new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, testMaterial, scale, 
                        meshTerrainGenerator.regionHeightCurve, waterPrefab, meshTerrainGenerator.IsInGodMode)
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

        GameObject groupedParent;
        public GameObject Parent { get { return groupedParent; } }

        MeshRenderer meshRenderer;

        MeshFilter meshFilter;
        public MeshFilter MeshFilter { get { return meshFilter; } }

        MeshCollider meshCollider;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        LODMesh collisionLODMesh;

        GameObject water;
        public GameObject Water { get { return water; } }
        GameObject cloud;

        Dictionary<Vector2, GameObject> trees;
        public Dictionary<Vector2, GameObject> Trees { get { return trees; } }

        float[,] noiseMap;
        public float[,] NoiseMap { get { return noiseMap; } }

        bool hasReceivedMapData;

        int previousLODIndex = -1;

        public bool isInitialized = false;
        int chunkSize;

        bool hasFishingGuy = false;
        bool hasSwimmingGirl = false;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] LODDetails, Transform parent, Material material, float scale, 
            AnimationCurve regionHeightCurve, GameObject waterPrefab, bool isInGodMode)
        {
            detailLevels = LODDetails;
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            chunkSize = size;

            groupedParent = new GameObject("Grouped Parent");

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.material = material;

            groupedParent.transform.position = positionV3 * scale;
            groupedParent.transform.parent = parent;
            meshObject.transform.parent = groupedParent.transform;
            meshObject.transform.localScale = Vector3.one * scale;
            meshObject.transform.localPosition = Vector3.zero;

            Vector2 boundSizes = new Vector2(size, size);

            water = ProceduralMeshTerrain.CreateWater(boundSizes, waterPrefab, regionHeightCurve, groupedParent.transform);
            cloud = null;
            SetVisible(false);

            trees = new Dictionary<Vector2, GameObject>();

            lodMeshes = new LODMesh[detailLevels.Length];
            for(int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].levelOfDetail, meshObject.transform, trees, UpdateTerrainChunk);
                if(detailLevels[i].useForCollider)
                {
                    collisionLODMesh = lodMeshes[i];
                }
            }

            meshTerrainGenerator.RequestMapData(position, OnMapDataReceived);
        }

        public void Reset()
        {
            this.noiseMap = null;
            hasReceivedMapData = false;

            for(int i = 0; i < lodMeshes.Length; i++)
            {
                lodMeshes[i].hasReceivedMesh = false;
                lodMeshes[i].hasRequestedMesh = false;
            }

            meshTerrainGenerator.RequestMapData(position, OnMapDataReceived);
        }

        public void ToggleMeshCollision(bool enabled)
        {
            meshCollider.enabled = enabled;
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
                        if(cloud == null)
                        {
                            cloud = meshTerrainGenerator.CreateCloud(new Vector2(chunkSize - 1, chunkSize - 1), groupedParent.transform, noiseMap);
                        }
                        if(lodIndex != 0)
                        {
                            isInitialized = true;
                        }
                    }
                    else if (!lodMesh.hasRequestedMesh)
                    {
                        lodMesh.RequestMeshData(noiseMap);
                    }
                }

                if (lodIndex == 0)  //only add the collider for the lowest level of detail (highest resolution)
                {
                    if (meshCollider && collisionLODMesh.hasReceivedMesh)
                    {
                        meshCollider.sharedMesh = collisionLODMesh.mesh;
                        meshCollider.enabled = true;

                        if(collisionLODMesh.hasReceivedTreeData)
                        {
                            if (collisionLODMesh.hasReceivedNPCData)
                            {
                                isInitialized = true;
                            }
                            else if(!collisionLODMesh.hasRequestedNPCData)
                            {
                                collisionLODMesh.RequestNPCData(
                                  ProceduralMeshTerrain.mapChunkSize,
                                  this,
                                  meshTerrainGenerator);
                            }
                        }
                        else if(!collisionLODMesh.hasRequestedTreeData)
                        {
                           collisionLODMesh.RequestTreeData();
                        }
                    }
                    else if (!collisionLODMesh.hasRequestedMesh)
                    {
                        collisionLODMesh.RequestMeshData(noiseMap);
                    }
                }
                else
                {
                    //remove the collider if the level of detail is not the highest resolution
                    if (meshCollider)
                    {
                        meshCollider.enabled = false;
                    }
                }

                lastVisibleTerrainChunks.Add(this);
            }

            SetVisible(isVisible);
        }

        public void SetVisible(bool visible)
        {
            groupedParent.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }

    private void OnDestroy()
    {
        //clear out the static variables
        viewerPosition = Vector2.zero;
        lastVisibleTerrainChunks.Clear();
    }

    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasReceivedMesh;
        public bool hasRequestedTreeData;
        public bool hasReceivedTreeData;
        public bool hasRequestedNPCData;
        public bool hasReceivedNPCData;

        int levelOfDetail;
        Action updateCallback;
        Transform parent;
        Dictionary<Vector2, GameObject> trees;
        MeshData meshData;
        public LODMesh(int levelOfDetail, Transform parentChunk, Dictionary<Vector2, GameObject> trees, Action callback)
        {
            mesh = new Mesh();
            this.levelOfDetail = levelOfDetail;
            updateCallback = callback;
            parent = parentChunk;
            this.trees = trees;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            MeshGenerator.CreateMesh(mesh, meshData);
            hasReceivedMesh = true;
            this.meshData = meshData;
            updateCallback();
        }

        void OnTreeDataReceived(Dictionary<Vector2, Vector3> treePositions, GameObject treePrefab, float[,] noiseMap)
        {
            meshTerrainGenerator.ClearTrees(trees, noiseMap);
            ProceduralMeshTerrain.InstantiateTrees(treePositions, trees, treePrefab, parent);
            hasReceivedTreeData = true;
            updateCallback();
        }

        public void RequestTreeData()
        {
            meshTerrainGenerator.RequestTreeData(meshData, parent, trees, OnTreeDataReceived);
            hasRequestedTreeData = true;
        }

        public void RequestMeshData(float[,] noiseMap)
        {
            meshTerrainGenerator.RequestMeshData(noiseMap, levelOfDetail, OnMeshDataReceived);
            hasRequestedMesh = true;
        }

        public void RequestNPCData(int mapChunkSize, TerrainChunk terrainChunk, ProceduralMeshTerrain proceduralMeshTerrain)
        {
            npcInitiator.RequestNPCData(mapChunkSize,
                terrainChunk.NoiseMap,
                terrainChunk.MeshFilter.mesh.vertices,
                terrainChunk.Water.transform.localPosition,
                proceduralMeshTerrain, 
                OnReceivedNPCData);
            hasRequestedNPCData = true;
        }

        public void OnReceivedNPCData(Dictionary<NPCType, PositionAndRotation> npcPositionAndRotations)
        {
            foreach (KeyValuePair<NPCType, PositionAndRotation> npcPositionAndRotation in npcPositionAndRotations)
            {
                Vector3 localPos = npcPositionAndRotation.Value.position;
                Quaternion rotation = npcPositionAndRotation.Value.rotation;

                GameObject npc = npcInitiator.SpawnNPC(npcPositionAndRotation.Key, localPos, rotation, parent.transform.parent);
            }
            hasReceivedNPCData = true;
            updateCallback();
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