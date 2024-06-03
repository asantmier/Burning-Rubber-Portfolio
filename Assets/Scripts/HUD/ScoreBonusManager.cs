using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreBonusManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void UpdateDisplay(List<Bonus> bonuses)
    {
        for (int i = bonuses.Count - 1; i >= 0; i--)
        {
            bonuses[i].remainingTime -= Time.deltaTime;
            if (bonuses[i].remainingTime <= 0)
            {
                bonuses.RemoveAt(i);
            }
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            if (i >= bonuses.Count)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            else
            {
                ScoreBonusElement sbe = transform.GetChild(i).GetComponent<ScoreBonusElement>();
                sbe.gameObject.SetActive(true);
                sbe.Title = bonuses[i].title;
                sbe.Score = bonuses[i].Value;
            }
        }
    }
}
