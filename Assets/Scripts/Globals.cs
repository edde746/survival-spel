using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour
{
    public GameObject rockDestroyParticle;
    public static Vector2 ScreenMousePosition()
    {
        // Fuck you, Unity
        return new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
    }

    public static Globals Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
            return;

        Instance = this;
    }
}
