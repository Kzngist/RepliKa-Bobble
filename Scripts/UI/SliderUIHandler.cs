using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderUIHandler : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TMP_Text text;

    void Start()
    {
        text.text = $"{slider.value:0.#}";
    }

    public void OnValueChange(float value)
    {
        text.text = $"{value:0.#}";
    }
}
