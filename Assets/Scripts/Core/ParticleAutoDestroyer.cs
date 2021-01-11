using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bomber.Core
{
    public class ParticleAutoDestroyer : MonoBehaviour
    {
        ParticleSystem ps;
        [SerializeField] GameObject targetToDestroy = null;

        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            if (!ps.IsAlive())
            {
                if (targetToDestroy != null)
                {
                    Destroy(targetToDestroy);
                }
                else
                {
                    Destroy(gameObject);
                }

            }
        }
    }

}
