using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonTween : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Transform target;
    [SerializeField] private float scale = 0.9f;
    public new bool enabled = true;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (enabled)
            target.DOScale(scale, 0.1f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (enabled)
            target.DOScale(1f, 0.1f);
    }

}
