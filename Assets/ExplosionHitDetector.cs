using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bomber.Items;

namespace Bomber.Core
{
    public class ExplosionHitDetector : MonoBehaviour
    {
        [SerializeField] Transform baseTransform = null;

        private void OnTriggerEnter(Collider other)
        {
            CheckIfHasHealth(other);
            CheckIfIsBomb(other);
        }

        private void CheckIfHasHealth(Collider other)
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                RaycastHit hitInfo;
                Vector3 direction = health.transform.position - baseTransform.position;
                bool isExposed = false;
                if (Physics.Raycast(baseTransform.position, direction, out hitInfo))
                {
                    isExposed = (hitInfo.collider == health.GetComponent<Collider>());
                }

                if (isExposed)
                {
                    health.AffectHealth(1f);
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