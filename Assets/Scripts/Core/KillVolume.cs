using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bomber.Core
{
    public class KillVolume : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.Die();
            }
        }
    }

}