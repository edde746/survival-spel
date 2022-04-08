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
        return RandomSpot(center, new Vector3(range, 0f, range), findFloor);
    }

    (Vector3 position, Vector3 normal) RandomSpot(Vector3 center, Vector3 range, bool findFloor = true)
    {
        var point = new Vector3(center.x + Random.Range(range.x / -2f, range.x / 2f), 200f, center.z + Random.Range(range.z / -2f, range.z / 2f));
        RaycastHit hit;
        if (findFloor && Physics.Raycast(point, Vector3.down, out hit, Mathf.Infinity, 1 << 6))
        {
            point.y = hit.point.y + yOffset;
            // Check if we are below water level
            if (point.y < 12.8f) return RandomSpot(center, range, findFloor);
            return (point, hit.normal);
        }

        return (Vector3.zero, Vector3.zero);
    }

    GameObject GetRandomObject()
    {
        return objects[Random.Range(0, objects.Count - 1)];
    }

    void Start()
    {
        // Get world dimensions
        var world = GameObject.Find("World");
        var worldGenerator = world.GetComponent<WorldGenerator>();
        var worldSize = new Vector3(worldGenerator.size, 0f, worldGenerator.size);

        if (placeInGroups)
        {
            int groupSize = 0;
            for (int i = 0; i < objectsToPlace; i += groupSize)
            {
                groupSize = Random.Range(minGroupCount, maxGroupCount);
                var groupPoint = RandomSpot(worldSize / 2f, worldSize, true);
                GameObject objectToSpawn = null;
                for (int j = 0; j < groupSize; j++)
                {
                    objectToSpawn = !objectToSpawn || mixGroups ? GetRandomObject() : objectToSpawn;
                    var spot = RandomSpot(groupPoint.position, groupRange);
                    if (spot.position == Vector3.zero) continue;
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
                var spot = RandomSpot(worldSize / 2f, worldSize);
                if (spot.position == Vector3.zero) continue;
                var newObject = Instantiate(GetRandomObject(), spot.position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                if (alignWithGround)
                    newObject.transform.rotation = Quaternion.FromToRotation(transform.up, spot.normal) * newObject.transform.rotation;
            }
        }

        Debug.Log(gameObject.name);
    }
}
