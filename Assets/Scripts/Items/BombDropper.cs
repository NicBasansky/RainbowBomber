using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bomber.Core;

namespace Bomber.Items
{
    public class BombDropper : MonoBehaviour, IPowerUp
    {
        [SerializeField] float damagePerBomb = 1.0f;
        [SerializeField] float dropDelay = 2f;
        [SerializeField] float placementOffsetX = 0.0f;
        [SerializeField] float placementOffsetY = 1.0f;
        [SerializeField] float placementOffsetZ = 0f;
        float timeSinceLastDroppedBomb = Mathf.Infinity;
        float initialExplosionRadius = 3.0f;
        float accumulativeBlastRadiusMultiplier = 1f;

        void Update() // later, could make it so if the player drops a bomb then all the rest do
        {

            timeSinceLastDroppedBomb += Time.deltaTime;
        }

        public void DropBomb()
        {
            if (timeSinceLastDroppedBomb > dropDelay)
            {
                GameObject bomb = Pool.singleton.Get("Bomb");
                if (bomb != null)
                {
                    bomb.SetActive(true);
                    bomb.transform.position = transform.position + new Vector3(placementOffsetX, placementOffsetY, placementOffsetZ);//spawnPosition.transform.position;
                    bomb.GetComponent<Bomb>().SetupBomb(GetExplosionRadius(), damagePerBomb);
                }
                timeSinceLastDroppedBomb = 0;
            }
        }

        public float GetExplosionRadius()
        {
            return initialExplosionRadius * accumulativeBlastRadiusMultiplier;
        }

        public void ApplyPowerUp(PowerUp details)
        {
            if (details.powerUpType == PowerUpType.BlastRadius)
            {
                // -1 so in Power Up we can say 1.x for a positive change to blast radius
                accumulativeBlastRadiusMultiplier += (details.blastRadiusMultiplier - 1);
                //print("accumulative blast radius: " + accumulativeBlastRadiusMultiplier);
            }
            // TODO increase damage
        }



    }

}