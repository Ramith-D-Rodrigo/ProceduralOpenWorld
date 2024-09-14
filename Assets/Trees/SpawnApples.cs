using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnApples : MonoBehaviour
{
    public GameObject spawnApplesPrefab;
    public float spawnInterval = 10.0f; //spawn an apple every 10 seconds
    public float appleLifeTime = 80.0f; //destroy apples after 80 seconds
    public bool shouldSpawnApples = false;
    public Transform spawnLocations; //locations where apples can spawn (children of this transform)

    float currentTime = 0.0f;
    Queue<GameObject> apples = new Queue<GameObject>(); //to store apples so that we can reuse them

    // Start is called before the first frame update
    void Start()
    {
        //randomly decide if we should spawn apples
        shouldSpawnApples = Random.Range(0, 100) % 10 != 0;

        if (shouldSpawnApples)
        {
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
            bool shouldSpawn = Random.Range(0, 100) % 5 != 0;
            if(!shouldSpawn)
            {
                yield return new WaitForSeconds(spawnInterval);
                continue;
            }
            Transform spawn = spawnLocations.GetChild(Random.Range(0, spawnLocations.childCount));
            GameObject apple = null;
            if(apples.Count == 0)
            {
                apple = Instantiate(spawnApplesPrefab, spawn.position, Quaternion.identity);
            }
            else
            {
                apple = apples.Dequeue();
                apple.transform.position = spawn.position;
                apple.SetActive(true);
            }

            StartCoroutine(RemoveApple(apple));
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    IEnumerator RemoveApple(GameObject apple)
    {
        yield return new WaitForSeconds(appleLifeTime);
        apple.SetActive(false);
        apples.Enqueue(apple);
    }
}
