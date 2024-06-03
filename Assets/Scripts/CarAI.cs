using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PrometeoCarController))]
public class CarAI : MonoBehaviour
{
    public float turnApproximation = 2f;
    public float speedApproximation = 10f;
    public AnimationCurve steerSpeedCurve = AnimationCurve.Constant(0, 1, 1f);
    public AIGoal goal;
    public LineRenderer lineRenderer;

    NavMeshAgent agent;
    PrometeoCarController pcc;
    NavMeshPath path;

    bool decelerating = false;

    private float speed = 0;
    private float dSpeed = 0;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        pcc = GetComponent<PrometeoCarController>();

        if (!agent.isOnNavMesh)
        {
            NavMeshHit hit;
            NavMesh.SamplePosition(transform.position, out hit, 3f, agent.areaMask);
            agent.transform.position = hit.position;
            agent.enabled = false;
            agent.enabled = true;
        }

        path = new NavMeshPath();
    }

    // Update is called once per frame
    void Update()
    {

        // Smooth out our speed using the same technique as Speedometer.cs
        speed = Mathf.SmoothDamp(speed, pcc.carSpeed, ref dSpeed, 0.1f);

        if (goal != null) {
            agent.CalculatePath(goal.transform.position, path);
            lineRenderer.SetPositions(path.corners);

            // Adjust car speed
            //Debug.Log("Car Speed: " + speed + " Target Speed: " + goal.targetSpeed);
            float desiredSpeedMultiplier = steerSpeedCurve.Evaluate(pcc.frontLeftCollider.steerAngle / pcc.maxSteeringAngle);
            if (speed < goal.targetSpeed * desiredSpeedMultiplier)
            {
                CancelInvoke("DecelerateOverride");
                decelerating = false;
                pcc.GoForward();
            } else if (speed > goal.targetSpeed * desiredSpeedMultiplier + speedApproximation)
            {
                CancelInvoke("DecelerateOverride");
                decelerating = false;
                pcc.GoReverse();
            } else
            {
                InvokeRepeating("DecelerateOverride", 0f, 0.1f);
                decelerating = true;
            }

            // TODO We could probably scale steering speed over distance
            // Adjust steering angle
            Vector3 nextLocation = path.corners[1];
            float turnAngle = Vector3.SignedAngle(transform.forward, nextLocation - transform.position, Vector3.up);
            //Debug.Log("Turn angle: " + turnAngle);
            // Use the tire's steering angle for precise movement
            if (turnAngle > pcc.frontLeftCollider.steerAngle)
            {
                pcc.TurnRight();
            }
            else if (turnAngle < pcc.frontLeftCollider.steerAngle)
            {
                pcc.TurnLeft();
            }
            else
            {
                pcc.ResetSteeringAngle();
            }
        } else
        {
            // Return to resting position
            pcc.ResetSteeringAngle();
            if (!decelerating)
            {
                InvokeRepeating("DecelerateOverride", 0f, 0.1f);
                decelerating = true;
            }
            
        }
        
    }

    // Grabbed from PlayerCarControl.cs
    // Manually call decelerate from here to avoid PCC canceling deceleration with its internals
    void DecelerateOverride()
    {
        pcc.DecelerateCar();
    }

    public void ReachedGoal(AIGoal next)
    {
        goal = next;
    }
}
