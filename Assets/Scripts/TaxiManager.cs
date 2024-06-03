using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TaxiManager : MonoBehaviour
{
    private static TaxiManager _instance;
    public static TaxiManager Instance { get { return _instance; } }

    public IndicatorLight hiredIndicator;
    public IndicatorLight vacantIndicator;
    public IndicatorLight stoppedIndicator;
    public TextMeshProUGUI passengerTimerText;
    public TextMeshProUGUI baseFareText;
    public ScoreManager scoreManager;
    public Leaderboard leaderboardManager;
    public Minimap minimap;

    public GameObject passenger;
    public GameObject goal;
    public GameObject taxiPoints;
    public float farePerMeter = 20;
    public float timePerMeter = 0.5f;
    public int bonusPointsPerSecond = 10;

    public float maxPickupDistance = 500f;
    public float minDropoffDistance = 100f;

    private int passengerFare;
    private float passengerGoalTime;

    private float passengerTimeRemaining;

    private bool hasPassenger;

    public GameObject player;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.Log("Singleton time :)");
            Destroy(gameObject);
        } else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        Indicators(false, true, false);
        SetPassengerTimer();

        PlacePassenger();

        leaderboardManager.SetPlayerCount(1);
        scoreManager.DoScoring(false);
        baseFareText.text = "0";
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            Debug.Log("No keyboard detected!");
            return;
        }

        if (keyboard.pKey.wasPressedThisFrame)
        {
            scoreManager.AddBonusInstant("Cool", 1200);
        }

        if (hasPassenger)
        {
            passengerTimeRemaining -= Time.deltaTime;
            if (passengerTimeRemaining <= 0)
            {
                ExpirePassenger();
            }
        }
        SetPassengerTimer();
    }

    public void PlacePassenger()
    {
        Debug.Log("Passenger and goal placed");
        int p;
        do
        {
            p = Random.Range(0, taxiPoints.transform.childCount);
        } 
        while (Vector3.Distance(player.transform.position, taxiPoints.transform.GetChild(p).position) > maxPickupDistance);
        Transform passLoc = taxiPoints.transform.GetChild(p);
        int g;
        do
        {
            g = Random.Range(0, taxiPoints.transform.childCount);
        } 
        while (g == p || Vector3.Distance(passLoc.position, taxiPoints.transform.GetChild(g).position) < minDropoffDistance);
        Transform goalLoc = taxiPoints.transform.GetChild(g);
        passenger.transform.position = passLoc.position;
        goal.transform.position = goalLoc.position;
        passenger.SetActive(true);
        goal.SetActive(false);
        float distance = Vector3.Distance(passenger.transform.position, goal.transform.position);
        passengerFare = (int)(farePerMeter * distance) - (int)(farePerMeter * distance) % 10;
        passengerGoalTime = (int)(timePerMeter * distance);
        minimap.SetPassenger(passenger.transform.position);
    }

    public bool TryPickUpPassenger()
    {
        if (hasPassenger)
        {
            return false;
        }
        Debug.Log("Passenger picked up");

        hasPassenger = true;

        Indicators(true, false, false);
        baseFareText.text = passengerFare.ToString();
        passengerTimeRemaining = passengerGoalTime;
        SetPassengerTimer();
        goal.SetActive(true);
        minimap.HidePassenger();
        minimap.SetGoal(goal.transform.position);

        scoreManager.DoScoring(true);
        leaderboardManager.AddScore(0, passengerFare);

        return true;
    }

    public bool TryDeliverPassenger()
    {
        if (!hasPassenger)
        {
            return false;
        }
        Debug.Log("Passenger delivered");

        hasPassenger = false;

        Indicators(false, true, false);
        minimap.HideGoal();

        int timeBonus = (int)passengerTimeRemaining * bonusPointsPerSecond;
        scoreManager.AddBonusInstant("Bonus Time", timeBonus);
        //leaderboardManager.AddScore(0, timeBonus);
        int styleScore = scoreManager.GetScore();
        scoreManager.ResetScore();
        scoreManager.DoScoring(false);
        leaderboardManager.AddScore(0, styleScore);

        baseFareText.text = "0";

        PlacePassenger();

        return true;
    }

    public void ExpirePassenger()
    {
        Debug.Log("Passenger expired");

        hasPassenger = false;

        Indicators(false, true, false);
        minimap.HideGoal();

        scoreManager.ResetScore();
        scoreManager.DoScoring(false);

        baseFareText.text = "0";

        PlacePassenger();
    }

    private void Indicators(bool hire, bool vacant, bool stop)
    {
        hiredIndicator.On = hire;
        vacantIndicator.On = vacant;
        stoppedIndicator.On = stop;
    }

    private void SetPassengerTimer()
    {
        if (hasPassenger)
        {
            passengerTimerText.text = FormatTime(passengerTimeRemaining);
        } else
        {
            //passengerTimerText.text = "--:--";
            passengerTimerText.text = "Find a passenger!";
        }
        
    }

    public void EndGame()
    {
        ExpirePassenger();
    }

    public static string FormatTime(float t)
    {
        if (t > 60)
        {
            int minutes = (int)(t / 60);
            int seconds = (int)(t % 60);
            return string.Format("{0}:{1}", minutes, seconds.ToString("d2"));
        } else
        {
            int seconds = (int)t;
            int milliseconds = (int)((t - seconds) * 100);
            return string.Format("{0}.{1}", seconds, milliseconds.ToString("d2"));
        }
    }
}
