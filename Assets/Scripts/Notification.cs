using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Notification : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI text;

    public void SetData(Sprite sprite, string text, float timed = 0f)
    {
        if (sprite)
        {
            icon.sprite = sprite;
        }
        else
        {
            icon.gameObject.SetActive(false);
            this.text.rectTransform.anchoredPosition = new Vector2(10f, 0f);
        }

        this.text.text = text;

        if (timed > 0f)
            Destroy(gameObject, timed);
    }
}
