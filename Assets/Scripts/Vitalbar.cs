using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Vitalbar : MonoBehaviour
{
    RectTransform fillTransform;

    public void SetVital(float percent)
    {
        fillTransform.sizeDelta = new Vector2(percent * 500f, fillTransform.sizeDelta.y);
    }

    void Start()
    {
        fillTransform = transform.GetChild(0).GetComponent<RectTransform>();
    }
}
