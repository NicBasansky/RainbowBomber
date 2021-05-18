using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bomber.Control;

namespace Bomber.Core
{
    public class BoostPad : MonoBehaviour
    {
        [SerializeField] float speedBoost = 15000f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                other.GetComponent<PlayerController>().BoostForwardSpeed(transform.forward, speedBoost);
            }

        }
    }

}