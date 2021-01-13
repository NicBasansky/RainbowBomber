using UnityEngine;

namespace Bomber.Core
{
    public interface IBombExplosion
    {
        void AffectByExplosion(float explosionForce, Vector3 sourcePosition, float radius);
    }
}