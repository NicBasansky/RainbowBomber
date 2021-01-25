using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bomber.Items;

// This would go on the explosion, not the character
namespace Bomber.Core
{
    public class ExplosionHitDetector : MonoBehaviour
    {
        [SerializeField] Transform baseTransform = null;
        float explosionRadius = 0f;
        float explosionForce = 0f;
        Bomb instigatorBomb = null;

        public void SetupExplosion(float force, float radius)
        {
            explosionForce = force;
            explosionRadius = radius; // TODO use
            GetComponent<SphereCollider>().radius = explosionRadius;
        }

        private void OnTriggerEnter(Collider other)
        {
            AffectHealthIfExposed(other);
            CheckIfIsBomb(other);
        }

        private void AffectHealthIfExposed(Collider other)
        {
            if (other.gameObject.tag == "PhysicsObject")
            {
                other.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, 7f);
                return;
            }

            if (other.gameObject.tag == "Slime" || other.gameObject.tag == "Player")
            {
                Health health = other.GetComponent<Health>();
                if (health != null)
                {
                    if (health.GetIsInvincible()) return;

                    RaycastHit hitInfo;
                    Vector3 direction = health.transform.position - baseTransform.position;
                    bool isExposed = true;
                    if (Physics.Raycast(baseTransform.position, direction, out hitInfo))
                    {
                        isExposed = (hitInfo.collider == health.GetComponent<Collider>());

                    }
                    // TODO fix the length of time the explosion collider is active for
                    if (isExposed)
                    {
                        health.AffectHealth(1f);

                        IBombExplosion bombExplosion = health.GetComponent<IBombExplosion>(); // if there are multiple components affected by the explosion then change it here
                        if (bombExplosion != null)
                        {
                            bombExplosion.AffectByExplosion(explosionForce, gameObject.transform.position, 7.0f);
                        }
                    }
                }
            }
        }

        private void CheckIfIsBomb(Collider other)
        {
            if (other.gameObject.tag == "Bomb")
            {
                other.GetComponent<Bomb>().ExplodeBomb();
            }
        }

    }

}
