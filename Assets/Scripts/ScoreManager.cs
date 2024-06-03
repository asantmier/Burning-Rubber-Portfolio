using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public enum BonusType
{
    DRIFT,
    SPEED,
    AIR
}

public class Bonus
{

    static int bonusId = 0;

    public int id;
    public string title;
    private int value;
    public float remainingTime;
    private float internalValue;

    public int Value { get => value; }
    public float InternalValue { get => internalValue; set
        {
            internalValue = value;
            this.value = (int)(internalValue) - (int)(internalValue) % 10;
        }
    }

    public Bonus(string title, float internalValue, float duration)
    {
        this.id = ++bonusId;
        this.title = title;
        this.InternalValue = internalValue;
        this.remainingTime = duration;
    }

    public int GetId()
    {
        return id;
    }
}

public class ScoreManager : MonoBehaviour
{
    public ScoreBonusManager scoreBonusManager;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI rankText;
    public Slider rankProgress;

    public int pointsToNextRank = 1000;
    public float rankRequirementMultiplier = .2f;
    public float rankDecay = 0.05f;
    public float D_ScoreMult = 1.05f;
    public float C_ScoreMult = 1.1f;
    public float B_ScoreMult = 1.15f;
    public float A_ScoreMult = 1.25f;
    public float S_ScoreMult = 1.40f;
    public float SS_ScoreMult = 1.65f;
    public float SSS_ScoreMult = 2.05f;
    public float rankDecayGracePeriod = 0.5f;

    private float nextRankProgress;

    private int score = 0;
    private int rankNumber = 0;
    // D-0, C-1, B-2, A-3, S-4, SS-5, SSS-6
    private float lastScoreTime;

    private bool doScoring;

    public int Score { get => score; }
    public string Rank { get
        {
            switch (rankNumber)
            {
                case 0:
                    return "D";
                case 1:
                    return "C";
                case 2:
                    return "B";
                case 3:
                    return "A";
                case 4:
                    return "S";
                case 5:
                    return "SS";
                case 6:
                    return "SSS";
                default:
                    Debug.LogError("If you're seeing this then something really bad happened in ScoreManager.");
                    return "X";
            }
        }
    }


    public float onScreenDuration = 3;

    List<Bonus> bonuses = new List<Bonus>();

    Dictionary<int, Bonus> continuousBonusDict = new Dictionary<int, Bonus>();

    // Start is called before the first frame update
    void Start()
    {
        scoreText.text = "0";
        rankText.text = "D";
        nextRankProgress = (pointsToNextRank + pointsToNextRank * (rankRequirementMultiplier * rankNumber)) / 2;
    }

    private void Update()
    {
        if (Time.time - lastScoreTime > rankDecayGracePeriod) {
            // Decay rank by a percentage of total progress. Percentage is bigger for higher ranks
            DecreaseRankProgressByPercent((rankDecay + rankDecay * (rankRequirementMultiplier * rankNumber)) * Time.deltaTime);
        }
    }

    private void LateUpdate()
    {
        scoreText.text = score.ToString();
        rankText.text = Rank.ToString();
        rankProgress.value = nextRankProgress / (pointsToNextRank + pointsToNextRank * (rankRequirementMultiplier * rankNumber));
        scoreBonusManager.UpdateDisplay(bonuses);
    }

    private string GetBonusTitle(BonusType type, int value)
    {
        string title = "";
        switch (type)
        {
            case BonusType.DRIFT:
                if (value < 500)
                {
                    title = "Drift";
                } else if (value >= 500 && value < 1500)
                {
                    title = "Sick Drift";
                } else if (value >= 1500)
                {
                    title = "EX Drift";
                }
                break;
            case BonusType.SPEED:
                if (value < 500)
                {
                    title = "Speed";
                }
                else if (value >= 500 && value < 1500)
                {
                    title = "Crazy Speed";
                }
                else if (value >= 1500)
                {
                    title = "EX Speed";
                }
                break;
            case BonusType.AIR:
                if (value < 500)
                {
                    title = "Airtime";
                }
                else if (value >= 500 && value < 1500)
                {
                    title = "Wild Airtime";
                }
                else if (value >= 1500)
                {
                    title = "EX Airtime";
                }
                break;
            default:
                Debug.LogError("No bonus exists of given type");
                break;
        }
        return title;
    }

    public void AddPenaltyInstant(string title, int cost)
    {
        bonuses.Add(new Bonus(title, cost, onScreenDuration));
        DecreaseScore(cost);
    }

    public void AddBonusInstant(string title, int value)
    {
        bonuses.Add(new Bonus(title, value, onScreenDuration));
        IncreaseScore(value);
    }

    // Return ID used to refernce this bonus
    public int OpenContinuousBonus(BonusType type, int value)
    {
        string title = GetBonusTitle(type, value);
        Bonus b = new Bonus(title, value, 1000f);
        var idNum = b.id;
        bonuses.Add(b);
        continuousBonusDict.Add(idNum, b);
        IncreaseScore(value);
        return idNum;
    }

    public void UpdateContinuousBonus(int id, BonusType type, float addInternalValue)
    {
        if (!continuousBonusDict.ContainsKey(id))
        {
            Debug.LogError("Attempted to update bonus that no longer exists.");
            return;
        }
        int oldValue = continuousBonusDict[id].Value;
        continuousBonusDict[id].InternalValue += addInternalValue;
        continuousBonusDict[id].title = GetBonusTitle(type, continuousBonusDict[id].Value);
        continuousBonusDict[id].remainingTime = 1000f;
        IncreaseScore(continuousBonusDict[id].Value - oldValue);
    }

    public bool IsBonusRegistered(int id)
    {
        return continuousBonusDict.ContainsKey(id);
    }

    public void CloseContinousBonus(int id)
    {
        if (!continuousBonusDict.ContainsKey(id))
        {
            Debug.LogError("Attempted to close bonus that no longer exists.");
            return;
        }

        continuousBonusDict[id].remainingTime = onScreenDuration;

        continuousBonusDict.Remove(id);
    }

    public float GetScoreMultiplier()
    {
        switch(rankNumber)
        {
            case 0:
                return D_ScoreMult;
            case 1:
                return C_ScoreMult;
            case 2:
                return B_ScoreMult;
            case 3:
                return A_ScoreMult;
            case 4:
                return S_ScoreMult;
            case 5:
                return SS_ScoreMult;
            case 6:
                return SSS_ScoreMult;
            default:
                Debug.LogError("If you're seeing this then something really bad happened in ScoreManager.");
                return 1f;
        }
    }

    private void DecreaseScore(float amount)
    {
        if (doScoring)
        {
            score = Mathf.Clamp(score - (int)amount, 0, int.MaxValue);
        }
        DecreaseRankProgressByPercent(0.5f);
    }

    private void IncreaseScore(float amount)
    {
        if (doScoring)
        {
            int multipliedScore = (int)(amount * GetScoreMultiplier());
            score += multipliedScore;
        }
        
        IncreaseRankProgress(amount);
    }

    private void IncreaseRankProgress(float points)
    {
        lastScoreTime = Time.time;
        nextRankProgress += points;
        while (nextRankProgress >= (pointsToNextRank + pointsToNextRank * (rankRequirementMultiplier * rankNumber)))
        {
            if (rankNumber < 6)
            {
                nextRankProgress -= (pointsToNextRank + pointsToNextRank * (rankRequirementMultiplier * rankNumber)) / 2;
                rankNumber += 1;
            } else
            {
                nextRankProgress = pointsToNextRank + pointsToNextRank * (rankRequirementMultiplier * rankNumber);
            }
        }
    }

    public void DecreaseRankProgressByPercent(float percentage)
    {
        nextRankProgress -= (pointsToNextRank + pointsToNextRank * (rankRequirementMultiplier * rankNumber)) * percentage;
        if (nextRankProgress < 0)
        {
            if (rankNumber > 0)
            {
                rankNumber -= 1;
                nextRankProgress = pointsToNextRank + pointsToNextRank * (rankRequirementMultiplier * rankNumber);
            } else
            {
                nextRankProgress = 0;
            }
        }
    }

    public int GetScore()
    {
        return score;
    }

    public void ResetScore()
    {
        score = 0;
    }

    public void FullReset()
    {
        score = 0;
        rankNumber = 0;
        // This reset of the rank progress to 50% doesn't actually seem to work for some reason
        nextRankProgress = (pointsToNextRank + pointsToNextRank * (rankRequirementMultiplier * rankNumber)) / 2;
        bonuses.Clear();
        continuousBonusDict.Clear();
        lastScoreTime = Time.time;
    }

    public void DoScoring(bool toggle)
    {
        doScoring = toggle;
    }
}
