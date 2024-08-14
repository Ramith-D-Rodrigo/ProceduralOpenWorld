using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshBaker : MonoBehaviour
{
    // Start is called before the first frame update
    NavMeshSurface surface;
    InfiniteTerrain infiniteTerrain;

    public Transform player;

    public float updateRate = 0.1f;
    public float movementThreshold = 25;
    public Vector3 navMeshSize;

    private Vector3 worldAnchor; //last position we baked the navmesh
    private NavMeshData navMeshData;
    private List<NavMeshBuildSource> buildSources = new List<NavMeshBuildSource>();

    void Start()
    {
        surface = GetComponent<NavMeshSurface>();
        infiniteTerrain = GetComponent<InfiniteTerrain>();

        navMeshSize = new Vector3(100, 100, 100);

        navMeshData = new NavMeshData();
        NavMesh.AddNavMeshData(navMeshData);
        StartCoroutine(CheckPlayerMovement());
    }

    IEnumerator CheckPlayerMovement()
    {
        yield return new WaitForSeconds(5.0f);
        WaitForSeconds waitForSeconds = new(updateRate);
        while(true)
        {
            if(Vector3.Distance(player.position, worldAnchor) > movementThreshold)
            {
                if(infiniteTerrain.isAllInitalized)
                {
                    BuildNavMesh(true);
                    worldAnchor = player.position;
                }
            }
            yield return waitForSeconds;
        }
    }

    private void BuildNavMesh(bool async)
    {
        Bounds bounds = new Bounds(player.position, navMeshSize);
        List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();

        List<NavMeshModifier> modifiers;

        if (surface.collectObjects == CollectObjects.Children)
        {
            modifiers = new List<NavMeshModifier>(GetComponentsInChildren<NavMeshModifier>());

        }
        else
        {
            modifiers = NavMeshModifier.activeModifiers;
        }

        foreach (NavMeshModifier modifier in modifiers)
        {
            if ((surface.layerMask & (1 << modifier.gameObject.layer)) != 0 && modifier.AffectsAgentType(surface.agentTypeID))
            {
                markups.Add(new NavMeshBuildMarkup()
                {
                    root = modifier.transform,
                    overrideArea = modifier.overrideArea,
                    area = modifier.area,
                    ignoreFromBuild = modifier.ignoreFromBuild
                });
            }
        }

        if(surface.collectObjects == CollectObjects.Children)
        {
            NavMeshBuilder.CollectSources(surface.transform, surface.layerMask, surface.useGeometry, surface.defaultArea, markups, buildSources);
        }
        else
        {
            NavMeshBuilder.CollectSources(bounds, surface.layerMask, surface.useGeometry, surface.defaultArea, markups, buildSources);
        }

        //remove agents from build sources
        buildSources.RemoveAll(source => 
            source.component != null 
            && source.component.gameObject != null 
            && source.component.gameObject.GetComponent<NavMeshAgent>() != null
        );

        foreach (var source in buildSources)
        {
            if (source.shape == NavMeshBuildSourceShape.Mesh && source.sourceObject is Mesh mesh)
            {
                Debug.Log($"Mesh: {mesh.name}, Vertices: {mesh.vertexCount}, source: {source.component}");
            }
            else
            {
                Debug.Log($"Non-Mesh Source: {source.shape}");
            }
        }

        if (async)
        {
            NavMeshBuilder.UpdateNavMeshDataAsync(navMeshData, surface.GetBuildSettings(), buildSources, bounds);
        }
        else
        {
            NavMeshBuilder.UpdateNavMeshData(navMeshData, surface.GetBuildSettings(), buildSources, bounds);
        }
    }
}
