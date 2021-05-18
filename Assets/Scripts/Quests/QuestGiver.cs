using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bomber.Control;

namespace Bomber.Quests
{
   
    [System.Serializable]
    public class Quest
    {
        public QuestType questType;
        [Tooltip("Will search for all coins if not specified")]
        public int minCoinsToCollect;
        [Tooltip("Will count all enemies if not specified")]
        public int minEnemiesToKill;

    }
    
    public class QuestGiver : MonoBehaviour
    {
        [SerializeField]
        Quest quest;
        QuestReceiver questReceiver = null;

        private void Awake()
        {
            questReceiver = GameObject.FindWithTag("Player").GetComponent<QuestReceiver>();

        }

        private void Start()
        {
            SetupQuest();
        }

        public void SetupQuest()
        {
            if (!questReceiver) return;

            if (quest.questType == QuestType.CollectAllCoins)
            {
                int totalCoins = GameObject.FindGameObjectsWithTag("Coin").Length;
                int coinsToCollect = totalCoins;
                if (quest.minCoinsToCollect > 0 && quest.minCoinsToCollect <= totalCoins)
                {
                    coinsToCollect = quest.minCoinsToCollect;
                }
                quest.minCoinsToCollect = coinsToCollect;
            }
            else if (quest.questType == QuestType.KillAllEnemies)
            {
                int totalEnemies = GameObject.FindGameObjectsWithTag("Slime").Length;
                int enemiesToKill = totalEnemies;
                if (quest.minEnemiesToKill > 0 && quest.minEnemiesToKill <= totalEnemies)
                {
                    enemiesToKill = quest.minEnemiesToKill;
                }
                quest.minEnemiesToKill = enemiesToKill;
            }

            questReceiver.BeginQuest(quest);
        }
    }
}
