using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// IMPORTANT the same goal and passenger are reused throughout the scene, so this script is never re-initialized
public class Passenger : MonoBehaviour
{
    PlayerCarControl tracked;

    private void OnEnable()
    {
        tracked = null;
    }

    private void Update()
    {
        if (tracked == null) return;
        TestParked();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            tracked = other.GetComponentInParent<PlayerCarControl>();
            TestParked();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            tracked = null;
        }
    }

    private void TestParked()
    {
        // Wait for the car to completely stop
        if (tracked.Speed() > 5)
        {
            return;
        }
        var tm = TaxiManager.Instance;
        if (tm.TryPickUpPassenger())
        {
            // turn off this passenger
            tracked = null;
            gameObject.SetActive(false);
        }
    }
}
