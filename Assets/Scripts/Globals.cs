using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour
{
    public GameObject rockDestroyParticle;
    public GameObject notificationsContainer;
    public GameObject notificationPrefab;

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

        // Clear children of notifications container
        foreach (Transform child in notificationsContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    // Create notification and set data
    public static void CreateNotification(string text, float duration = 3f, Sprite icon = null)
    {
        var notification = Instantiate(Instance.notificationPrefab, Instance.notificationsContainer.transform, false);
        notification.GetComponent<Notification>().SetData(icon, text, duration);
    }
}
