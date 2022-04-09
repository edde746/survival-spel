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
    float targetRotation;
    Animator animator;
    AnimalState state = AnimalState.Wander;
    CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == AnimalState.Idle)
        {
            // Do nothing
        }
        else if (state == AnimalState.Wander)
        {
            // Wandering
            RaycastHit hit;
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
            controller.SimpleMove(transform.forward * Time.deltaTime * walkSpeed);

            // Slightly randomize rotation
            if (Random.Range(0f, 1f) < 0.01f)
            {
                targetRotation = transform.localEulerAngles.y + Random.Range(-30f, 30f);
            }
        }
        else if (state == AnimalState.Attack)
        {
            // TODO: Implement
        }

        // Lerp animator speed
        animator.SetFloat("Speed", Mathf.Lerp(animator.GetFloat("Speed"), controller.velocity.magnitude, Time.deltaTime * 10f));
        // Lerp target rotation
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, Mathf.LerpAngle(transform.localEulerAngles.y, targetRotation, Time.deltaTime * 10f), 0f);
    }
}
