using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bomber.Quests
{
    public class QuestReceiver : MonoBehaviour
    {
        [SerializeField] EndGoal endGoalPortal;

        Quest currentQuest;
        int coinGoal = 0;
        int coinCount = 0;
        int killGoal = 0;
        int killCount = 0;

        public void BeginQuest(Quest quest)
        {
            currentQuest = quest;

            if (quest.questType == QuestType.CollectAllCoins)
            {
                coinGoal = quest.minCoinsToCollect;
            }
            else if (quest.questType == QuestType.KillAllEnemies)
            {
                killGoal = quest.minEnemiesToKill;
            }
            Debug.Log($"currentQuest has " + currentQuest.minCoinsToCollect + " coins to collect.");
            Debug.Log($"currentQuest has " + currentQuest.minEnemiesToKill + " enemies to kill.");
        }

        public void IncrementCoinCount() // called by Pickup => scoreHUD => here
        {
            coinCount++;
            CheckIfReachedGoal();
        }

        public void IncrementKillCount()
        {
            killCount++;
            CheckIfReachedGoal();
        }

        private void CheckIfReachedGoal()
        {
            if (coinGoal > 0 && coinCount >= coinGoal)
            {
                Debug.Log("Coin Goal Reached!");
                endGoalPortal.MakeGoalVisible(true);
            }
            else if (killGoal > 0 && killCount >= killGoal)
            {
                Debug.Log("Kill Goal Reached!");
                endGoalPortal.MakeGoalVisible(true);
            }
        }
    }

}