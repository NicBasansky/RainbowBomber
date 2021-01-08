using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bomber.Items
{
    public class ExplosionParticleScaler : MonoBehaviour
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
            shapeModule.radius = (shapeModule.radius + 1) * multiplier;
            shapeModule.radius -= 1;
            // the radius is already 0.5 so adding one to make it grow with the multiplier, not 
            // cut it in half
            shapeModule.radius += extraScale;
            print("shapeModule radius: " + shapeModule.radius);
        }
    }

}