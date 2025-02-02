using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour, IPointerDownHandler
{
    public bool buttonActive = true;
    public int buttonId;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (buttonActive)
            FindObjectOfType<LevelSelector>().OpenLevelOptions(buttonId);
    }
}
