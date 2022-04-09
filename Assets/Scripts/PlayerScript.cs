using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Handle player specific behaviour
public class PlayerScript : MonoBehaviour
{
    public Vector3 spawnPoint;
    Camera playerCamera;
    public float spawnRadius = 300f;
    bool itemBusy = false;
    [HideInInspector]
    public float health = 100f, hunger = 100f, thirst = 100f;
    public GameObject pickableText;
    public Vitalbar healthBar, hungerBar, thirstBar;
    GameObject healthNotification, hungerNotification, thirstNotification;
    AudioSource footstepSource;
    public List<AudioClip> footstepSounds;
    CharacterController controller;
    float timeSinceLastFootstep = 0f;
    Movement movement;

    public static PlayerScript Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
            return;

        Instance = this;
    }

    void Start()
    {
        // Get components
        playerCamera = GetComponentInChildren<Camera>();
        footstepSource = GetComponentInChildren<AudioSource>();
        controller = GetComponent<CharacterController>();
        movement = GetComponent<Movement>();

        Spawn();
    }

    void Spawn()
    {
        // Set spawnpoint
        var point = new Vector3(spawnPoint.x + Random.Range(spawnRadius / -2f, spawnRadius / 2f), 200f, spawnPoint.z + Random.Range(spawnRadius / -2f, spawnRadius / 2f));

        RaycastHit hit;
        if (Physics.Raycast(point, Vector3.down, out hit, Mathf.Infinity, Physics.AllLayers))
            point.y = hit.point.y + 3f;

        transform.position = point;

        // Generate random vitals to start
        health = Random.Range(50f, 90f);
        hunger = Random.Range(45f, 80f);
        thirst = Random.Range(40f, 80f);
    }

    void Update()
    {
        hunger -= Time.deltaTime * 0.09f;
        thirst -= Time.deltaTime * 0.1f;
        healthBar.SetVital(health / 100f);
        hungerBar.SetVital(hunger / 100f);
        thirstBar.SetVital(thirst / 100f);

        // Starving
        if (hunger < 5f)
        {
            health -= Time.deltaTime * 0.7f;
            if (healthNotification == null)
                healthNotification = Globals.CreateNotification("You are starving!", 0f);
        }
        else
        {
            if (healthNotification != null)
                Destroy(healthNotification);
        }

        // Dehydrated
        if (thirst < 7f)
        {
            health -= Time.deltaTime * 0.6f;
            if (thirstNotification == null)
                thirstNotification = Globals.CreateNotification("You are dehydrated!", 0f);
        }
        else
        {
            if (thirstNotification != null)
                Destroy(thirstNotification);
        }

        // Dead
        if (health < 0f)
            Die();

        // Check if busy or mouse not locked
        if (itemBusy || !Cursor.lockState.Equals(CursorLockMode.Locked))
            return;

        ref var activeItem = ref Inventory.Instance.GetActiveItem();
        if (activeItem != null && activeItem.item != null && activeItem.item.flags.HasFlag(ItemFlags.Food) && Input.GetButtonDown("Fire1"))
        {
            if (hunger < 98f)
            {
                hunger = Mathf.Clamp(hunger + activeItem.item.GetStat("hunger"), 0f, 100f);
                activeItem.Consume(1);
                Globals.PlayEatSound(transform.position);
                StartCoroutine(BusyFor(1f));
            }
        }

        var showPickableText = false;
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 5f, ~(1 << 2)))
        {
            // Check if we are aiming at a pickable item
            if (hit.transform.gameObject.CompareTag("Interactable"))
            {
                var pickable = hit.transform.GetComponent<Pickable>();
                if (pickable != null)
                {
                    if (!showPickableText)
                        pickableText.GetComponent<TextMeshProUGUI>().text = pickable.GetPickupText();
                    showPickableText = true;
                    if (Input.GetKeyDown(KeyCode.E))
                        pickable.Pick();
                }
            }

            // Handle usage of item
            if (Input.GetButton("Fire1") && activeItem != null)
            {
                if (activeItem.count == 0 || activeItem.item == null) return;

                if (activeItem.item.flags.HasFlag(ItemFlags.Tool))
                {
                    // Use tool
                    var farmable = hit.transform.gameObject.GetComponent<Farmable>();
                    if (farmable != null && (farmable.toolType == 0 || activeItem.item.flags.HasFlag(farmable.toolType)))
                    {
                        StartCoroutine(Farm(farmable, activeItem.item));
                    }
                }

                if (activeItem.item.flags.HasFlag(ItemFlags.Placeable))
                {
                    // Place item
                    StartCoroutine(Place(activeItem.item.model, hit.point));
                    activeItem.Consume(1);
                }

                if (activeItem.item.flags.HasFlag(ItemFlags.Weapon))
                {
                    // Check if we are aiming at an animal
                    var animal = hit.transform.gameObject.GetComponent<AnimalScript>();
                    if (animal != null && animal.IsAlive())
                    {
                        // Attack animal
                        StartCoroutine(Attack(animal, activeItem.item));
                    }
                }
            }
        }

        pickableText.SetActive(showPickableText);

        // Play footsteps if player is moving
        if (Mathf.Abs(controller.velocity.magnitude) > 0f && movement.isOnGround)
        {
            if (timeSinceLastFootstep > 0.3f)
            {
                // Play random footstep sound
                footstepSource.clip = footstepSounds[Random.Range(0, footstepSounds.Count)];
                footstepSource.Play();
                timeSinceLastFootstep = 0f;
            }
            timeSinceLastFootstep += Time.deltaTime;
        }
        else
        {
            if (footstepSource.isPlaying)
                footstepSource.Stop();
        }
    }

    IEnumerator BusyFor(float time)
    {
        itemBusy = true;
        yield return new WaitForSeconds(time);
        itemBusy = false;
    }

    // Farm coroutine
    IEnumerator Farm(Farmable farmable, Item activeItem)
    {
        itemBusy = true;
        // Apply damage & start tool animation
        farmable.ApplyDamage(activeItem.GetStat("damage"));
        var animator = Inventory.Instance.activeItemModel?.GetComponent<Animator>() ?? null;
        if (animator != null) animator.SetTrigger("Using");
        yield return new WaitForSeconds((activeItem?.GetStat("cooldown") ?? 1f) * .5f);
        // Award items to player
        var giveAmount = (int)Mathf.Floor(farmable.itemAmount * (1f - farmable.health / farmable.maxHealth));
        farmable.itemAmount -= giveAmount;
        Inventory.Instance.GiveItem(farmable.item, (int)Mathf.Floor(giveAmount * activeItem?.GetStat("gather") ?? 1f));
        // Wait then stop animation
        yield return new WaitForSeconds((activeItem?.GetStat("cooldown") ?? 1f) * .5f);
        itemBusy = false;
    }

    // Place coroutine
    IEnumerator Place(GameObject model, Vector3 point)
    {
        itemBusy = true;
        Instantiate(model, point, Quaternion.identity);
        // TODO: Sound FX & PTFX
        yield return new WaitForSeconds(.5f);
        itemBusy = false;
    }

    // Attack coroutine
    IEnumerator Attack(AnimalScript animal, Item activeItem)
    {
        itemBusy = true;
        // Apply damage & start wewapon animation
        animal.ApplyDamage(activeItem.GetStat("damage"));
        var animator = Inventory.Instance.activeItemModel?.GetComponent<Animator>() ?? null;
        if (animator != null) animator.SetTrigger("Using");
        yield return new WaitForSeconds((activeItem?.GetStat("cooldown") ?? 1f) * .5f);
        // Wait then stop animation
        yield return new WaitForSeconds((activeItem?.GetStat("cooldown") ?? 1f) * .5f);
        itemBusy = false;
    }

    // Apply damage function
    public void ApplyDamage(float damage)
    {
        health -= damage;
        if (health <= 0f)
            Die();
    }

    public void Die()
    {
        // TODO: Show some kind of game over/respawn screen
        // Debug.Log("You died!");
    }
}
