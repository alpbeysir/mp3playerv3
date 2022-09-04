using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace MP3Player.Misc
{
    public class ButtonTween : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private Transform target;
        [SerializeField] private float scale = 0.9f;
        public new bool enabled = true;
        private bool tweening;
        public void OnPointerDown(PointerEventData eventData)
        {
            if (enabled)
                StartCoroutine(WaitAndAnimate(0.05f));
        }

        private IEnumerator WaitAndAnimate(float time)
        {
            Vector2 firstPos = Input.mousePosition;
            yield return new WaitForSecondsRealtime(time);
            if (Vector2.Distance(firstPos, Input.mousePosition) < Screen.dpi * 0.1f && !tweening)
            {
                target.DOScale(scale, 0.1f).OnComplete(() => tweening = false);
                tweening = true;
            }
        }

        public async void OnPointerUp(PointerEventData eventData)
        {
            await UniTask.WaitUntil(() => !tweening);
            if (enabled)
            {
                target.DOScale(1f, 0.1f).OnComplete(() => tweening = false);
                tweening = true;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (enabled && !tweening)
            {
                target.DOScale(1f, 0.1f).OnComplete(() => tweening = false);
                tweening = true;
            }
        }
    }
}