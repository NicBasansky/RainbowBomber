using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bomber.Core
{

    public class Health : MonoBehaviour
    {
        [SerializeField] float startingHealth = 3f;
        [SerializeField] float invincibilityDuration = 1.5f;
        [SerializeField] float invinsibilityDeltaTime = 0.15f;
        [SerializeField] GameObject model;
        bool isInvincible = false;
        public float health; // todo make private
        bool isDead = false;


        void Start()
        {
            health = startingHealth; // TODO use a Lazy Value initialization
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
            if (health <= 0)
            {
                Die();
            }
            return isDead;
        }

        private void Die()
        {
            health = 0;
            isDead = true;

            // play death animation
            // stop moving
            // destroy / return to object pool
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
                    ScaleModelTo(Vector3.one);
                }
                yield return new WaitForSeconds(invinsibilityDeltaTime);

            }

            ScaleModelTo(Vector3.one);

            isInvincible = false;
        }

        private void ScaleModelTo(Vector3 scale)
        {
            model.transform.localScale = scale;
        }

        private void OnParticleCollision(GameObject other)
        {
            if (other.tag == "BombFX")
            {
                AffectHealth(1f);
            }
        }

    }

}
