using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropHandler : MonoBehaviour
{
    public List<GameObject> prefabs;
    public RestaurantSeeker seeker;
    public List<string> trackedGoalNames;

    public List<Vector3> positions;
    public List<Vector3> stackOffsets;
    public List<Vector3> scalingFactors;

    private List<List<GameObject>> spawnedObjects;

    void Start()
    {
        spawnedObjects = new List<List<GameObject>>();
        foreach (GameObject prefab in prefabs)
        {
            spawnedObjects.Add(new List<GameObject>());
        }
    }

    void Update()
    {
        for (int idx = 0; idx < prefabs.Count; idx++)
        {
            Goal trackedGoal = seeker.goals.Find(x => x.name == trackedGoalNames[idx]);
            while (Mathf.FloorToInt(5 - trackedGoal.value) > spawnedObjects[idx].Count)
            {
                GameObject newObject = Object.Instantiate(prefabs[idx], transform);
                newObject.transform.localScale = scalingFactors[idx];
                newObject.transform.localPosition = positions[idx] + spawnedObjects[idx].Count * stackOffsets[idx];
                spawnedObjects[idx].Add(newObject);
            }
            while (Mathf.FloorToInt(5 - trackedGoal.value) < spawnedObjects[idx].Count)
            {
                Destroy(spawnedObjects[idx][spawnedObjects[idx].Count-1]);
                spawnedObjects[idx].RemoveAt(spawnedObjects[idx].Count - 1);
            }
        }
    }
}
