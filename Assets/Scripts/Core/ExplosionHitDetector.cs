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
        [SerializeField] float colliderOverlapTime = 0.2f;
        float explosionRadius = 0f;
        float explosionForce = 0f;
        Bomb instigatorBomb = null;
        float timeSinceEnabled = Mathf.Infinity;
        bool isDisabled = false;

        public void SetupExplosion(float force, float radius)
        {
            explosionForce = force;
            explosionRadius = radius;
            GetComponent<SphereCollider>().radius = explosionRadius;

            timeSinceEnabled = 0;
            isDisabled = false;
        }

        private void Update()
        {
            timeSinceEnabled += Time.deltaTime;
            if (timeSinceEnabled > colliderOverlapTime)
            {
                DisableCollider();
            }

        }

        private void OnTriggerEnter(Collider other)
        {
            AffectHealthIfExposed(other);
            CheckIfIsBomb(other);
        }

        private bool AffectHealthIfExposed(Collider other)
        {
            if (other.gameObject.tag == "PhysicsObject")
            {
                other.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, 7f);
                return true;
            }

            if (other.gameObject.tag == "Slime" || other.gameObject.tag == "Player")
            {
                Health health = other.GetComponent<Health>();
                if (health != null)
                {
                    if (health.GetIsInvincible()) return false;

                    RaycastHit hitInfo;
                    Vector3 direction = health.transform.position - baseTransform.position;
                    bool isExposed = true;
                    if (Physics.Raycast(baseTransform.position, direction, out hitInfo))
                    {
                        isExposed = (hitInfo.collider == health.GetComponent<Collider>());
                    }

                    if (isExposed)
                    {
                        health.AffectHealth(1f);

                        IBombExplosion bombExplosion = health.GetComponent<IBombExplosion>(); // if there are multiple components affected by the explosion then change it here
                        if (bombExplosion != null)
                        {
                            bombExplosion.AffectByExplosion(explosionForce, gameObject.transform.position, 7.0f);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private void CheckIfIsBomb(Collider other)
        {
            if (other.gameObject.tag == "Bomb")
            {
                other.GetComponent<Bomb>().ExplodeBomb();
            }
        }

        private void DisableCollider()
        {
            if (!isDisabled)
            {
                isDisabled = true;
                GetComponent<SphereCollider>().enabled = false;
            }
        }
    }
}
