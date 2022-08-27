using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LoadingBar : MonoBehaviour
{
    [SerializeField] private Image loadingImage;
    [SerializeField] private TextMeshProUGUI percentageText;

    private float targetAmount;

    public void Init()
    {
        gameObject.SetActive(true);
        loadingImage.fillAmount = 0;
        percentageText.text = string.Format("%{0}", 0);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (loadingImage.fillAmount != targetAmount)
        {
            loadingImage.fillAmount = Mathf.Lerp(loadingImage.fillAmount, targetAmount, 0.15f);
            percentageText.text = string.Format("%{0}", Mathf.CeilToInt(targetAmount * 100f));
        }
    }

    public void SetProgress(float progress)
    {
        targetAmount = Mathf.Clamp01(progress);
    }
}
