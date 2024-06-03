using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class AbilityChargeGauge : MonoBehaviour
{
    public Gradient gradient;

    private Slider slider;
    private Image fill;

    public void OnValueChanged(float value)
    {
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }
        if (fill == null)
        {
            fill = slider.fillRect.GetComponent<Image>();
        }

        fill.color = gradient.Evaluate(value);
    }
}
