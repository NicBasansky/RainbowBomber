using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bomber.Core
{
    public class ParticleAutoDestroyer : MonoBehaviour
    {
        ParticleSystem ps;
        [SerializeField] GameObject targetToDestroy = null;
        [Tooltip("Will kill after timer only if greater than 0")]
        [SerializeField] float destroyTimer = 0f;
        [SerializeField] ParticleSystem particlesToStopLooping = null;

        private float timeSinceSpawned = 0;

        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            if (particlesToStopLooping != null)
            {
                particlesToStopLooping.loop = false;
            }

            if (!ps.IsAlive())
            {
                DestroyTarget();

            }

            if (destroyTimer > 0)
            {
                timeSinceSpawned += Time.deltaTime;

                if (timeSinceSpawned > destroyTimer)
                {
                    DestroyTarget();
                }
            }
        }

        private void DestroyTarget()
        {
            if (targetToDestroy != null)
            {
                Destroy(targetToDestroy);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

}
