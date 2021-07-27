using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bomber.Core;
using Bomber.Control;

public class DangerCylinder : MonoBehaviour
{
    [SerializeField] float contactDamage = 1f;
    [SerializeField] float playerKickbackForce = 200f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.GetComponent<Health>().AffectHealth(-contactDamage);
            other.GetComponent<PlayerController>().AffectByExplosion(playerKickbackForce, transform.position, 1.0f);

        }
        else if (other.gameObject.tag == "Slime")
        {
            other.GetComponent<Health>().AffectHealth(-contactDamage);
        }
    }
}
