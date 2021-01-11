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
        [SerializeField] float placementOffsetY = 1.0f;
        float timeSinceLastDroppedBomb = Mathf.Infinity;
        float accumulativeBlastRadiusMultiplier = 1f;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.B) && timeSinceLastDroppedBomb > dropDelay)
            {
                DropBomb();
                timeSinceLastDroppedBomb = 0;
            }
            timeSinceLastDroppedBomb += Time.deltaTime;
        }

        private void DropBomb()
        {
            GameObject bomb = Pool.singleton.Get("Bomb");
            if (bomb != null)
            {
                bomb.SetActive(true);
                bomb.transform.position = transform.position + new Vector3(0, placementOffsetY, 0);//spawnPosition.transform.position;
                bomb.GetComponent<Bomb>().SetupBomb(accumulativeBlastRadiusMultiplier, damagePerBomb);
            }
        }

        public void ApplyPowerUp(PowerUp details)
        {
            if (details.powerUpType == PowerUpType.BlastRadius)
            {
                // -1 so in Power Up we can say 1.x for a positive change to blast radius
                accumulativeBlastRadiusMultiplier += (details.blastRadiusMultiplier - 1);
                print("accumulative blast radius: " + accumulativeBlastRadiusMultiplier);
            }
            // TODO increase damage
        }



    }

}