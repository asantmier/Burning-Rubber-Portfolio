using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpBonus : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerCarControl cd = other.GetComponentInParent<PlayerCarControl>();
        if (cd != null)
        {
            cd.AddJump();
            Destroy(gameObject);
        }
    }
}
