using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Button_UI : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Application.isFocused)
            AudioManager.Instance.PlayOneShot("OverButtonUI");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance.PlayOneShot("ClickButtonUI");
    }
}
