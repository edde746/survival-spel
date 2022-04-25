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
    bool busy = false;
    [HideInInspector]
    public float health = 100f, hunger = 100f, thirst = 100f;
    public GameObject crosshairTooltip;
    public Vitalbar healthBar, hungerBar, thirstBar;
    GameObject healthNotification, hungerNotification, thirstNotification;
    AudioSource footstepSource;
    public List<AudioClip> footstepSounds;
    CharacterController controller;
    float timeSinceLastFootstep = 0f;
    Movement movement;
    GameObject previewBlock;
    [HideInInspector]
    public BuildingBlock selectedBlock;
    public GameObject blockPickerGUI;
    public int rotationOffset = 0;

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
        VitalsUpdate();
        HammerUpdate();
        var showCrosshair = false;

        // Check if mouse not locked
        if (!Cursor.lockState.Equals(CursorLockMode.Locked))
        {
            Globals.Instance.crosshair.SetActive(false);
            return;
        }

        // Busy check
        if (busy) return;

        RaycastHit hit;
        var activeItem = Inventory.Instance.GetActiveItem();

        // Handle eating active item (if food)
        // TODO: Drinking
        if (activeItem?.item?.flags.HasFlag(ItemFlags.Food) ?? false && Input.GetButtonDown("Fire1") && hunger < 98f)
        {
            hunger = Mathf.Clamp(hunger + activeItem.item.GetStat("hunger"), 0f, 100f);
            activeItem.Consume(1);
            Globals.PlayEatSound(transform.position);
            StartCoroutine(BusyFor(1f));
        }

        var showCrosshairTooltip = false;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 2f, ~(1 << 2)))
        {
            // Check if we are aiming at a pickable item
            if (hit.transform.gameObject.CompareTag("Interactable"))
            {
                showCrosshair = true;
                var pickable = hit.transform.GetComponent<Pickable>();
                if (pickable != null)
                {
                    if (!showCrosshairTooltip)
                        crosshairTooltip.GetComponent<TextMeshProUGUI>().text = pickable.GetPickupText();

                    showCrosshairTooltip = true;
                    if (Input.GetKeyDown(KeyCode.E))
                        pickable.Pick();
                }
            }

            // Handle usage of item
            if (Input.GetButton("Fire1") && activeItem != null)
            {
                // Check that item is valid
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
                    // TODO: Creature/Living thing base class
                    var animal = hit.transform.gameObject.GetComponent<AnimalScript>();
                    if (animal != null && animal.IsAlive())
                    {
                        // Attack animal
                        StartCoroutine(Attack(animal, activeItem.item));
                    }
                }
            }
        }
        else if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 2.5f, 1 << 4))
        {

            if (!showCrosshairTooltip)
                crosshairTooltip.GetComponent<TextMeshProUGUI>().text = "Press [E] to drink";
            showCrosshairTooltip = true;
            showCrosshair = true;

            // Handle water actions (drinking, filling container)
            // TODO: Filling water container

            if (Input.GetKeyDown(KeyCode.E))
            {
                // Drinking direct from water source
                thirst = Mathf.Min(thirst + Random.Range(5f, 10f), 100f);
            }
        }

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

        // Update UI elements
        Globals.Instance.crosshair.SetActive(showCrosshair);
        crosshairTooltip.SetActive(showCrosshairTooltip);
    }

    public void SetActiveBuildingBlock(BuildingBlock block)
    {
        selectedBlock = block;
        blockPickerGUI.SetActive(false);
        Globals.SetGUICursorActive(false);
        if (previewBlock != null)
        {
            Destroy(previewBlock);
            previewBlock = null;
        }
    }

    void HammerUpdate()
    {
        // Handle hammer item (building)
        if (Inventory.Instance.GetActiveItem()?.item?.id == 9)
        {
            // Change building block
            if (Input.GetButtonDown("Fire2") || Input.GetKey(KeyCode.T))
            {
                // Show block picker
                blockPickerGUI.SetActive(true);
                Globals.SetGUICursorActive(true);
            }

            // Cycle rotation (0-3) on R key
            if (Input.GetKeyDown(KeyCode.R))
            {
                rotationOffset = (rotationOffset + 1) % 4;
            }

            if (selectedBlock == null) return;

            RaycastHit hit;
            var previewPosition = Vector3.zero;
            var previewRotation = Quaternion.LookRotation(playerCamera.transform.forward);
            previewRotation.eulerAngles = new Vector3(0f, previewRotation.eulerAngles.y, 0f);

            // Find snapping point
            var snapPoints = Physics.RaycastAll(playerCamera.transform.position, playerCamera.transform.forward, 5f, 1 << 8);
            foreach (var point in snapPoints)
            {
                if (selectedBlock.snapsTo.HasFlag(point.transform.GetComponent<BuildingSnap>().type))
                {
                    // Check if point is occupied
                    // Get point collider
                    var pointCollider = point.collider;
                    var overlaps = Physics.OverlapBox(pointCollider.bounds.center, pointCollider.bounds.extents * 0.5f, Quaternion.identity, 1 << 9, QueryTriggerInteraction.Collide);

                    // Check if point is occupied
                    if (overlaps.Length > 0 && !selectedBlock.flags.HasFlag(BlockFlag.ForgoeCollision))
                        return;

                    previewPosition = point.transform.position;
                    previewRotation = point.transform.rotation;
                    // Apply rotation offset
                    if (selectedBlock.flags.HasFlag(BlockFlag.CanRotate))
                        previewRotation.eulerAngles = new Vector3(0f, previewRotation.eulerAngles.y + rotationOffset * 90f, 0f);
                    break;
                }
            }

            if (selectedBlock.flags.HasFlag(BlockFlag.CanPlaceOnGround) && previewPosition == Vector3.zero)
            {
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 5f, 1 << 6))
                {
                    previewPosition = hit.point;
                }
                else
                {
                    previewPosition = playerCamera.transform.position + playerCamera.transform.forward * 5f;
                }
            }

            if (previewPosition == Vector3.zero)
            {
                if (previewBlock != null)
                {
                    Destroy(previewBlock);
                    previewBlock = null;
                }
                return;
            }

            // Place block
            if (!busy && Input.GetButtonDown("Fire1") && Cursor.lockState.Equals(CursorLockMode.Locked))
            {
                // TODO: Cost
                var newBlock = Instantiate(selectedBlock.block, previewPosition, Quaternion.identity);
                newBlock.transform.rotation = previewBlock.transform.rotation;
                StartCoroutine(BusyFor(0.3f));
            }

            // Preview building block
            if (previewBlock == null && selectedBlock != null)
            {
                previewBlock = Instantiate(selectedBlock.block, previewPosition, Quaternion.identity);
                // Set material of all mesh renderers of preview block
                foreach (var meshRenderer in previewBlock.GetComponentsInChildren<MeshRenderer>())
                    meshRenderer.material = Globals.Instance.buildingBlockPreviewMaterial;
                // Disable all colliders of preview block
                foreach (var collider in previewBlock.GetComponentsInChildren<Collider>())
                    collider.enabled = false;
            }
            else if (previewBlock != null)
            {
                previewBlock.transform.position = previewPosition;
                previewBlock.transform.rotation = previewRotation;
            }
        }
        else if (previewBlock != null)
        {
            Destroy(previewBlock);
            previewBlock = null;
        }
    }

    void VitalsUpdate()
    {
        // Update vitals
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
    }

    IEnumerator BusyFor(float time)
    {
        busy = true;
        yield return new WaitForSeconds(time);
        busy = false;
    }

    // Farm coroutine
    IEnumerator Farm(Farmable farmable, Item activeItem)
    {
        busy = true;
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
        busy = false;
    }

    // Place coroutine
    IEnumerator Place(GameObject model, Vector3 point)
    {
        busy = true;
        Instantiate(model, point, Quaternion.identity);
        // TODO: Sound FX & PTFX
        yield return new WaitForSeconds(.5f);
        busy = false;
    }

    // Attack coroutine
    IEnumerator Attack(AnimalScript animal, Item activeItem)
    {
        busy = true;
        // Apply damage & start wewapon animation
        animal.ApplyDamage(activeItem.GetStat("damage"));
        var animator = Inventory.Instance.activeItemModel?.GetComponent<Animator>() ?? null;
        if (animator != null) animator.SetTrigger("Using");
        yield return new WaitForSeconds((activeItem?.GetStat("cooldown") ?? 1f) * .5f);
        // Wait then stop animation
        yield return new WaitForSeconds((activeItem?.GetStat("cooldown") ?? 1f) * .5f);
        busy = false;
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
