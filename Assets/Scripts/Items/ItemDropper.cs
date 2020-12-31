using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bomber.Core;


namespace Bomber.Items
{
    public class ItemDropper : MonoBehaviour, IPowerUp
    {

        [SerializeField] float placementOffsetY = 1.0f;
        float accumulativeBlastRadiusMultiplier = 1f;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                DropBomb();

            }
        }

        private void DropBomb()
        {
            GameObject bomb = Pool.singleton.Get("Bomb");
            if (bomb != null)
            {
                bomb.SetActive(true);
                bomb.transform.position = transform.position + new Vector3(0, placementOffsetY, 0);//spawnPosition.transform.position;
                bomb.GetComponent<Bomb>().SetBlastRadius(accumulativeBlastRadiusMultiplier);
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
        }

        private void ResetBlastRadiusMultiplier()
        {
            accumulativeBlastRadiusMultiplier = 1f;
        }

    }

}