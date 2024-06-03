using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    public TextMeshProUGUI yourScore;
    public TextMeshProUGUI inputField;
    public GameObject[] scorePlates;

    private float playerTime;

    private List<KeyValuePair<string, float>> scores;
    private int numscores;

    private float closeTimer = 5;
    private bool closing = false;

    // Start is called before the first frame update
    void Start()
    {
        scores = new List<KeyValuePair<string, float>>();

        numscores = PlayerPrefs.GetInt("numScores");
        for (int i = 0; i < numscores; i++)
        {
            string name = PlayerPrefs.GetString("Name" + i);
            float time = PlayerPrefs.GetFloat("Time" + i);
            scores.Add(new KeyValuePair<string, float>(name, time));
        }

        SortScores();
    }

    private void Update()
    {
        if (closing)
        {
            closeTimer -= Time.deltaTime;
            if (closeTimer <= 0)
            {
                SceneManager.LoadScene(0);
            }
        }
    }

    public void ResetScene()
    {
        SceneManager.LoadScene(0);
    }

    public void SetTime(float newtime)
    {
        playerTime = newtime;
        yourScore.text = string.Format("{0}:{1}:{2}", Mathf.RoundToInt(playerTime / 60).ToString("D2"), Mathf.RoundToInt(playerTime % 60).ToString("D2"), Mathf.RoundToInt((playerTime % 1) * 100).ToString("D2"));
    }

    public void SubmitScore()
    {
        if (!closing)
        {
            closing = true;
            string name = inputField.text;
            numscores += 1;
            PlayerPrefs.SetInt("numScores", numscores);
            PlayerPrefs.SetString("Name" + (numscores - 1), name);
            PlayerPrefs.SetFloat("Time" + (numscores - 1), playerTime);
            PlayerPrefs.Save();

            scores.Add(new KeyValuePair<string, float>(name, playerTime));
            SortScores();
        }
    }

    void SortScores()
    {
        scores = scores.OrderBy(x => x.Value).ToList();

        for (int i = 0; i < scorePlates.Length && i < scores.Count; i++)
        {
            string timestring = string.Format("{0}:{1}:{2}", Mathf.RoundToInt(scores[i].Value / 60).ToString("D2"), Mathf.RoundToInt(scores[i].Value % 60).ToString("D2"), Mathf.RoundToInt((scores[i].Value % 1) * 100).ToString("D2"));
            scorePlates[i].GetComponent<ScoreplateHelper>().Setup(scores[i].Key, timestring);
        }
    }
}
