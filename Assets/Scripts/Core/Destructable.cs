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

        public void BeginDestruction(Collider[] hits)
        {
            SpawnParticles();
            ApplyExplosionForce(hits);

            // todo play a sound

            DestroyGameObject();
        }

        private void SpawnParticles()
        {
            Instantiate(ExplosionFx, transform.position, Quaternion.identity);
        }



        private void ApplyExplosionForce(Collider[] hits)
        {
            //Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider hit in hits)
            {
                if (hit.gameObject.tag == "PhysicsObject")
                {
                    hit.GetComponent<Rigidbody>().AddExplosionForce(power, transform.position,
                                                    explosionRadius, upwardsModifier);
                }
            }
        }

        private void DestroyGameObject()
        {
            MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
            meshRenderer.enabled = false;
            Collider collider = GetComponent<BoxCollider>();
            collider.enabled = false;

            Destroy(gameObject, 1f);
        }

    }

}
