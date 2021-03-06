using UnityEngine;

public class Movement : MonoBehaviour
{
    CharacterController controller;
    public float speed = 12f;
    public static bool disableMovement = false;
    Vector3 velocity;

    bool onGround = false;

    public bool isOnGround
    {
        get
        {
            return onGround;
        }
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        onGround = Physics.CheckSphere(transform.position + new Vector3(0f, -0.65f * transform.localScale.y, 0f), 0.5f, ~(1 << 2 | 1 << 4));
        if (onGround && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        if (disableMovement) return;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 direction = transform.right * x + transform.forward * z;

        controller.Move(direction * speed * Time.deltaTime);

        velocity += Physics.gravity * 2f * Time.deltaTime;

        if (onGround && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y += 9f;
        }

        controller.Move(velocity * Time.deltaTime);
    }
}