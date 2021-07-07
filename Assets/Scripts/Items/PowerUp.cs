using UnityEngine;

namespace Bomber.Items
{
    [CreateAssetMenu(fileName = "PowerUp", menuName = "PowerUp/Create New PowerUp", order = 51)]
    public class PowerUp : ScriptableObject
    {
        public PowerUpType powerUpType = PowerUpType.None;
        public float scoreContribution = 0;
        public float speedMultiplier = 1;
        public float duration = 0.0f;
        public float blastRadiusMultiplier = 1f;
        public string pickupSoundFMODPath = "";
        
        
    }
}
