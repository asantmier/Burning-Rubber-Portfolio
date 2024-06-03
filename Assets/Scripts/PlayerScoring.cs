using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerCarControl))]
public class PlayerScoring : MonoBehaviour
{
    public float highSpeed = 60;
    public int highSpeedPointsPerSecond = 100;
    public float crazySpeed = 80;
    public int crazySpeedPointsPerSecond = 250;
    public float exSpeed = 100;
    public int exSpeedPointsPerSecond = 500;
    public float speedExpirationTime = 2f;

    public int airtimePointsPerSecond = 200;
    public float minAirtime = 1f;

    public int driftPointsPerSecond = 100;
    public float crazyDriftSlideSpeed = 10;
    public int crazyDriftPointsPerSecond = 200;
    public float exDriftSlideSpeed = 20;
    public int exDriftPointsPerSecond = 300;
    public float driftExpirationTime = 2f;

    TaxiManager tm;
    ScoreManager scoreManager;
    PlayerCarControl carControl;
    PrometeoCarController pcc;
    Rigidbody rb;
    WheelCollider wheelCollider;

    bool airborne = false;
    float airborneStartTime;
    int airBonusId;

    bool drifting = false;
    float lastDriftTime;
    int driftBonusId;

    bool speeding = false;
    float lastSpeedtime;
    int speedBonusId;

    // Start is called before the first frame update
    void Start()
    {
        tm = TaxiManager.Instance;
        scoreManager = tm.scoreManager;
        carControl = GetComponent<PlayerCarControl>();
        pcc = GetComponent<PrometeoCarController>();
        rb = GetComponent<Rigidbody>();
        wheelCollider = GetComponentInChildren<WheelCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        float speed = carControl.Speed();
        if (speed >= highSpeed)
        {
            speeding = true;
            lastSpeedtime = Time.time;
            int pps;
            if (speed >= exSpeed)
            {
                pps = exSpeedPointsPerSecond;
            }
            else if (speed < exSpeed && speed >= crazySpeed)
            {
                pps = crazySpeedPointsPerSecond;
            }
            else
            {
                pps = highSpeedPointsPerSecond;
            }
            if (scoreManager.IsBonusRegistered(speedBonusId))
            {
                scoreManager.UpdateContinuousBonus(speedBonusId, BonusType.SPEED, pps * Time.deltaTime);
            } 
            else
            {
                speedBonusId = scoreManager.OpenContinuousBonus(BonusType.SPEED, pps);
            }
        } else if (speeding)
        {
            if (Time.time - lastSpeedtime > speedExpirationTime)
            {
                speeding = false;
                scoreManager.CloseContinousBonus(speedBonusId);
            }
        }

        if (!airborne && !wheelCollider.isGrounded && !carControl.IsFlipped())
        {
            airborne = true;
            airborneStartTime = Time.time;
        }
        else if (airborne && !wheelCollider.isGrounded && !carControl.IsFlipped())
        {
            if (Time.time - airborneStartTime > minAirtime)
            {
                if (!scoreManager.IsBonusRegistered(airBonusId))
                {
                    airBonusId = scoreManager.OpenContinuousBonus(BonusType.AIR, airtimePointsPerSecond);
                }
                else
                {
                    scoreManager.UpdateContinuousBonus(airBonusId, BonusType.AIR, airtimePointsPerSecond * Time.deltaTime);
                }
            }
            
        }
        else if (airborne && (wheelCollider.isGrounded || carControl.IsFlipped()))
        {
            airborne = false;
            if (scoreManager.IsBonusRegistered(airBonusId))
            {
                scoreManager.CloseContinousBonus(airBonusId);
            }
        }

        // Maybe scale drifting score off of forward speed as well
        if (pcc.isDrifting && !airborne)
        {
            drifting = true;
            lastDriftTime = Time.time;
            float drift = Mathf.Abs(transform.InverseTransformDirection(rb.velocity).x);
            //Debug.Log(drift);
            int pps;
            if (drift >= crazyDriftSlideSpeed && drift < exDriftSlideSpeed)
            {
                pps = crazyDriftPointsPerSecond;
            }
            else if (drift >= exDriftSlideSpeed)
            {
                pps = exDriftPointsPerSecond;
            } 
            else
            {
                pps = driftPointsPerSecond;
            }
            if (scoreManager.IsBonusRegistered(driftBonusId))
            {
                scoreManager.UpdateContinuousBonus(driftBonusId, BonusType.DRIFT, pps * Time.deltaTime);
            }
            else
            {
                driftBonusId = scoreManager.OpenContinuousBonus(BonusType.DRIFT, pps);
            }
        } else if (drifting && (!pcc.isDrifting || airborne))
        {
            if (Time.time - lastDriftTime > driftExpirationTime)
            {
                drifting = false;
                scoreManager.CloseContinousBonus(driftBonusId);
            }
        }
    }
}
