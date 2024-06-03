using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class IndicatorLight : MonoBehaviour
{
    private bool on = false;

    public Color offColor;
    public Color onColor; 

    private void Start()
    {
        On = on;
    }

    public bool On { get => on; set {
            on = value;
            if (on)
            {
                GetComponent<Image>().color = onColor;
            } else
            {
                GetComponent<Image>().color = offColor;
            }
        } 
    }
}
