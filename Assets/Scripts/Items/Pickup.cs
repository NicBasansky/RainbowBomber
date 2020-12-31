using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bomber.UI;
using System;

namespace Bomber.Items
{
    public class Pickup : MonoBehaviour
    {
        [SerializeField] ScoreHUD scoreHUD;
        //[SerializeField] int scoreIncrement = 10;
        GameObject player;

        [SerializeField] PowerUp powerUpDetails;

        private void Awake()
        {
            player = GameObject.FindWithTag("Player");
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
                    scoreHUD.UpdateScore(Mathf.RoundToInt(powerUpDetails.scoreContribution));
                }

                ApplyPowerUp();

                // play sound

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
