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

    public void Init(Action<double> progressListener)
    {
        loadingImage.fillAmount = 0;
        percentageText.text = string.Format("%{0}", 0);
        progressListener += OnProgressChanged;
    }

    private void OnProgressChanged(double progress)
    {
        loadingImage.fillAmount = Mathf.Clamp01((float)progress);
        percentageText.text = string.Format("%{0}", progress);
    }
}
