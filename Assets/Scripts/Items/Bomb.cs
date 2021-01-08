using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bomber.Core;


namespace Bomber.Items
{
    public class Bomb : MonoBehaviour
    {
        [SerializeField] float timeToExplode = 3.0f;
        [SerializeField] GameObject explosionPrefab;
        [SerializeField] float initialExplosionRadius = 3.0f;
        [SerializeField] float currentExplosionRadius = 3.0f; // todo make private
        [SerializeField] float explosionForce = 1000f;
        [SerializeField] float upwardsModifier = 1f;
        [SerializeField] float damage = 50.0f;

        [Header("Flashing")]
        [SerializeField] float maxFlashSpeed = 3f;
        [SerializeField] float flashSpeedMultiplier = 1.3f;
        [SerializeField] float flashAccelTime = 0.2f;

        private void OnEnable()
        {
            StartCoroutine(RunBombSequence());

        }


        IEnumerator RunBombSequence()
        {
            StartCoroutine(BombFlash());
            yield return new WaitForSeconds(timeToExplode);

            GameObject fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            fx.GetComponentInChildren<ExplosionParticleScaler>().MultiplyParticleScale(currentExplosionRadius);

            CheckForOverlappingHits();



            gameObject.SetActive(false);
        }

        private void CheckForOverlappingHits()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, currentExplosionRadius);
            foreach (Collider hit in hits)
            {
                DealDamage(hit);

                if (hit.gameObject.tag == "PhysicsObject")
                {
                    ApplyExplosionForce(hit);
                }

                Destructable destructable = hit.GetComponent<Destructable>();
                if (destructable != null)
                {
                    destructable.BeginDestruction();
                }

            }
        }

        private void DealDamage(Collider hit)
        {
            Health enemy = hit.GetComponent<Health>();
            if (enemy != null)
            {
                enemy.AffectHealth(damage);
            }
        }

        private void ApplyExplosionForce(Collider hit)
        {
            print(hit.gameObject.name);
            hit.transform.GetComponentInChildren<Rigidbody>().AddExplosionForce(explosionForce, transform.position,
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

        public void SetBlastRadius(float multiplier)
        {
            currentExplosionRadius = initialExplosionRadius;
            currentExplosionRadius *= multiplier;
            print("new explosionRadius: " + currentExplosionRadius.ToString());
        }

        private void ResetBlastRadius()
        {
            currentExplosionRadius = 1f;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, currentExplosionRadius);

            // todo make the particles reflect the size of the explosion radius
        }
    }

}
