using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIGoal : MonoBehaviour
{
    public AIGoal nextWaypoint;
    public float targetSpeed = 80;

    private void OnTriggerEnter(Collider other)
    {
        CarAI cai = other.GetComponentInParent<CarAI>();
        if (cai != null)
        {
            cai.ReachedGoal(nextWaypoint);
        }
    }
}
