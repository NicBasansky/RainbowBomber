using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

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

        private void Awake()
        {
            trail = GetComponent<TrailRenderer>();
        }

        void Start()
        {
            health = startingHealth; // TODO use a Lazy Value initialization
            initialScale = model.transform.localScale;
        }

        public void AffectHealth(float delta)
        {
            if (!isInvincible)
            {

                health -= delta;
                print(gameObject.name + " is at " + health + " health"); // todo remove
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

        private void Die()
        {
            health = 0;
            isDead = true;

            Instantiate(deathFX, transform.position, Quaternion.identity);
            BodyVisible(false);

            onDeath.Invoke();

        }

        // TODO make AI stop following upon player death
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
