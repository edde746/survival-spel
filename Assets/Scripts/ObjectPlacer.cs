using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    public List<GameObject> objects;
    public float yOffset = 0f;
    public int objectsToPlace = 500;
    public bool placeInGroups = false;
    public bool mixGroups = false;
    public float groupRange;
    public int minGroupCount, maxGroupCount;
    public bool alignWithGround = true;

    (Vector3 position, Vector3 normal) RandomSpot(Vector3 center, float range, bool findFloor = true)
    {
        var point = new Vector3(center.x + Random.Range(range / -2f, range / 2f), 200f, center.z + Random.Range(range / -2f, range / 2f));
        RaycastHit hit;
        if (findFloor && Physics.Raycast(point, Vector3.down, out hit, Mathf.Infinity, 1 << 6))
        {
            point.y = hit.point.y + yOffset;
            // Check if we are below water level
            if (point.y < 12.5f) return RandomSpot(center, range, findFloor);
            return (point, hit.normal);
        }

        return (point, Vector3.zero);
    }

    GameObject GetRandomObject()
    {
        return objects[Random.Range(0, objects.Count - 1)];
    }

    void Start()
    {
        if (placeInGroups)
        {
            int groupSize = 0;
            for (int i = 0; i < objectsToPlace; i += groupSize)
            {
                groupSize = Random.Range(minGroupCount, maxGroupCount);
                var groupPoint = RandomSpot(WorldGeneration.centerPoint, WorldGeneration.size, true);
                GameObject objectToSpawn = null;
                for (int j = 0; j < groupSize; j++)
                {
                    objectToSpawn = !objectToSpawn || mixGroups ? GetRandomObject() : objectToSpawn;
                    var spot = RandomSpot(groupPoint.position, groupRange);
                    var newObject = Instantiate(objectToSpawn, spot.position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                    if (alignWithGround)
                        newObject.transform.rotation = Quaternion.FromToRotation(transform.up, spot.normal) * newObject.transform.rotation;
                }

            }
        }
        else
        {
            for (int i = 0; i < objectsToPlace; i++)
            {
                var spot = RandomSpot(WorldGeneration.centerPoint, WorldGeneration.size);
                var newObject = Instantiate(GetRandomObject(), spot.position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                if (alignWithGround)
                    newObject.transform.rotation = Quaternion.FromToRotation(transform.up, spot.normal) * newObject.transform.rotation;
            }
        }
    }
}
