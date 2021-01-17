using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Neon2.SlimeSystem;

namespace Bomber.Core
{
    public class EnemyFaceChanger : MonoBehaviour
    {
        public SlimeVisualConfig aliveSlimeVisualConfig;
        public SlimeVisualConfig deadSlimeVisualConfig;

        private SlimeVisual slimeVisual;

        private void Awake()
        {
            slimeVisual = GetComponentInChildren<SlimeVisual>();
        }

        public void ChangeAppearance(bool isInjured)
        {
            if (isInjured)
            {
                slimeVisual.SetupVisual(deadSlimeVisualConfig);
            }
            else
            {
                slimeVisual.SetupVisual(aliveSlimeVisualConfig);
            }
        }
    }

}
