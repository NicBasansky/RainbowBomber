using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Bomber.UI;
using MoreMountains.Tools;

namespace Bomber.Core
{
    public class Health : MonoBehaviour
    {
        [SerializeField] float startingHealth = 3f;
        [SerializeField] float invincibilityDuration = 1.5f;
        [SerializeField] float invinsibilityDeltaTime = 0.15f;
        [SerializeField] GameObject model;
        [SerializeField] GameObject RainbowHead;

        [Header("FX")]
        [SerializeField] ParticleSystem deathFX;

        public event Action onDeath;

        bool isInvincible = false;
        public float health; // todo make private
        bool isDead = false;
        Vector3 initialScale;
        TrailRenderer trail = null;
        ScoreHUD scoreHUD;
        MMHealthBar healthBar;

        private void Awake()
        {
            trail = GetComponent<TrailRenderer>();
            scoreHUD = FindObjectOfType<ScoreHUD>();
            healthBar = GetComponent<MMHealthBar>();
        }

        void Start()
        {
            health = startingHealth; // TODO use a Lazy Value initialization
            initialScale = model.transform.localScale;
            if (healthBar != null)
            {
                healthBar.UpdateBar(health, 0, startingHealth, true);

            }
        }

        public void AffectHealth(float delta)
        {
            //print("Affect health called on " + name);
            if (!isInvincible)
            {

                health -= delta;
                if (healthBar != null)
                {
                    healthBar.UpdateBar(health, 0, startingHealth, true);
                }

                if (!CheckIsDead())
                {
                    StartCoroutine(BecomeInvincible());
                }
            }
        }

        public bool CheckIsDead()
        {
            if (isDead)
                return true;

            if (health <= 0)
            {
                Die();
                return true;
            }
            return false;
        }

        public void Die()
        {
            health = 0;
            bool isPlayer = gameObject.CompareTag("Player");
            if (deathFX != null)
            {
                Instantiate(deathFX, transform.position, Quaternion.identity);
                
            }
            
            if (isPlayer)
            {
                BodyVisible(false);
                FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Player/DeathSizzle", transform.position);
            }
            else // if enemy only
            {
                // check if is dead so kill count isn't incremented if was first killed then thrown off the map
                if (!isDead && scoreHUD != null)
                {
                    scoreHUD.IncrementKillCount();
                }

                // update quest in ScoreHUD

            }
            isDead = true;
            onDeath.Invoke();

            if (!isPlayer)
                Destroy(gameObject, 0.2f);

        }

       
        public void BodyVisible(bool isVisible)
        {
            model.SetActive(isVisible);
            RainbowHead.SetActive(isVisible);

            trail.Clear();
            trail.enabled = isVisible;
        }

        private IEnumerator BecomeInvincible()
        {
            isInvincible = true;

            for (float i = 0; i < invincibilityDuration; i += invinsibilityDeltaTime)
            {
                if (model.transform.localScale == Vector3.one)
                {
                    ScaleModelTo(Vector3.zero);
                }
                else
                {
                    ScaleModelTo(initialScale);
                }
                yield return new WaitForSeconds(invinsibilityDeltaTime);
            }

            ScaleModelTo(initialScale);

            isInvincible = false;
        }

        public void ResetHealth()
        {
            health = startingHealth;
            isDead = false;
            StartCoroutine(BecomeInvincible());
        }

        public bool GetIsInvincible()
        {
            return isInvincible;
        }

        public bool GetIsDead()
        {
            return isDead;
        }


        private void ScaleModelTo(Vector3 scale)
        {
            model.transform.localScale = scale;
        }

    }

}
