using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SubTriggerHelper : MonoBehaviour
{
    public UnityEvent triggerEntered;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            triggerEntered.Invoke();
        }
    }
}
