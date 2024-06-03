using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.SceneManagement;

public class RaceController : MonoBehaviour
{
    public bool disableStartupSequence = false;

    public TextMeshProUGUI lapText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI lap1Time;
    public TextMeshProUGUI lap2Time;
    public TextMeshProUGUI lap3Time;

    public GameObject victoryScreen;
    public TextMeshProUGUI finalTimeText;
    public GameObject failureScreen;
    public GameObject pressAnyButtonToContinue;
    public Canvas startScreen;
    public Canvas scoreScreen;
    public Canvas gameScreen;
    public TextMeshProUGUI countdownText;
    public Scoreboard scoreboard;

    private int lap;
    private float timer;
    private float timer2;

    private int RACING = 0;
    private int START = 1;
    private int WIN = 2;
    private int LOSE = 3;
    private int SCORE = 4;
    private int COUNTDOWN = 5;

    private int state;
    private float endWait = 3;
    private float countdown = 3;
    private float countdownDelay = 1;
    private float loseDelay = 6;

    private string tppXAxis;
    private string tppYAxis;


    // Start is called before the first frame update
    void Start()
    {
        GameObject cinemachine = GameObject.FindGameObjectWithTag("Player Camera");
        if (cinemachine == null)
        {
            Debug.Log("Can't find third person camera in RaceController!");
        }
        tppYAxis = cinemachine.GetComponent<CinemachineFreeLook>().m_YAxis.m_InputAxisName;
        tppXAxis = cinemachine.GetComponent<CinemachineFreeLook>().m_XAxis.m_InputAxisName;

        scoreScreen.enabled = false;
        startScreen.enabled = true;
        gameScreen.enabled = false;
        victoryScreen.SetActive(false);
        failureScreen.SetActive(false);
        pressAnyButtonToContinue.SetActive(false);
        timer = 0f;
        timer2 = 0f;
        lap1Time.text = "Lap 1: 00:00:00";
        lap2Time.text = "Lap 2: 00:00:00";
        lap3Time.text = "Lap 3: 00:00:00";
        SetLap(1);
        if (disableStartupSequence)
        {
            countdownText.enabled = false;
            GoToRacing();
        } else
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

        if (keyboard.deleteKey.wasPressedThisFrame)
        {
            SetLap(lap + 1);
        }

        if (keyboard.pageUpKey.wasPressedThisFrame)
        {
            Lose();
        }

        if (state == RACING)
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

            timer += Time.deltaTime;
            timer2 += Time.deltaTime;
            timerText.text = string.Format("{0}:{1}:{2}", Mathf.RoundToInt(timer / 60).ToString("D2"), Mathf.RoundToInt(timer % 60).ToString("D2"), Mathf.RoundToInt((timer % 1) * 100).ToString("D2"));
            if (lap == 1)
            {
                lap1Time.text = string.Format("Lap 1: {0}:{1}:{2}", Mathf.RoundToInt(timer2 / 60).ToString("D2"), Mathf.RoundToInt(timer2 % 60).ToString("D2"), Mathf.RoundToInt((timer2 % 1) * 100).ToString("D2"));
            }
            else if (lap == 2)
            {
                lap2Time.text = string.Format("Lap 2: {0}:{1}:{2}", Mathf.RoundToInt(timer2 / 60).ToString("D2"), Mathf.RoundToInt(timer2 % 60).ToString("D2"), Mathf.RoundToInt((timer2 % 1) * 100).ToString("D2"));
            }
            else if (lap == 3)
            {
                lap3Time.text = string.Format("Lap 3: {0}:{1}:{2}", Mathf.RoundToInt(timer2 / 60).ToString("D2"), Mathf.RoundToInt(timer2 % 60).ToString("D2"), Mathf.RoundToInt((timer2 % 1) * 100).ToString("D2"));
            }
        } else if (state == WIN)
        {
            if (endWait <= 0)
            {
                pressAnyButtonToContinue.SetActive(true);

                if (keyboard.anyKey.wasPressedThisFrame)
                {
                    GoToScore();
                }
            } else
            {
                endWait -= Time.deltaTime;
            }
        } else if (state == START)
        {
            if (keyboard.anyKey.wasPressedThisFrame)
            {
                state = COUNTDOWN;
                gameScreen.enabled = true;
                startScreen.enabled = false;
                countdownText.enabled = true;
            }
        } else if (state == COUNTDOWN)
        {
            countdown -= Time.deltaTime;
            countdownText.text = ((int)Mathf.Ceil(countdown)).ToString();
            if (countdown <= 0)
            {
                countdownText.text = "GO!";
                EnableControl();
                state = RACING;
            }
        } else if (state == LOSE)
        {
            loseDelay -= Time.deltaTime;
            if (loseDelay <= 0)
            {
                SceneManager.LoadScene(0);
            }
        }
    }

    public void SetLap(int newLap)
    {
        if (newLap > 3)
        {
            Win();
            return;
        }
        lap = newLap;
        lapText.text = lap.ToString() + " / 3";
        timer2 = 0f;
    }

    private void Win()
    {
        // Check to make sure the game hasn't finished
        if (state == RACING)
        {
            state = WIN;
            victoryScreen.SetActive(true);
            finalTimeText.text = string.Format("{0}:{1}:{2}", Mathf.RoundToInt(timer / 60).ToString("D2"), Mathf.RoundToInt(timer % 60).ToString("D2"), Mathf.RoundToInt((timer % 1) * 100).ToString("D2"));
            DisableControl();
        }
    }

    public void Lose()
    {
        // Check to make sure the game hasn't finished
        if (state == RACING)
        {
            state = LOSE;
            failureScreen.SetActive(true);
            DisableControl();
        }
    }

    private void GoToStart()
    {
        state = START;

        scoreScreen.enabled = false;
        startScreen.enabled = true;
        gameScreen.enabled = false;

        DisableControl();
    }

    private void GoToScore()
    {
        state = SCORE;

        startScreen.enabled = false;
        gameScreen.enabled = false;
        scoreScreen.enabled = true;

        DisableControl();

        scoreboard.SetTime(timer);
    }

    private void GoToRacing()
    {
        state = RACING;

        startScreen.enabled = false;
        gameScreen.enabled = true;
        scoreScreen.enabled = false;

        EnableControl();
    }

    public void DisableControl()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.Log("Cant find player from RaceController!");
        }
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
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.Log("Cant find player from RaceController!");
        }
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
