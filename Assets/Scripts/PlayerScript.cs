using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handle player specific behaviour
public class PlayerScript : MonoBehaviour
{
    public Vector3 spawnPoint;
    Camera camera;
    public float cooldown = 0f;
    public float spawnRadius = 300f;
    void Start()
    {
        // Set spawnpoint
        var point = new Vector3(spawnPoint.x + Random.Range(spawnRadius / -2f, spawnRadius / 2f), 200f, spawnPoint.z + Random.Range(spawnRadius / -2f, spawnRadius / 2f));
        RaycastHit hit;
        if (Physics.Raycast(point, Vector3.down, out hit, Mathf.Infinity, Physics.AllLayers))
            point.y = hit.point.y + 3f;

        transform.position = point;

        // Get camera
        camera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (cooldown > -0.1f)
            cooldown -= Time.deltaTime;

        if (Input.GetButton("Fire1") && cooldown < 0f)
        {
            var activeItem = Inventory.Instance.GetActiveItem();
            if (activeItem != null)
            {
                var itemCooldown = activeItem.GetStat("cooldown");
                if (itemCooldown > 0f)
                {
                    cooldown = itemCooldown;
                }
                else
                {
                    cooldown = 1f;
                }
            }
            else
            {
                cooldown = 1f;
            }

            RaycastHit hit;
            if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, 5f, ~(1 << 2)))
            {
                var farmable = hit.transform.gameObject.GetComponent<Farmable>();
                if (farmable)
                {
                    if (farmable.toolType == ItemType.Any || farmable.toolType == activeItem.type)
                    {
                        farmable.health -= activeItem.GetStat("damage");
                        // TODO: Implement gather amount
                        Inventory.Instance.GiveItem(farmable.item, farmable.itemAmount);
                    }
                }
            }
        }
    }
}
