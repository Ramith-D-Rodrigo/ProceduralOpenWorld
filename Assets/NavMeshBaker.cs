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

    public float updateRate = 0.5f;
    public float movementThreshold = 75;
    public Vector3 navMeshSize = new Vector3(180, 180, 180);

    private Vector3 worldAnchor; //last position we baked the navmesh
    private NavMeshData navMeshData;
    private List<NavMeshBuildSource> buildSources = new List<NavMeshBuildSource>();
    private List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();
    private List<NavMeshModifier> modifiers = new List<NavMeshModifier>();

    void Start()
    {
        surface = GetComponent<NavMeshSurface>();
        infiniteTerrain = GetComponent<InfiniteTerrain>();

        navMeshData = new NavMeshData();
        NavMesh.AddNavMeshData(navMeshData);
        BuildNavMesh(false);
        StartCoroutine(CheckPlayerMovement());
    }

    IEnumerator CheckPlayerMovement()
    {
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

        if(markups.Count == 0)
        {
            if (surface.collectObjects == CollectObjects.Children && modifiers.Count == 0)
            {
                modifiers = new List<NavMeshModifier>(GetComponentsInChildren<NavMeshModifier>());

            }
            else if (surface.collectObjects != CollectObjects.Children && modifiers.Count == 0)
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
        }

        if(buildSources.Count == 0)
        {
            if (surface.collectObjects == CollectObjects.Children)
            {
                NavMeshBuilder.CollectSources(surface.transform, surface.layerMask, surface.useGeometry, surface.defaultArea, markups, buildSources);
            }
            else
            {
                NavMeshBuilder.CollectSources(bounds, surface.layerMask, surface.useGeometry, surface.defaultArea, markups, buildSources);
            }
        }


        if (async)
        {
            NavMeshBuilder.UpdateNavMeshDataAsync(navMeshData, surface.GetBuildSettings(), buildSources, bounds);
        }
        else
        {
            NavMeshBuilder.UpdateNavMeshData(navMeshData, surface.GetBuildSettings(), buildSources, bounds);
            Debug.Log("NavMesh baked!");
        }
    }
}
