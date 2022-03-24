using UnityEngine;

public class Movement : MonoBehaviour
{
    CharacterController controller;
    public float speed = 12f;
    Vector3 velocity;

    bool onGround = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        onGround = Physics.CheckSphere(transform.position + new Vector3(0f, -0.6f, 0f), 0.5f, ~(1 << 6));
        if (onGround && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

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