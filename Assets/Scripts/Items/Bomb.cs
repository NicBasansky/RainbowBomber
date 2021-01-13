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
        float damage = 0;

        [Header("Flashing")]
        [SerializeField] float maxFlashSpeed = 3f;
        [SerializeField] float flashSpeedMultiplier = 1.3f;
        [SerializeField] float flashAccelTime = 0.2f;

        private float explosionRadius = 0;

        private void OnEnable()
        {
            StartCoroutine(RunBombSequence());
        }

        public void SetupBomb(float radius, float damage)
        {
            //currentExplosionRadius = initialExplosionRadius; TODO need?
            explosionRadius = radius;
            print("new explosionRadius: " + currentExplosionRadius.ToString());

            this.damage = damage;
        }

        IEnumerator RunBombSequence() // todo rework explosion scalar
        {
            StartCoroutine(BombFlash());
            yield return new WaitForSeconds(timeToExplode);

            //GameObject fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            //fx.GetComponentInChildren<ExplosionParticleScaler>().MultiplyParticleScale(currentExplosionRadius);
            ActivateExplosionFX();

            CheckForOverlappingPhysicsObjects();

            gameObject.SetActive(false);

        }

        public void ExplodeBomb()
        {
            StopCoroutine(RunBombSequence());

            ActivateExplosionFX();

            CheckForOverlappingPhysicsObjects();

            gameObject.SetActive(false);
        }

        private void ActivateExplosionFX()
        {
            GameObject fx = Pool.singleton.Get("BombFX");
            if (fx != null)
            {
                fx.SetActive(true);
                fx.transform.position = transform.position;
                fx.GetComponentInChildren<ParticleEmissionHandler>().ActivateFX();
            }
        }

        private void CheckForOverlappingPhysicsObjects() // there will be problems if two destructable crates hit eachother
        {
            bool destructableFound = false;
            Collider[] hits = Physics.OverlapSphere(transform.position, currentExplosionRadius);
            foreach (Collider hit in hits)
            {
                Destructable destructable = hit.GetComponent<Destructable>();
                if (destructable == null) continue;

                destructable.BeginDestruction(hits);
                destructableFound = true;
            }

            if (destructableFound) return; // don't apply physics if there is a destructable nearby

            foreach (Collider hit in hits)
            {
                if (hit.gameObject.tag == "PhysicsObject")
                {
                    ApplyExplosionForce(hit);
                }

                IBombExplosion bombExplosion = hit.GetComponent<IBombExplosion>();
                if (bombExplosion != null)
                {
                    bombExplosion.AffectByExplosion(explosionForce, transform.position, currentExplosionRadius);
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

        private void ResetBlastRadius()
        {
            currentExplosionRadius = 1f;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, currentExplosionRadius);
        }
    }
}
