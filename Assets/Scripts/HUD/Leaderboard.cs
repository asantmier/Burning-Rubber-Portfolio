using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Leaderboard : MonoBehaviour
{
    public GameObject content;
    
    int playerCount;

    private void Start()
    {
        SetPlayerCount(1);
        SetScore(0, 0);
        SetScore(1, 0);
        SetScore(2, 0);
        SetScore(3, 0);
    }

    public void SetPlayerCount(int count)
    {
        playerCount = count;
        content.transform.GetChild(0).gameObject.SetActive(playerCount >= 1);
        content.transform.GetChild(1).gameObject.SetActive(playerCount >= 2);
        content.transform.GetChild(2).gameObject.SetActive(playerCount >= 3);
        content.transform.GetChild(3).gameObject.SetActive(playerCount >= 4);
    }

    public void SetName(int player, string name)
    {
        content.transform.GetChild(player).GetComponent<LeaderboardElement>().Name = name;
    }

    public void SetScore(int player, int score)
    {
        content.transform.GetChild(player).GetComponent<LeaderboardElement>().Score = score;
    }

    public void AddScore(int player, int score)
    {
        content.transform.GetChild(player).GetComponent<LeaderboardElement>().Score += score;
    }

    public int GetScore(int player)
    {
        return content.transform.GetChild(player).GetComponent<LeaderboardElement>().Score;
    }
}
