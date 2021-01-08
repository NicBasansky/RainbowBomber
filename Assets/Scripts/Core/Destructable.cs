using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bomber.Core
{
    public class Destructable : MonoBehaviour
    {
        [SerializeField] ParticleSystem ExplosionFx;
        [SerializeField] float explosionRadius = 4f;
        [SerializeField] float power = 2000f;
        [SerializeField] float upwardsModifier = 3f;

        public void BeginDestruction()
        {
            SpawnParticles();
            ApplyExplosionForce();

            // todo play a sound

            Destroy(gameObject);
        }

        private void SpawnParticles()
        {
            Instantiate(ExplosionFx, transform.position, Quaternion.identity);
        }

        private void ApplyExplosionForce()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider hit in hits)
            {
                if (hit.gameObject.tag == "PhysicsObject")
                {
                    hit.GetComponent<Rigidbody>().AddExplosionForce(power, transform.position,
                                                    explosionRadius, upwardsModifier);
                }
            }
        }

    }

}
