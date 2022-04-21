using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float sensetivity = 200f;
    public static bool disableLook = false;
    public static bool lockCursor = true;
    float yRotation;

    void Update()
    {
        if (lockCursor && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (!disableLook && Cursor.lockState == CursorLockMode.Locked)
        {
            float x = Input.GetAxis("Mouse X") * sensetivity * Time.deltaTime;
            float y = Input.GetAxis("Mouse Y") * sensetivity * Time.deltaTime;

            yRotation = Mathf.Clamp(yRotation - y, -90f, 90f);
            transform.localRotation = Quaternion.Euler(yRotation, 0f, 0f);
            transform.parent.Rotate(Vector3.up, x);
        }
    }
}