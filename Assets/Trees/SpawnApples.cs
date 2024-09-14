using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnApples : MonoBehaviour
{
    public GameObject spawnApplesPrefab;
    public float spawnInterval = 30.0f; //spawn an apple every 30 seconds
    public float appleLifeTime = 120.0f; //destroy apples after 180 seconds
    public bool shouldSpawnApples = false;
    public Transform spawnLocations; //locations where apples can spawn (children of this transform)

    float currentTime = 0.0f;
    Queue<GameObject> spawnedApples;

    // Start is called before the first frame update
    void Start()
    {
        //randomly decide if we should spawn apples
        shouldSpawnApples = Random.Range(0, 100) % 7 == 0;

        if (shouldSpawnApples)
        {
            spawnedApples = new Queue<GameObject>();
            StartCoroutine(SpawnApple());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!shouldSpawnApples)
        {
            return;
        }
    }

    IEnumerator SpawnApple()
    {
        while(true)
        {
            Transform spawn = spawnLocations.GetChild(Random.Range(0, spawnLocations.childCount));
            GameObject apple = Instantiate(spawnApplesPrefab, spawn.position, Quaternion.identity);
            spawnedApples.Enqueue(apple);
            StartCoroutine(RemoveApple(apple));
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    IEnumerator RemoveApple(GameObject apple)
    {
        while(true)
        {
            yield return new WaitForSeconds(appleLifeTime);
            if(spawnedApples.Count > 0)
            {
                spawnedApples.Dequeue();
                Destroy(apple);
            }
        }
    }
}
