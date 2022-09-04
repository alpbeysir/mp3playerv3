using MP3Player.Components;
using MP3Player.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MP3Player.Views
{
    public class OptionsViewArgs
    {
        public struct OptionsElementArgs
        {
            public string iconUnicode;
            public string title;
            public Action onClick;
        }
        public List<OptionsElementArgs> options = new();
    }

    public class OptionsView : UIView, IPointerClickHandler
    {
        [SerializeField] private Transform optionsParent;
        [SerializeField] private GameObject optionPrefab;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Vector2.Distance(eventData.pressPosition, eventData.position) / Screen.width < 0.03f)
                ScreenManager.Instance.Back();
        }

        public override void Show(params object[] args)
        {
            var data = args[0] as OptionsViewArgs;
            foreach (Transform child in optionsParent) Destroy(child.gameObject);
            foreach (var option in data.options)
            {
                var go = Instantiate(optionPrefab, optionsParent);
                go.GetComponent<OptionsElement>().Populate(option);
            }
        }
    }
}