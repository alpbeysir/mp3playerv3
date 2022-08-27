using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OptionsElement : MonoBehaviour, IPointerClickHandler
{
    private OptionsViewArgs.OptionsElementArgs args;
    [SerializeField] private MaterialIcon iconDisplay;
    [SerializeField] private TMPro.TextMeshProUGUI textDisplay;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (Vector2.Distance(eventData.pressPosition, eventData.position) / Screen.width < 0.03f)
        {
            args.onClick.Invoke();
            ScreenManager.Instance.Back();
        }
    }

    public void Populate(OptionsViewArgs.OptionsElementArgs _args)
    {
        args = _args;
        iconDisplay.iconUnicode = args.iconUnicode;
        textDisplay.text = args.title;
    }
}
