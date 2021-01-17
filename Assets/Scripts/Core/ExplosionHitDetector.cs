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
        bool alreadyHit = false;

        private void OnTriggerEnter(Collider other)
        {
            AffectHealthIfExposed(other);
            CheckIfIsBomb(other);
        }

        private void AffectHealthIfExposed(Collider other)
        {
            //if (alreadyHit) return;
            // alreadyHit = true;
            print("AffectHealthIfExposed");

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

                    IBombExplosion bombExplosion = health.GetComponent<IBombExplosion>(); // if there are multiple components affected by the explosion then change it here
                    if (bombExplosion != null)
                    {
                        bombExplosion.AffectByExplosion(peripheralExplosionForce, gameObject.transform.position, 3f);
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