using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bomber.UI
{

    public class HealthHUD : MonoBehaviour
    {
        [SerializeField] GameObject rightFullHealthDot = null;
        [SerializeField] GameObject leftFullHealthDot = null;
        //[SerializeField] GameObject rightEmptyHealthDot = null;
        //[SerializeField] GameObject leftEmptyHealthDot = null;

        private int startingHealthPoints = 2;
        private int healthPoints = 2;

        private enum Dot
        {
            Right,
            Left
        }

        private void Start()
        {
            ResetHealthUI();

        }

        public void ResetHealthUI()
        {
            rightFullHealthDot.SetActive(true);
            leftFullHealthDot.SetActive(true);
            healthPoints = startingHealthPoints;
        }

        public void ReduceHealthUI()
        {
            if (healthPoints < 0) return;

            healthPoints -= 1;

            if (healthPoints == 1)
            {
                SetDotState(Dot.Left, false);
            }
            if (healthPoints == 0)
            {
                SetDotState(Dot.Right, false);
            }
        }

        private void SetDotState(Dot dot, bool isFull)
        {
            if (dot == Dot.Right)
            {
                rightFullHealthDot.SetActive(isFull);
            }
            else
            {
                leftFullHealthDot.SetActive(isFull);
            }
        }

    }

}