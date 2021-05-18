using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Bomber.Quests;


namespace Bomber.UI
{
    public class ScoreHUD : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI scoreCount;
        [SerializeField] TextMeshProUGUI coinCount;
        [SerializeField] TextMeshProUGUI killCount;
        [SerializeField] QuestReceiver questReceiver;
        [SerializeField] int kills = 0;
        [SerializeField] int coins = 0;
        int score = 0;

        void Start()
        {
            scoreCount.text = score.ToString();
            killCount.text = kills.ToString();
            coinCount.text = coins.ToString();
        }

        public void UpdateScore(int delta)
        {
            score += delta;
            scoreCount.text = score.ToString();
        }

        public void IncrementCoinCount()
        {
            coins++;
            coinCount.text = coins.ToString();
            questReceiver.IncrementCoinCount();
        }

        public void IncrementKillCount()
        {
            kills++;
            killCount.text = kills.ToString();
            questReceiver.IncrementKillCount();
        }
    }
}
