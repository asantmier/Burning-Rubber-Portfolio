using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostBonus : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerCarControl cd = other.GetComponentInParent<PlayerCarControl>();
        if (cd != null)
        {
            cd.AddBoost();
            Destroy(gameObject);
        }
    }
}
