using Bomber.Control;
using Bomber.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPuncher : MonoBehaviour
{
    [SerializeField] float contactDamage = 1f;
    [SerializeField] Transform spawnTransform;
    [SerializeField] GameObject particleFx;
    [SerializeField] float kickbackForce = 100f;
    [SerializeField] float slowDownSpeedFraction = 0.2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.GetComponent<Health>().AffectHealth(-contactDamage);

            if (particleFx != null)
            {
                GameObject fx = Instantiate(particleFx, transform);
                fx.transform.position = spawnTransform.position;

            }

            var pc = other.GetComponent<PlayerController>();
            pc.SlowSpeed(slowDownSpeedFraction);
            pc.AffectByExplosion(kickbackForce, transform.position, 3.0f);
        }
        
    }
 
}
