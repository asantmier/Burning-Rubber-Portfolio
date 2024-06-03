using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(PrometeoCarController))]
public class CarDurability : MonoBehaviour
{
    public float maxDurability;
    // This setting determines the most damage one collision can deal, preventing one shots
    [Range(0f, 1f)]
    public float maxPercentDamagePerCollision = 0.2f;
    private float durability;

    public float maxWear;
    private float wear;

    public Slider durabilitySlider;
    public Slider wearSlider;

    private WheelCollider frontLeftCollider;
    private WheelCollider frontRightCollider;
    private WheelCollider rearLeftCollider;
    private WheelCollider rearRightCollider;

    private PrometeoCarController pcc;
    private Rigidbody rb;

    private float defaultStiffness;
    public float minStiffness;

    public float slipThreshold;
    public float forwardSlipMult;
    public float sidewaysSlipMult;

    public int crashCost = 2000;

    public float Durability { get => durability; 
        set
        {
            durability = Mathf.Clamp(value, 0, maxDurability);
            durabilitySlider.value = durability / maxDurability;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        durability = maxDurability;
        wear = 0;

        pcc = GetComponent<PrometeoCarController>();
        rb = GetComponent<Rigidbody>();

        frontLeftCollider = pcc.frontLeftCollider;
        frontRightCollider = pcc.frontRightCollider;
        rearLeftCollider = pcc.rearLeftCollider;
        rearRightCollider = pcc.rearRightCollider;

        defaultStiffness = frontLeftCollider.sidewaysFriction.stiffness;
    }

    // Update is called once per frame
    void Update()
    {
        WheelHit FLWheelData;
        frontLeftCollider.GetGroundHit(out FLWheelData);
        if (FLWheelData.forwardSlip > slipThreshold)
        {
            //Debug.Log(FLWheelData.forwardSlip);
            wear += FLWheelData.forwardSlip * forwardSlipMult;
        }
        if (FLWheelData.sidewaysSlip > slipThreshold)
        {
            //Debug.Log(FLWheelData.sidewaysSlip);
            wear += FLWheelData.sidewaysSlip * sidewaysSlipMult;
        }
        // Lets look at slip as rates like slip per second. Rate of change in slip might be a better metric than slip itself.
        wearSlider.value = 1 - (wear / maxWear);

        //float localVelocityX = pcc.transform.InverseTransformDirection(rb.velocity).x;
        //if ((pcc.isTractionLocked || Mathf.Abs(localVelocityX) > 5f) && Mathf.Abs(pcc.carSpeed) > 12f)
        //{
        //    // The car is skidding in PCC

        //} else if (pcc.isDrifting && Mathf.Abs(pcc.carSpeed) > 12f)
        //{
        //    // The car is just drifting

        //}

        float stiffness = Mathf.Lerp(minStiffness, defaultStiffness, 1 - wear / maxWear);
        WheelFrictionCurve curve;
        curve = frontLeftCollider.sidewaysFriction;
        curve.stiffness = stiffness;
        frontLeftCollider.sidewaysFriction = curve;
        curve = frontLeftCollider.forwardFriction;
        curve.stiffness = stiffness;
        frontLeftCollider.forwardFriction = curve;

        curve = frontRightCollider.sidewaysFriction;
        curve.stiffness = stiffness;
        frontRightCollider.sidewaysFriction = curve;
        curve = frontRightCollider.forwardFriction;
        curve.stiffness = stiffness;
        frontRightCollider.forwardFriction = curve;

        curve = rearLeftCollider.sidewaysFriction;
        curve.stiffness = stiffness;
        rearLeftCollider.sidewaysFriction = curve;
        curve = rearLeftCollider.forwardFriction;
        curve.stiffness = stiffness;
        rearLeftCollider.forwardFriction = curve;

        curve = rearRightCollider.sidewaysFriction;
        curve.stiffness = stiffness;
        rearRightCollider.sidewaysFriction = curve;
        curve = rearRightCollider.forwardFriction;
        curve.stiffness = stiffness;
        rearRightCollider.forwardFriction = curve;
    }

    public void Damage(float amount)
    {
        Durability -= amount;

        if (durability <= 0)
        {
            RaceController rc = FindObjectOfType<RaceController>();
            if (rc == null)
            {
                Debug.LogError("Couldn't find race controller from car durability script!");
            } else
            {
                rc.Lose();
                return;
            }

            TaxiGameController tgc = FindObjectOfType<TaxiGameController>();
            if (tgc == null)
            {
                Debug.LogError("Couldn't find taxi manager from car durability script!");
            } else
            {
                tgc.Defeat();
            }
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        float damageScale = 1f;
        Obstacle obstacle = collision.gameObject.GetComponent<Obstacle>();
        if (obstacle != null)
        {
            damageScale = obstacle.crashForceScale;
        }
        Debug.Log(string.Format("Collision! Force: {0}, Vecloity {1}, Scale {2}.", (collision.impulse / Time.fixedDeltaTime).magnitude, collision.relativeVelocity.magnitude, damageScale));
        Damage(Mathf.Clamp(collision.relativeVelocity.magnitude * damageScale, 0, maxPercentDamagePerCollision * maxDurability));
        TaxiManager.Instance.scoreManager.AddPenaltyInstant("Crash!", crashCost);
    }
}
