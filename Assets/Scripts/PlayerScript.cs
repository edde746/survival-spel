using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handle player specific behaviour
public class PlayerScript : MonoBehaviour
{
    public Vector3 spawnPoint;
    public float spawnRadius = 300f;
    void Start()
    {
        var point = new Vector3(spawnPoint.x + Random.Range(spawnRadius / -2f, spawnRadius / 2f), 200f, spawnPoint.z + Random.Range(spawnRadius / -2f, spawnRadius / 2f));
        RaycastHit hit;
        if (Physics.Raycast(point, Vector3.down, out hit, Mathf.Infinity, Physics.AllLayers))
            point.y = hit.point.y + 3f;

        transform.position = point;
    }

}
