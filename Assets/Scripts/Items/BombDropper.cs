using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bomber.Core;

namespace Bomber.Items
{
    public enum BombExplosionLevel
    {
        Base,
        RingMin,
        RingMid,
        RingMax
    }

    public class BombDropper : MonoBehaviour, IPowerUp
    {
        [SerializeField] float damagePerBomb = 1.0f;
        [SerializeField] float dropDelay = 2f;
        [SerializeField] float placementOffsetX = 0.0f;
        [SerializeField] float placementOffsetY = 1.0f;
        [SerializeField] float placementOffsetZ = 0f;
        [SerializeField] float initialExplosionRadius = 5.0f;
        [SerializeField] float maxExplosionRadius = 8.5f;
        [SerializeField] float currentExplosionRadius = 0;
        [SerializeField] int numPowerupsToMaxBlastRadius = 4;
        [SerializeField] BombExplosionLevel bombExplosionLevel;
        [SerializeField] int currentNumBombs = 3;
        [SerializeField] int maxNumBombs = 3;
        [SerializeField] float secondsToReload = 2f;

        float timeSinceLastDroppedBomb = Mathf.Infinity;
        float timeSinceReload = 0;
        float accumulativeBlastRadiusMultiplier = 1f;

        void Start()
        {
            currentExplosionRadius = initialExplosionRadius;
            currentNumBombs = Mathf.Min(currentNumBombs, maxNumBombs);

        }

        void Update() 
        {

            timeSinceLastDroppedBomb += Time.deltaTime;
            timeSinceReload += Time.deltaTime;

            if (timeSinceLastDroppedBomb > secondsToReload)
            {
                ReloadBomb();
            }
        }

        private void ReloadBomb() // TODO Hook up UI for num bombs available
        {
            
            if (timeSinceReload > secondsToReload)
            {
                currentNumBombs++;
                currentNumBombs = Mathf.Min(currentNumBombs, maxNumBombs);
                timeSinceReload = 0;

            }
        }

        public int GetCurrentNumBombs()
        {
            return currentNumBombs;
        }

        public void DropBomb()
        {
            if (timeSinceLastDroppedBomb > dropDelay && currentNumBombs > 0)
            {
                GameObject bomb = Pool.singleton.Get("Bomb");
                if (bomb != null)
                {
                    bomb.transform.position = transform.position + new Vector3(placementOffsetX, placementOffsetY, placementOffsetZ);//spawnPosition.transform.position;
                    bomb.GetComponent<Bomb>().SetupBomb(GetExplosionRadius(), damagePerBomb, bombExplosionLevel);
                    bomb.SetActive(true);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Bomb/DropBomb", transform.position);

                    currentNumBombs--;
                }
                timeSinceLastDroppedBomb = 0;
            }
        }

        public float GetExplosionRadius()
        {
            return currentExplosionRadius; // * accumulativeBlastRadiusMultiplier;
        }

        public void ApplyPowerUp(PowerUp details)
        {
            if (details.powerUpType == PowerUpType.BlastRadius)
            {
                if (numPowerupsToMaxBlastRadius == 0)
                    numPowerupsToMaxBlastRadius = 1;

                // how much to boost expl rad. with every powerup given starting and max radius and powerups in level
                float incrementalAmount = (maxExplosionRadius - initialExplosionRadius) / numPowerupsToMaxBlastRadius;
                currentExplosionRadius += incrementalAmount;
                if (currentExplosionRadius > maxExplosionRadius)
                {
                    currentExplosionRadius = maxExplosionRadius;
                }
                if (bombExplosionLevel != BombExplosionLevel.RingMax)
                {
                    bombExplosionLevel++;

                }
                
                //// -1 so in Power Up we can say 1.x for a positive change to blast radius
                //accumulativeBlastRadiusMultiplier += (details.blastRadiusMultiplier - 1);
                //print("accumulative blast radius: " + accumulativeBlastRadiusMultiplier);
            }
            // TODO increase damage
        }

        //private void OnDrawGizmos()
        //{
        //    Gizmos.color = Color.red;
        //    Gizmos.DrawWireSphere(transform.position, currentExplosionRadius);
        //}



    }

}