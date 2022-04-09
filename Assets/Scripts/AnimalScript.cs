using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum AnimalState
{
    Idle,
    Wander,
    Attack
}

public class AnimalScript : MonoBehaviour
{
    public bool hostile = false;
    public float health = 100f;
    public float visionRange = 15f;
    public float walkSpeed = 2f;
    public float attackCooldown = 1f;
    public float attackRange = 2f;
    public float attackStrength = 10f;
    public List<ItemEntry> drops;
    bool busy = false;
    float targetRotation;
    Animator animator;
    AnimalState state = AnimalState.Wander;
    CharacterController controller;
    GameObject player;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (busy || !IsAlive()) return;
        // Get ground normal
        RaycastHit hit;
        var forward = transform.forward;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, 1 << 6))
            forward = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;

        if (state == AnimalState.Idle)
        {
            if (Random.Range(0f, 1f) < 0.01f)
                state = AnimalState.Wander;
        }
        else if (state == AnimalState.Wander)
        {
            // Wandering
            var position = transform.position + new Vector3(0f, controller.height / 2f, 0f);
            if (Physics.Raycast(position, transform.forward, out hit, visionRange * 0.2f, ~(1 << 7)))
            {
                if (Vector3.Angle(transform.forward, hit.point - transform.position) > 30f)
                {
                    // Randomly turn
                    targetRotation = transform.localEulerAngles.y + Random.Range(-30f, 30f);
                }
            }

            // Avoid water
            // Get position infront of the animal
            var targetPosition = transform.position + transform.forward * 4f;
            // Raycast to the ground
            if (Physics.Raycast(targetPosition, Vector3.down, out hit, Mathf.Infinity))
            {
                // Check if below water level
                if (hit.point.y < 0f)
                {
                    // Turn around
                    targetRotation = transform.localEulerAngles.y + 180f;
                    transform.localEulerAngles = new Vector3(0f, targetRotation, 0f);
                }
            }

            // Move forward
            controller.SimpleMove(forward * Time.deltaTime * walkSpeed);

            // Slightly randomize rotation
            if (Random.Range(0f, 1f) < 0.01f)
                targetRotation = transform.localEulerAngles.y + Random.Range(-30f, 30f);

            if (Random.Range(0f, 1f) < 0.001f)
                state = AnimalState.Idle;
        }
        else if (state == AnimalState.Attack)
        {
            if (Vector3.Distance(transform.position, player.transform.position) > visionRange)
                state = AnimalState.Wander;

            // Check if we are close to player
            if (Vector3.Distance(transform.position, player.transform.position) < attackRange)
            {
                // Attack
                animator.SetTrigger("Attack");
                StartCoroutine(Busy(attackCooldown));
                // Apply damage to player
                player.GetComponent<PlayerScript>().ApplyDamage(attackStrength);
            }
            else
            {
                // Turn towards player
                targetRotation = Mathf.Atan2(player.transform.position.x - transform.position.x, player.transform.position.z - transform.position.z) * Mathf.Rad2Deg;

                // Run towards player
                controller.SimpleMove(forward * Time.deltaTime * walkSpeed * 2f);
            }
        }

        // Check if player is in range
        if (hostile && state != AnimalState.Attack && Vector3.Distance(transform.position, player.transform.position) < visionRange)
            state = AnimalState.Attack;

        // Lerp animator speed
        animator.SetFloat("Speed", Mathf.Lerp(animator.GetFloat("Speed"), controller.velocity.magnitude, Time.deltaTime * 10f));
        // Lerp target rotation
        transform.localEulerAngles = new Vector3(0f, Mathf.LerpAngle(transform.localEulerAngles.y, targetRotation, Time.deltaTime * 10f), 0f);
    }

    // Busy coroutine
    IEnumerator Busy(float time)
    {
        busy = true;
        yield return new WaitForSeconds(time);
        busy = false;
    }

    public void ApplyDamage(float damage)
    {
        health -= damage;
        if (health <= 0f)
        {
            // Play death animation
            animator.SetTrigger("Death");
            // Drop loot
            var inventory = player.GetComponentInChildren<Inventory>();
            foreach (var entry in drops)
            {
                inventory.GiveItem(entry.item, entry.count);
            }
            // Destroy the animal
            Destroy(gameObject, 25f);
        }
    }

    public bool IsAlive()
    {
        return health > 0f;
    }
}
