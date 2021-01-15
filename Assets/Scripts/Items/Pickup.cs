using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bomber.UI;
using Bomber.Core;

namespace Bomber.Items
{
    public class Pickup : MonoBehaviour
    {
        [Tooltip("ScoreContribution only if there are no Power Up Details")]
        [SerializeField] float scoreContribution = 20f;
        [SerializeField] PowerUp powerUpDetails;
        //[SerializeField] GameObject pickupParticles = null;
        //[SerializeField] Vector3 particleOffset = new Vector3(0, 1f, 0);
        [SerializeField] bool isCoin = false;

        ScoreHUD scoreHUD;
        GameObject player;
        Animator animator;

        private void Awake()
        {
            player = GameObject.FindWithTag("Player");
            scoreHUD = FindObjectOfType<ScoreHUD>();
            animator = GetComponentInChildren<Animator>();
        }

        private void OnTriggerEnter(Collider other)
        {

            if (other.gameObject.tag == "Player")
            {

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

                if (animator != null)
                {
                    animator.SetTrigger("onPickedUp");
                }

                // play sound

                // play particle effect
                // Vector3 particleLocation = new Vector3(transform.position.x + particleOffset.x,
                //                                         transform.position.y + particleOffset.y,
                //                                         transform.position.z + particleOffset.z);
                // Instantiate(pickupParticles, particleLocation, Quaternion.identity);
                //Instantiate(pickupParticles, transform.position, Quaternion.identity);

                // send object back to object Pool

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
