using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(PrometeoCarController))]
public class PlayerCarControl : MonoBehaviour
{
    // Boost
    public float boostDuration;
    public int speedBoost;
    public int accelerationBoost;
    public int maxBoostUses = 2;
    public float boostChargeDuration;
    // Jump
    public float jumpForce;
    public int maxJumpUses = 1;
    public float jumpChargeDuration;
    // Air Control
    public float gyroTorque = 100;

    // Boost Trail
    public TrailRenderer leftTrail;
    public TrailRenderer rightTrail;

    // HUD
    public Speedometer speedometer;
    public Slider boostSlider;
    public Slider boostSlider2;
    public Slider jumpSlider;

    public float flipForce = 20;

    public float airDashForce = 20000;

    PrometeoCarController pcc;
    Rigidbody rb;
    WheelCollider wheelCollider;

    private int boostUses;
    private int jumpUses;
    private bool boosting = false;
    private bool decelerating = false;

    private float boostChargingCountdown;
    private float jumpChargingCountdown;

    private bool inBoostZone = false;

    private bool isFlipped;
    private float groundCheckDistance;
    private Vector3 groundCheckPosition;

    // Start is called before the first frame update
    void Start()
    {
        pcc = GetComponent<PrometeoCarController>();
        rb = GetComponent<Rigidbody>();
        wheelCollider = GetComponentInChildren<WheelCollider>();
        leftTrail.emitting = false;
        rightTrail.emitting = false;

        boostSlider.gameObject.SetActive(true);
        //boostSlider.value = 1f;
        boostSlider2.gameObject.SetActive(false);

        jumpSlider.gameObject.SetActive(true);
        jumpSlider.value = 1f;

        boostChargingCountdown = boostChargeDuration;
        jumpChargingCountdown = jumpChargeDuration;

        groundCheckPosition = GetComponentInChildren<BoxCollider>().center;
    }

    // Update is called once per frame
    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            Debug.Log("No keyboard detected!");
            return;
        }

        groundCheckDistance = GetComponentInChildren<BoxCollider>().bounds.extents.y + 0.1f;

        Debug.DrawRay(transform.TransformPoint(groundCheckPosition), Vector3.down * groundCheckDistance, Color.red, 0.1f) ;
        isFlipped = Physics.Raycast(transform.TransformPoint(groundCheckPosition), Vector3.down, groundCheckDistance, ~LayerMask.GetMask("Player"));

        // Recharge the boost
        if (boostChargingCountdown > 0 && boostUses < maxBoostUses && !boosting)
        {
            // If you have a boost already, the bar that fiils should be a different one
            // Or the same one but when you have a boost you underlay a bar
            boostChargingCountdown -= Time.deltaTime;
            // Fill current slider
            if (boostUses < 1)
            {
                boostSlider.gameObject.SetActive(true);
                boostSlider2.gameObject.SetActive(false);
                boostSlider.value = 1 - (boostChargingCountdown / boostChargeDuration);
            } else
            {
                boostSlider2.gameObject.SetActive(true);
                boostSlider2.value = 1 - (boostChargingCountdown / boostChargeDuration);
            }
            // It would be cool to have an effect when you reach 100% to indicate charge (think ultimates in shooters)
            // We just finished charging a boost
            if (boostChargingCountdown <= 0)
            {
                boostUses += 1;
                // Reset the charging countdown
                boostChargingCountdown = boostChargeDuration;
            }
        }

        if (keyboard.leftShiftKey.wasPressedThisFrame && boostUses > 0)
        {
            if (wheelCollider.isGrounded && !boosting)
            {
                StartCoroutine("Boost");
            } 
            else if (!wheelCollider.isGrounded && !isFlipped) 
            {
                rb.AddForce(airDashForce * transform.forward, ForceMode.Impulse);
                boostUses -= 1;
            }
            
        }

        // Stop the boost deceleration if we've decelerated enough
        if (decelerating && pcc.carSpeed <= pcc.maxSpeed)
        {
            decelerating = false;
            CancelInvoke("DecelerateOverride");
        }
        
        // Recharge jump if grounded
        if (jumpChargingCountdown > 0 && jumpUses < maxJumpUses && wheelCollider.isGrounded)
        {
            jumpChargingCountdown -= Time.deltaTime;
            jumpSlider.value = 1 - (jumpChargingCountdown / jumpChargeDuration);

            if (jumpChargingCountdown <= 0)
            {
                jumpUses += 1;
                jumpChargingCountdown = jumpChargeDuration;
            }
        }

        if (keyboard.eKey.wasPressedThisFrame && jumpUses > 0)
        {
            // Check if grounded
            if (/*wheelCollider.isGrounded*/ true)
            {
                Vector3 force = Vector3.up * jumpForce;
                // Check if we're in a boost zone
                if (inBoostZone)
                {
                    force += transform.forward * jumpForce;
                    TaxiManager.Instance.scoreManager.AddBonusInstant("Jump Boost!", 400);
                }
                rb.AddForce(force, ForceMode.Impulse);
                jumpUses -= 1;
                jumpSlider.value = 0;
            }
        }

        // If the player isn't grounded, give them control over their movement, and pull them towards the center
        if (!wheelCollider.isGrounded)
        {
            var torque = gyroTorque;
            if (isFlipped /*&& Vector3.Dot(transform.up, Vector3.down) > 0*/)
            {
                torque *= flipForce;
            }
            Vector3 dir = Vector3.zero;
            if (keyboard.sKey.isPressed)
            {
                dir -= transform.right;
            }
            if (keyboard.wKey.isPressed)
            {
                dir += transform.right;
            }
            if (keyboard.aKey.isPressed)
            {
                dir += transform.forward;
            }
            if (keyboard.dKey.isPressed)
            {
                dir -= transform.forward;
            }
            rb.AddTorque(dir * torque * Time.deltaTime);
        }

        // TODO: PCC's car speed is actually wheel RPM
        //speedometer.SetSpeed(pcc.carSpeed);
        // This method converts the transform's velocity into mph
        speedometer.SetSpeed(Mathf.Abs(transform.InverseTransformDirection(rb.velocity).z * 3.6f / 1.609f));
        //Debug.Log("PCC internal: "+pcc.carSpeed);
        //Debug.Log("Speedometer: "+speedometer.speedText.text);
        //Debug.Log("Transform: " + (transform.InverseTransformDirection(rb.velocity).z * 3.6));
    }

    // Returns speed in mph
    public float Speed()
    {
        return Mathf.Abs((transform.InverseTransformDirection(rb.velocity).z * 3.6f) / 1.609f);
    }

    // Manually call decelerate from here to avoid PCC canceling deceleration with its internals
    void DecelerateOverride()
    {
        pcc.DecelerateCar();
    }

    IEnumerator Boost()
    {
        // start
        boosting = true;

        // Swap sliders
        boostSlider.value = boostSlider2.value;
        boostSlider2.value = 1;
        // Enable the trail
        leftTrail.emitting = true;
        rightTrail.emitting = true;
        // Stop the boost deceleration if it's happening
        decelerating = false;
        CancelInvoke("DecelerateOverride");

        int tempSpeed = pcc.maxSpeed;
        int tempAcceleration = pcc.accelerationMultiplier;
        pcc.maxSpeed = pcc.maxSpeed + speedBoost;
        pcc.accelerationMultiplier = pcc.accelerationMultiplier + accelerationBoost;
        float timer = 0.0f;
        while (timer < boostDuration)
        {
            pcc.GoForward();
            // Make slider drain
            boostSlider2.value = 1 - (timer / boostDuration);
            yield return null;
            timer += Time.deltaTime;
        }
        pcc.maxSpeed = tempSpeed;
        pcc.accelerationMultiplier = tempAcceleration;

        // Decelerate from our speed that's higher than normal
        if (pcc.carSpeed > pcc.maxSpeed)
        {
            InvokeRepeating("DecelerateOverride", 0f, 0.1f);
            decelerating = true;
        }
        // Disable the trail
        leftTrail.emitting = false;
        rightTrail.emitting = false;

        // end
        boostUses -= 1;
        boosting = false;
    }

    public void SetBoostZone(bool inZone)
    {
        inBoostZone = inZone;
    }

    public bool IsFlipped()
    {
        return isFlipped;
    }

    public void AddJump()
    {
        if (jumpUses >= maxJumpUses)
        {
            return;
        }

        jumpUses += 1;
        jumpSlider.value = 1;
        jumpChargingCountdown = jumpChargeDuration;
    }

    public void AddBoost()
    {
        if (boostUses >= maxBoostUses)
        {
            return;
        }
        if (boostUses < 1)
        {
            boostSlider.gameObject.SetActive(true);
            boostSlider2.gameObject.SetActive(true);
            boostSlider.value = 1;
            boostSlider2.value = 1 - (boostChargingCountdown / boostChargeDuration);
        }
        else
        {
            boostSlider2.gameObject.SetActive(true);
            boostSlider2.value = 1;
            boostChargingCountdown = boostChargeDuration;
        }
        boostUses += 1;
    }
}
