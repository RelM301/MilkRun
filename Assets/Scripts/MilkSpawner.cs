using System.Collections.Generic;
using UnityEngine;

public class MilkSpawner : MonoBehaviour
{
    #region Variables
    [Header("Setup")]
    [SerializeField] private GameObject milkPrefab;

    [Tooltip("These points will ALWAYS spawn a milk.")]
    [SerializeField] private Transform[] fixedSpawnPoints;

    [Tooltip("The script will pick from these to fill the remaining count.")]
    [SerializeField] private Transform[] randomSpawnPoints;

    private int _totalToSpawn = 5;
    #endregion

    #region Unity Callbacks
    void Start()
    {
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            _totalToSpawn = gm.MilksRequired;
        }

        SpawnMilks();
    }
    #endregion

    #region Private Methods
    void SpawnMilks()
    {
        // 1. Always spawn at the fixed points first
        foreach (Transform fp in fixedSpawnPoints)
        {
            Instantiate(milkPrefab, fp.position, Quaternion.identity);
        }

        // 2. Calculate how many random ones we still need
        int randomNeeded = _totalToSpawn - fixedSpawnPoints.Length;

        if (randomNeeded <= 0) return; // We already hit the limit with fixed points!

        if (randomSpawnPoints.Length < randomNeeded)
        {
            Debug.LogError("Not enough random spawn points to fulfill the GameManager requirement!");
            return;
        }

        // 3. Shuffle and spawn the remaining random ones
        List<int> indices = new List<int>();
        for (int i = 0; i < randomSpawnPoints.Length; i++) indices.Add(i);

        for (int i = 0; i < indices.Count; i++)
        {
            int temp = indices[i];
            int randomIndex = Random.Range(i, indices.Count);
            indices[i] = indices[randomIndex];
            indices[randomIndex] = temp;
        }

        for (int i = 0; i < randomNeeded; i++)
        {
            Instantiate(milkPrefab, randomSpawnPoints[indices[i]].position, Quaternion.identity);
        }
    }
    #endregion
}
