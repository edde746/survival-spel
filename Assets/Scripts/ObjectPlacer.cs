using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    public List<GameObject> objects;
    public int objectsToPlace = 500;
    public bool placeInGroups = false;
    public bool mixGroups = false;
    public float groupRange;
    public int minGroupCount, maxGroupCount;
    Vector3 centerPoint;

    Vector3 RandomSpot(Vector3 center, float range, bool findFloor = true)
    {
        var point = new Vector3(center.x + Random.Range(range / -2f, range / 2f), 200f, center.z + Random.Range(range / -2f, range / 2f));
        RaycastHit hit;
        if (findFloor && Physics.Raycast(point, Vector3.down, out hit, Mathf.Infinity, Physics.AllLayers))
        {
            point.y = hit.point.y - 0.02f;
            // Check if we are below water level
            if (point.y < 10f) return RandomSpot(center, range, findFloor);
        }

        return point;
    }

    void Start()
    {
        centerPoint = WorldGeneration.GetCenterPoint();

        if (placeInGroups)
        {
            int groupSize = 0;
            for (int i = 0; i < objectsToPlace; i += groupSize)
            {
                groupSize = Random.Range(minGroupCount, maxGroupCount);
                var groupPoint = RandomSpot(centerPoint, WorldGeneration.size, true);
                GameObject objectToSpawn = null;
                for (int j = 0; j < groupSize; j++)
                {
                    objectToSpawn = !objectToSpawn || mixGroups ? objects[Random.Range(0, objects.Count - 1)] : objectToSpawn;
                    Instantiate(objectToSpawn, RandomSpot(groupPoint, groupRange), Quaternion.identity);
                }

            }
        }
        else
        {
            for (int i = 0; i < objectsToPlace; i++)
            {
                Instantiate(objects[Random.Range(0, objects.Count - 1)], RandomSpot(centerPoint, WorldGeneration.size), Quaternion.identity);
            }
        }
    }
}
