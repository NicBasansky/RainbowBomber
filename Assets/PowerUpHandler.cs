using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bomber.Control;

namespace Bomber.Items
{
    public class PowerUpHandler : MonoBehaviour, IPowerUp
    {
        PlayerController playerController;
        PowerUp currentPowerUp = null;
        Coroutine speedBoostCoroutine = null;
        float startingPlayerThrust = 0;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
        }

        private void Start()
        {
            startingPlayerThrust = playerController.GetThrust();
        }

        // Interface method
        public void ApplyPowerUp(PowerUp details)
        {
            if (details == null) return;

            currentPowerUp = details;

            switch (details.powerUpType)
            {
                case PowerUpType.SpeedBuff:
                    SpeedBuffPowerUp();
                    break;
                default:
                    break;
            }           
        }

        private void SpeedBuffPowerUp()
        {
            if (speedBoostCoroutine != null)
            {
                StopCoroutine(speedBoostCoroutine);
            }
            speedBoostCoroutine = StartCoroutine(TempSpeedBoost(currentPowerUp.speedMultiplier));
        }

        private IEnumerator TempSpeedBoost(float multiplier)
        {  
            playerController.SetThrust(playerController.GetThrust() * multiplier);

            if (currentPowerUp.duration > 0)
            {
                yield return new WaitForSeconds(currentPowerUp.duration);
            }

            playerController.SetThrust(startingPlayerThrust);
            currentPowerUp = null;
            speedBoostCoroutine = null;
        }

    }

}