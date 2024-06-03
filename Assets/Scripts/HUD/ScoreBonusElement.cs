using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreBonusElement : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI scoreText;
    string title;
    int score;

    public string Title
    {
        get => title;
        set
        {
            title = value;
            titleText.text = title;
        }
    }
    public int Score
    {
        get => score;
        set
        {
            score = value;
            scoreText.text = score.ToString();
        }
    }
}
