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
        [SerializeField] float peripheralExplosionForce = 1000f;
        Bomb instigatorBomb = null;
        bool cancelPhysics = false;

        private void OnEnable() // reset values
        {
            instigatorBomb = null;
            cancelPhysics = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            AffectHealthIfExposed(other);

            CheckIfIsBomb(other);

        }

        private void AffectHealthIfExposed(Collider other)
        {
            if (!cancelPhysics && other.gameObject.tag == "PhysicsObject")
            {
                other.GetComponent<Rigidbody>().AddExplosionForce(peripheralExplosionForce, transform.position, 7f);
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
                        //isExposed = (hitInfo.collider == health.GetComponent<Collider>());
                        if (hitInfo.transform.tag == "Environment")
                        {
                            isExposed = false;
                        }
                    }

                    if (isExposed)
                    {
                        health.AffectHealth(1f);

                        if (!cancelPhysics)
                        {
                            //print("an explosion is happening in the hit detector!");
                            IBombExplosion bombExplosion = health.GetComponent<IBombExplosion>(); // if there are multiple components affected by the explosion then change it here
                            if (bombExplosion != null)
                            {
                                bombExplosion.AffectByExplosion(peripheralExplosionForce, gameObject.transform.position, 7.0f);
                            }
                        }
                    }
                }
            }
        }

        public void SetBombReference(Bomb bombReference, bool shouldCancelPhysics)
        {
            instigatorBomb = bombReference;
            if (cancelPhysics)
            {
                return;
            }
            cancelPhysics = shouldCancelPhysics;
        }

        private void CheckIfIsBomb(Collider other)
        {
            if (other.gameObject.tag == "Bomb")
            {
                other.GetComponent<Bomb>().ExplodeBomb(instigatorBomb);
            }
        }

    }

}
