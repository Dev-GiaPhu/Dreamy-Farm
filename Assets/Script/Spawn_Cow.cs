using UnityEngine;
using System.Collections;

public class Spawn_Cow : MonoBehaviour
{
    [Header("Thiết lập Spawn")]
    public GameObject cowPrefab;
    public int numberOfCows = 5;
    public float spawnDelay = 2f;       // thời gian giữa các lần spawn
    public float respawnDelay = 5f;     // thời gian respawn sau khi bò chết
    public float spawnAreaMin;
    public float spawnAreaMax;

    public bool isSpawning = false;

    public int currentCowCount = 0;

    void Start()
    {
    }

    void Update()
    {
        if (currentCowCount < numberOfCows && !isSpawning)
        {
            StartCoroutine(SpawnCowWithDelay());
        }
    }

    void SpawnOneCow()
    {
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float randomDist = Random.Range(spawnAreaMin, spawnAreaMax);
        Vector2 spawnPosition = (Vector2)transform.position + randomDir * randomDist;

        GameObject cow = Instantiate(cowPrefab, spawnPosition, Quaternion.identity, transform);
        SetChildrenLayerRecursively(cow, gameObject.layer);
    }

    public void OnCowDied()
    {
        currentCowCount -= 1;
    }
    IEnumerator SpawnCowWithDelay()
    {
        isSpawning = true;
        yield return new WaitForSeconds(spawnDelay);
        SpawnOneCow();
        currentCowCount++;
        isSpawning = false;
    }

    void SetChildrenLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetChildrenLayerRecursively(child.gameObject, layer);
        }
    }

}

