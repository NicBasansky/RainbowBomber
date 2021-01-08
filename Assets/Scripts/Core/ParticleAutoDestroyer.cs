using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bomber.Core
{
    public class ParticleAutoDestroyer : MonoBehaviour
    {
        ParticleSystem ps;

        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            if (!ps.IsAlive())
            {
                Destroy(gameObject);
            }
        }
    }

}
