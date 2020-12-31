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
        [SerializeField] float explosionRadius = 3.0f;
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
            fx.GetComponentInChildren<ExplosionScaler>().MultiplyParticleScale(explosionRadius);
            ExplosionDamage();

            gameObject.SetActive(false);
        }

        private void ExplosionDamage()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider hit in hits)
            {
                Health enemy = hit.GetComponent<Health>();
                if (enemy == null) continue;

                enemy.AffectHealth(damage);
                print("detected " + enemy.name);
            }
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
            explosionRadius = initialExplosionRadius;
            explosionRadius *= multiplier;
            print("new explosionRadius: " + explosionRadius.ToString());
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);

            // todo make the particles reflect the size of the explosion radius
        }
    }

}
