using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TaxiGameController : MonoBehaviour
{
    public bool disableStartupSequence = false;

    public GameObject victoryScreen;
    public TextMeshProUGUI finalScore;
    public GameObject failureScreen;
    public GameObject pressAnyButtonToContinue;
    public Canvas startScreen;
    public Canvas gameScreen;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI gameTimerText;

    public float loseDelaySetting;
    public float gameTimerDuration;
    private float gameTimeRemaining;

    public Vector3 spawnPos;
    public Vector3 spawnRot;

    private int RACING = 0;
    private int START = 1;
    private int END = 2;
    private int LOSE = 3;
    private int COUNTDOWN = 4;

    private int state;
    private float endWait = 3;
    private float countdown = 3;
    private float countdownDelay = 1;
    private float loseDelay = 5;

    private string tppXAxis;
    private string tppYAxis;

    public GameObject player;


    // Start is called before the first frame update
    void Start()
    {
        GameObject cinemachine = GameObject.FindGameObjectWithTag("Player Camera");
        if (cinemachine == null)
        {
            Debug.Log("Can't find third person camera in scene!");
        }
        tppYAxis = cinemachine.GetComponent<CinemachineFreeLook>().m_YAxis.m_InputAxisName;
        tppXAxis = cinemachine.GetComponent<CinemachineFreeLook>().m_XAxis.m_InputAxisName;

        startScreen.enabled = true;
        gameScreen.enabled = false;
        victoryScreen.SetActive(false);
        failureScreen.SetActive(false);
        pressAnyButtonToContinue.SetActive(false);
        gameTimeRemaining = gameTimerDuration;
        if (disableStartupSequence)
        {
            countdownText.enabled = false;
            GoToRacing();
        }
        else
        {
            GoToStart();
        }
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            Debug.Log("No keyboard detected!");
            return;
        }

        if (keyboard.pageUpKey.wasPressedThisFrame)
        {
            Defeat();
        }

        if (keyboard.endKey.wasPressedThisFrame)
        {
            gameTimeRemaining = 3;
        }

        if (state == RACING)
        {
            
            DoRacing();

        }
        else if (state == END)
        {
            DoEnd();
        }
        else if (state == START)
        {
            if (keyboard.anyKey.wasPressedThisFrame)
            {
                state = COUNTDOWN;
                gameScreen.enabled = true;
                startScreen.enabled = false;
                countdownText.enabled = true;
            }
        }
        else if (state == COUNTDOWN)
        {
            countdown -= Time.deltaTime;
            countdownText.text = ((int)Mathf.Ceil(countdown)).ToString();
            if (countdown <= 0)
            {
                countdownText.text = "GO!";
                EnableControl();
                state = RACING;
            }
        }
        else if (state == LOSE)
        {
            DoLose();
        }
        if (state == RACING || state == LOSE)
        {
            gameTimeRemaining -= Time.deltaTime;
            gameTimerText.text = TaxiManager.FormatTime(gameTimeRemaining);
        }

        if (gameTimeRemaining <= 0 && state != END)
        {
            GoToEnd();
        }
    }

    private void DoRacing()
    {
        // Delay the countdown for a second while the GO text is on screen
        if (countdownDelay > 0)
        {
            countdownDelay -= Time.deltaTime;
            if (countdownDelay < 0)
            {
                countdownText.enabled = false;
            }
        }
    }

    private void DoLose()
    {
        loseDelay -= Time.deltaTime;
        if (loseDelay > 0)
        {
            return;
        }
        failureScreen.SetActive(false);
        var cd = player.GetComponent<CarDurability>();
        cd.Durability = cd.maxDurability;
        // Send player back to spawn
        player.transform.position = spawnPos;
        player.transform.rotation = Quaternion.Euler(spawnRot);
        // Reset their score
        TaxiManager.Instance.scoreManager.FullReset();
        GoToRacing();
    }

    private void DoEnd()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            Debug.Log("No keyboard detected!");
            return;
        }
        if (endWait <= 0)
        {
            pressAnyButtonToContinue.SetActive(true);

            if (keyboard.anyKey.wasPressedThisFrame)
            {
                SceneManager.LoadScene(0);
            }
        }
        else
        {
            endWait -= Time.deltaTime;
        }
    }

    private void GoToStart()
    {
        state = START;

        startScreen.enabled = true;
        gameScreen.enabled = false;

        DisableControl();
    }

    private void GoToRacing()
    {
        state = RACING;

        startScreen.enabled = false;
        gameScreen.enabled = true;

        EnableControl();
    }

    private void GoToLose()
    {
        state = LOSE;

        failureScreen.SetActive(true);
        TaxiManager.Instance.ExpirePassenger();
        loseDelay = loseDelaySetting;

        DisableControl();
    }

    private void GoToEnd()
    {
        state = END;

        victoryScreen.SetActive(true);
        TaxiManager.Instance.EndGame();
        finalScore.text = TaxiManager.Instance.leaderboardManager.GetScore(0).ToString();

        DisableControl();
    }

    public void Defeat()
    {
        Debug.Log("Car destroyed!");
        GoToLose();
    }

    public void DisableControl()
    {
        player.GetComponent<PrometeoCarController>().enabled = false;
        player.GetComponent<PlayerCarControl>().enabled = false;

        GameObject cinemachine = GameObject.FindGameObjectWithTag("Player Camera");
        if (cinemachine == null)
        {
            Debug.Log("Can't find third person camera in RaceController!");
        }
        cinemachine.GetComponent<CinemachineFreeLook>().m_YAxis.m_InputAxisName = "";
        cinemachine.GetComponent<CinemachineFreeLook>().m_XAxis.m_InputAxisName = "";
    }

    public void EnableControl()
    {
        player.GetComponent<PrometeoCarController>().enabled = true;
        player.GetComponent<PlayerCarControl>().enabled = true;

        GameObject cinemachine = GameObject.FindGameObjectWithTag("Player Camera");
        if (cinemachine == null)
        {
            Debug.Log("Can't find third person camera in RaceController!");
        }
        cinemachine.GetComponent<CinemachineFreeLook>().m_YAxis.m_InputAxisName = tppYAxis;
        cinemachine.GetComponent<CinemachineFreeLook>().m_XAxis.m_InputAxisName = tppXAxis;
    }
}
