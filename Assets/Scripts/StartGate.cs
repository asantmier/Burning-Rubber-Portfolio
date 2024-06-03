using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StartGate : MonoBehaviour
{
    //public TextMeshProUGUI text;
    private RaceController raceController;
    private int lap = 1;

    private bool lapStarted = false;
    private bool lapFinished = false;

    // Start is called before the first frame update
    void Start()
    {
        raceController = GameObject.FindObjectOfType<RaceController>();
        if (raceController == null)
        {
            Debug.LogError("Couldn't find a race controller to attach to start gate!");
        }

        lap = 1;
        lapStarted = false;
        lapFinished = false;
        raceController.SetLap(1);
        //text.text = lap.ToString();
    }

    public void LapStarted()
    {
        Debug.Log("Crossed lap start trigger");
        lapStarted = true;
    }

    public void GateCrossed()
    {
        if (lapStarted && lapFinished)
        {
            Debug.Log("Completed a lap.");
            lap += 1;
            lapStarted = false;
            lapFinished = false;
            raceController.SetLap(lap);
            //text.text = lap.ToString();
        }
    }

    public void LapFinished()
    {
        Debug.Log("Crossed lap finish trigger.");
        lapFinished = true;
    }
}
