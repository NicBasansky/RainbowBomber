using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bomber.Core
{

    public class Health : MonoBehaviour
    {
        [SerializeField] float startingHealth = 100f;
        public float health; // todo make private
        bool isDead = false;


        // Start is called before the first frame update
        void Start()
        {
            health = startingHealth;
        }

        public void AffectHealth(float delta)
        {
            health -= delta;
            CheckIsDead();
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

    }

}
