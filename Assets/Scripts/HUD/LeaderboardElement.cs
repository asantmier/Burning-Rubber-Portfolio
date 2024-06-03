using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardElement : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI scoreText;
    string playername;
    int score;

    public string Name { 
        get => playername; 
        set { 
            playername = value;
            titleText.text = playername;
        }
    }
    public int Score { 
        get => score;
        set
        {
            score = value;
            scoreText.text = score.ToString();
        }
    }
}
