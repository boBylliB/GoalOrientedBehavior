using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderOutput : MonoBehaviour
{
    public List<GameObject> prefabs;

    public Vector3 basePosition;
    public List<Vector3> queueOffsets;
    public List<Vector3> scalingFactors;

    public float aliveTime = 2f;

    private List<GameObject> spawnedObjects;
    private List<int> typeIndices;
    private List<float> lifeTimers;

    void Start()
    {
        spawnedObjects = new List<GameObject>();
        typeIndices = new List<int>();
        lifeTimers = new List<float>();
    }

    void Update()
    {
        for (int idx = 0; idx < spawnedObjects.Count; idx++)
        {
            lifeTimers[idx] -= Time.deltaTime;
            if (lifeTimers[idx] <= 0)
            {
                lifeTimers.RemoveAt(idx);
                RemoveOrderFromQueue(idx);
                break;
            }
        }
    }
    public void AddOrder(int index)
    {
        GameObject newObject = Object.Instantiate(prefabs[index], transform);
        newObject.transform.localScale = scalingFactors[index];
        newObject.transform.localPosition = basePosition + spawnedObjects.Count * queueOffsets[index];
        spawnedObjects.Add(newObject);
        typeIndices.Add(index);
        lifeTimers.Add(aliveTime);
    }
    public void RemoveOrderFromQueue(int index)
    {
        Destroy(spawnedObjects[index]);
        spawnedObjects.RemoveAt(index);
        for (int jdx = 0; jdx < spawnedObjects.Count; jdx++)
        {
            spawnedObjects[jdx].transform.localPosition = basePosition + jdx * queueOffsets[typeIndices[jdx]];
        }
    }
}
