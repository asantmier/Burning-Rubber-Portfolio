using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var pcc = other.GetComponentInParent<PlayerCarControl>();
        if (pcc != null)
        {
            pcc.SetBoostZone(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var pcc = other.GetComponentInParent<PlayerCarControl>();
        if (pcc != null)
        {
            pcc.SetBoostZone(false);
        }
    }
}
