using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bomber.Core;
using System;

namespace Bomber.Items
{
    public class Bomb : MonoBehaviour
    {
        [SerializeField] float timeToExplode = 3.0f;
        [SerializeField] GameObject explosionPrefab;
        //[SerializeField] float initialExplosionRadius = 3.0f;
        [SerializeField] float currentExplosionRadius = 3.0f; // todo make private
        [SerializeField] float explosionForce = 1000f;
        [SerializeField] float upwardsModifier = 1f;
        [SerializeField] float theshroldDistanceToIncreaseExplosion = 3f;
        [SerializeField] float explosionIncreaseMultiplier = 2.5f;
        float damage = 0;

        [Header("Flashing")]
        [SerializeField] float maxFlashSpeed = 3f;
        [SerializeField] float flashSpeedMultiplier = 1.3f;
        [SerializeField] float flashAccelTime = 0.2f;

        Vector3 explosionCentre;
        bool shouldCancelPhysics = false;
        bool explosionCentreUpdated = false;
        Vector3 halfwayPoint; // remove

        private void OnEnable()
        {
            StartCoroutine(RunBombSequence());
        }

        private void Update()
        {
            if (!explosionCentreUpdated)
            {
                explosionCentre = transform.position;
            }
        }

        public void SetupBomb(float radius, float damage)
        {
            //currentExplosionRadius = initialExplosionRadius; TODO need?
            currentExplosionRadius = radius;
            //print("new explosionRadius: " + currentExplosionRadius.ToString());

            this.damage = damage;
        }

        IEnumerator RunBombSequence() // todo rework explosion scalar
        {
            StartCoroutine(BombFlash());
            yield return new WaitForSeconds(timeToExplode);

            //GameObject fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            //fx.GetComponentInChildren<ExplosionParticleScaler>().MultiplyParticleScale(currentExplosionRadius);
            ActivateExplosionFX();

            //CheckForOverlappingPhysicsObjects();

            ResetBombParameters();

            gameObject.SetActive(false);

        }

        public void ExplodeBomb(Bomb instigatingBomb)
        {
            StopCoroutine(RunBombSequence());

            // ActivateExplosionFX();
            // //physics in here
            // CheckForOverlappingPhysicsObjects();

            if (instigatingBomb != null) // explosion from another bomb
            {
                IncreaseExplosionSize(instigatingBomb);
                ActivateExplosionFX();
            }
            else
            {
                //physics in here
                ActivateExplosionFX();
                //physics in here
                // CheckForOverlappingPhysicsObjects();
            }

            ResetBombParameters();

            gameObject.SetActive(false);
        }

        private void IncreaseExplosionSize(Bomb otherBomb)
        {
            Vector3 vectorToOtherbomb = otherBomb.transform.position - transform.position;
            float distance = Vector3.Distance(transform.position, otherBomb.transform.position);//GetNewExplosionPoint());
            halfwayPoint = Vector3.Lerp(transform.position, transform.position + vectorToOtherbomb, 0.5f);
            SetNewExplosionPoint(halfwayPoint);
            // if (distance >= theshroldDistanceToIncreaseExplosion)
            // {
            //     currentExplosionRadius = (distance / 2) * 1.5f;
            // }
            // else
            // {
            currentExplosionRadius *= explosionIncreaseMultiplier; // TODO configure
            //}
            otherBomb.CancelPhysics();
        }

        private void SetNewExplosionPoint(Vector3 halfwayPoint)
        {
            explosionCentre = halfwayPoint;
            explosionCentreUpdated = true;
        }

        public Vector3 GetNewExplosionPoint()
        {
            return explosionCentre;
        }

        private void ActivateExplosionFX()
        {
            GameObject fx = Pool.singleton.Get("BombFX");
            if (fx != null)
            {
                fx.SetActive(true);
                fx.transform.position = transform.position;
                // null check?
                fx.GetComponent<ExplosionHitDetector>().SetBombReference(this, shouldCancelPhysics);
            }
        }



        private void CheckForOverlappingPhysicsObjects() // there will be problems if two destructable crates hit eachother
        {
            //bool destructableFound = false;
            Collider[] hits = Physics.OverlapSphere(explosionCentre, currentExplosionRadius);
            // foreach (Collider hit in hits) // TODO bring back for destructive crates
            // {
            //     Destructable destructable = hit.GetComponent<Destructable>();
            //     if (destructable == null) continue;

            //     destructable.BeginDestruction(hits);
            //     destructableFound = true;
            // }

            // if (destructableFound) return; // don't apply physics if there is a destructable nearby

            foreach (Collider hit in hits)
            {
                if (hit.gameObject.tag == "PhysicsObject")
                {
                    ApplyExplosionForce(hit);
                    continue;
                }
            }
        }

        private void ApplyExplosionForce(Collider hit)
        {
            hit.transform.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, explosionCentre,
            currentExplosionRadius, 3.0f);
        }

        IEnumerator BombFlash()
        {
            float speed = 1;
            while (speed < maxFlashSpeed)
            {
                GetComponent<Animator>().SetFloat("speed", speed);
                speed *= flashSpeedMultiplier;
                yield return new WaitForSeconds(flashAccelTime);
            }
        }

        public void CancelPhysics()
        {
            shouldCancelPhysics = true;
        }


        private void ResetBlastRadius()
        {
            currentExplosionRadius = 1f;
        }

        private void ResetBombParameters()
        {
            ResetBlastRadius();
            shouldCancelPhysics = false;
            explosionCentreUpdated = false;
            halfwayPoint = Vector3.zero;
            currentExplosionRadius = 3.0f;

        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(explosionCentre, currentExplosionRadius);
        }
    }
}
