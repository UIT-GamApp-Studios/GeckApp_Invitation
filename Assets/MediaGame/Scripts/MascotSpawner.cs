using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MascotSpawner : MonoBehaviour
{
    [SerializeField] private MediaGameConfig config;
    [SerializeField] private GameObject[] mascotPrefabs; 

    [Header("Spawn Boundaries")]
    [SerializeField] private Transform spawnPointLeft;
    [SerializeField] private Transform spawnPointRight;
    [SerializeField] private Transform topBoundary;

    private List<Mascot> activeMascots = new List<Mascot>();
    private bool isSpawning = false;

    public List<Mascot> ActiveMascots => activeMascots;

    public void StartSpawning()
    {
        isSpawning = true;
        StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            activeMascots.RemoveAll(m => m == null);

            if (activeMascots.Count < config.maxActiveMascots)
            {
                TrySpawnMascot();
            }

            yield return new WaitForSeconds(config.mascotSpawnInterval);
        }
    }

    private void TrySpawnMascot()
    {
        if (mascotPrefabs == null || mascotPrefabs.Length == 0)
        {
            Debug.LogWarning("MascotPrefabs array is empty!");
            return;
        }

        float randomX = Random.Range(spawnPointLeft.position.x, spawnPointRight.position.x);
        Vector2 spawnPos = new Vector2(randomX, spawnPointLeft.position.y);

        Collider2D hit = Physics2D.OverlapCircle(spawnPos, config.spawnOverlapRadius);
        if (hit != null && hit.GetComponent<Mascot>() != null)
        {
            return;
        }

        int randomIndex = Random.Range(0, mascotPrefabs.Length);
        GameObject selectedPrefab = mascotPrefabs[randomIndex];

        GameObject obj = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);
        Mascot mascot = obj.GetComponent<Mascot>();
        mascot.Initialize(config, topBoundary.position.y);

        activeMascots.Add(mascot);
    }
}