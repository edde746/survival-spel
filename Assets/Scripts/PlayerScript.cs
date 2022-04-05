using System.Collections;
using UnityEngine;

// Handle player specific behaviour
public class PlayerScript : MonoBehaviour
{
    public Vector3 spawnPoint;
    Camera playerCamera;
    public float spawnRadius = 300f;
    bool itemBusy = false;
    float health = 65f, hunger = 75f, thirst = 45f;
    bool showPickableText = false;
    public Vitalbar healthBar, hungerBar, thirstBar;

    void Start()
    {
        // Set spawnpoint
        var point = new Vector3(spawnPoint.x + Random.Range(spawnRadius / -2f, spawnRadius / 2f), 200f, spawnPoint.z + Random.Range(spawnRadius / -2f, spawnRadius / 2f));
        RaycastHit hit;
        if (Physics.Raycast(point, Vector3.down, out hit, Mathf.Infinity, Physics.AllLayers))
            point.y = hit.point.y + 3f;

        transform.position = point;

        // Get camera
        playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        showPickableText = false;
        hunger -= Time.deltaTime * 0.09f;
        thirst -= Time.deltaTime * 0.1f;
        healthBar.SetVital(health / 100f);
        hungerBar.SetVital(hunger / 100f);
        thirstBar.SetVital(thirst / 100f);

        // Starving
        if (hunger < 5f)
            health -= Time.deltaTime * 0.7f;

        // Dehydrated
        if (thirst < 7f)
            health -= Time.deltaTime * 0.6f;


        // Check if busy or mouse not locked
        if (itemBusy || !Cursor.lockState.Equals(CursorLockMode.Locked))
            return;

        ref var activeItem = ref Inventory.Instance.GetActiveItem();
        if (activeItem != null && activeItem.item != null && activeItem.item.flags.HasFlag(ItemFlags.Food) && Input.GetButtonDown("Fire1"))
        {
            hunger += activeItem.item.GetStat("hunger");
            activeItem.Consume(1);
            StartCoroutine(BusyFor(1f));
        }

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 5f, ~(1 << 2)))
        {
            if (hit.transform.gameObject.CompareTag("Interactable"))
            {
                var pickable = hit.transform.GetComponent<Pickable>();
                if (pickable != null)
                {
                    showPickableText = true;
                    if (Input.GetKeyDown(KeyCode.E))
                        pickable.Pick();
                }
            }

            if (Input.GetButton("Fire1") && activeItem != null)
            {
                if (activeItem.count == 0 || activeItem.item == null) return;

                if (activeItem.item.flags.HasFlag(ItemFlags.Tool))
                {
                    var farmable = hit.transform.gameObject.GetComponent<Farmable>();
                    if (farmable != null && (farmable.toolType == 0 || activeItem.item.flags.HasFlag(farmable.toolType)))
                    {
                        StartCoroutine(Farm(farmable, activeItem.item));
                    }
                }
                else if (activeItem.item.flags.HasFlag(ItemFlags.Placeable))
                {
                    StartCoroutine(Place(activeItem.item.model, hit.point));
                    activeItem.Consume(1);
                }
            }
        }
    }

    IEnumerator BusyFor(float time)
    {
        itemBusy = true;
        yield return new WaitForSeconds(time);
        itemBusy = false;
    }

    IEnumerator Farm(Farmable farmable, Item activeItem)
    {
        itemBusy = true;
        // Apply damage & start tool animation
        farmable.ApplyDamage(activeItem.GetStat("damage"));
        var animator = Inventory.Instance.activeItemModel?.GetComponent<Animator>() ?? null;
        if (animator != null)
            animator.SetTrigger("Using");
        yield return new WaitForSeconds((activeItem?.GetStat("cooldown") ?? 1f) * .5f);
        // Award items to player
        var giveAmount = (int)Mathf.Floor(farmable.itemAmount * (1f - farmable.health / farmable.maxHealth));
        farmable.itemAmount -= giveAmount;
        Inventory.Instance.GiveItem(farmable.item, (int)Mathf.Floor(giveAmount * activeItem?.GetStat("gather") ?? 1f));
        // Wait then stop animation
        yield return new WaitForSeconds((activeItem?.GetStat("cooldown") ?? 1f) * .5f);
        itemBusy = false;
    }

    IEnumerator Place(GameObject model, Vector3 point)
    {
        itemBusy = true;
        Instantiate(model, point, Quaternion.identity);
        // TODO: Sound FX & PTFX
        yield return new WaitForSeconds(.5f);
        itemBusy = false;
    }
}
