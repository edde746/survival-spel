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

    public static Texture2D SolidColorTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    public static Globals Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
            return;

        Instance = this;
    }
}
