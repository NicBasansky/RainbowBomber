using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bomber.UI;
using System;

namespace Bomber.Items
{
    public class Pickup : MonoBehaviour
    {
        [Tooltip("ScoreContribution only if there are no Power Up Details")]
        [SerializeField] float scoreContribution = 20f;
        [SerializeField] PowerUp powerUpDetails;

        ScoreHUD scoreHUD;
        GameObject player;

        private void Awake()
        {
            player = GameObject.FindWithTag("Player");
            scoreHUD = FindObjectOfType<ScoreHUD>();
        }

        private void OnTriggerEnter(Collider other)
        {

            if (other.gameObject.tag == "Player")
            {
                GetComponentInChildren<MeshRenderer>().enabled = false;

                // play particle effect

                // increase count in UI
                if (scoreHUD != null)
                {
                    if (powerUpDetails != null)
                    {
                        scoreHUD.UpdateScore(Mathf.RoundToInt(powerUpDetails.scoreContribution));
                    }
                    else
                    {
                        scoreHUD.UpdateScore(Mathf.RoundToInt(scoreContribution));
                    }
                }

                if (powerUpDetails != null)
                {
                    ApplyPowerUp();
                }

                // play sound

                // play particle effect

                // send object back to object Pool
                Destroy(gameObject);
            }
        }

        private void ApplyPowerUp()
        {
            foreach (var powerUp in player.GetComponents<IPowerUp>())
            {
                powerUp.ApplyPowerUp(powerUpDetails);
            }
        }
    }

}
