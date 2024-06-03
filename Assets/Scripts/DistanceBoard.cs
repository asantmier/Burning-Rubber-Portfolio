using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class DistanceBoard : MonoBehaviour
{
    public Transform parent;
    // Start is called before the first frame update
    void OnEnable()
    {
        GetComponent<TextMeshProUGUI>().text = parent.position.z.ToString() + "m";
    }
}
