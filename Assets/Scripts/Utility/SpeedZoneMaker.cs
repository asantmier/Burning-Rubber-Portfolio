using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedZoneMaker : MonoBehaviour
{
    public GameObject prefab;
    public float distance;
    public float number;

    // Start is called before the first frame update
    void OnEnable()
    {
        for (int i = 1; i < number; i++)
        {
            Instantiate(prefab, transform.position + Vector3.forward * i * distance, Quaternion.identity, transform);
        }
    }
}
