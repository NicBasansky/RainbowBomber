using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bomber.Items
{
    public class ExplosionScaler : MonoBehaviour
    {
        ParticleSystem ps;
        [SerializeField] float extraScale = 1f;

        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
        }

        // todo make the explosion of particles get bigger
        public void MultiplyParticleScale(float multiplier)
        {
            var shapeModule = ps.shape;
            shapeModule.radius *= multiplier;
            shapeModule.radius += extraScale;
            print(shapeModule.radius);
        }
    }

}