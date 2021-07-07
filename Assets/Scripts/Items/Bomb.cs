﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bomber.Core;

namespace Bomber.Items
{
    public class Bomb : MonoBehaviour
    {
        [SerializeField] float timeToExplode = 3.0f;
        [SerializeField] GameObject explosionPrefab;
        [SerializeField] float explosionForce = 1500f;
        [SerializeField] float upwardsModifier = 1f;

        [Header("Flashing")]
        [SerializeField] float maxFlashSpeed = 3f;
        [SerializeField] float flashSpeedMultiplier = 1.3f;
        [SerializeField] float flashAccelTime = 0.2f;

        float damage = 0;
        float explosionRadius = 0f;


        private void OnEnable()
        {
            StartCoroutine(RunBombSequence());
        }

        public void SetupBomb(float radius, float damage)
        {
            explosionRadius = radius;

            this.damage = damage;
        }

        IEnumerator RunBombSequence() // todo rework explosion scalar
        {
            StartCoroutine(BombFlash());
            yield return new WaitForSeconds(timeToExplode);

            ActivateExplosionFX();

            gameObject.SetActive(false);
        }

        public void ExplodeBomb()
        {
            StopCoroutine(RunBombSequence());

            FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Bomb/BombExplosion", transform.position);
            ActivateExplosionFX();

            gameObject.SetActive(false);
        }

        private void ActivateExplosionFX()
        {
            GameObject fx = Pool.singleton.Get("BombFX");
            if (fx != null)
            {
                fx.GetComponent<ExplosionHitDetector>().SetupExplosion(explosionForce, explosionRadius);
                fx.transform.position = transform.position;
                fx.SetActive(true);
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

        // private void OnDrawGizmos()
        // {
        //     Gizmos.color = Color.red;
        //     Gizmos.DrawWireSphere(transform.position, explosionRadius);
        // }
    }
}
