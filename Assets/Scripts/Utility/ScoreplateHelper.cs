using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreplateHelper : MonoBehaviour
{
    public TextMeshProUGUI nameplate;
    public TextMeshProUGUI timeplate;

    public void Setup(string name, string time)
    {
        nameplate.text = name;
        timeplate.text = time;
    }
}
