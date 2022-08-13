using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class ButtonTween : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        transform.DOScale(0.8f, 0.1f).OnComplete(() => { transform.DOScale(1f, 0.1f); });
    }

}
