using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bomber.Core
{
    public class ParticleEmissionHandler : MonoBehaviour
    {
        float timeSinceActivated = Mathf.Infinity;
        [SerializeField] float emissionDuration = 1.5f;
        [SerializeField] GameObject baseGameObject = null;
        ParticleSystem[] particleSystems;



        private void Awake()
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>();
        }

        private void OnEnable()
        {
            ActivateFX();
        }

        private void Update()
        {
            timeSinceActivated += Time.deltaTime;
            StopParticleEmissionCheck();
        }

        public void ActivateFX()
        {
            foreach (var ps in particleSystems)
            {
                EnableEmissionModule(ps, true);
            }

            timeSinceActivated = 0;
        }

        private void StopParticleEmissionCheck()
        {
            if (timeSinceActivated > emissionDuration)
            {
                foreach (var ps in particleSystems)
                {
                    EnableEmissionModule(ps, false);
                }

                baseGameObject.SetActive(false);
            }
        }

        private static void EnableEmissionModule(ParticleSystem ps, bool enabled)
        {
            ParticleSystem.EmissionModule emissionModule = ps.emission;
            emissionModule.enabled = enabled;
        }

    }

}