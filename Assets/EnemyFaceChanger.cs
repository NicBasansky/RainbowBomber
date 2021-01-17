using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Neon2.SlimeSystem;

namespace Bomber.Core
{
    public class EnemyFaceChanger : MonoBehaviour
    {
        public GameObject aliveHead, injuredHead;

        private void Awake()
        {
            injuredHead.SetActive(false);
        }

        public void ChangeAppearance(bool isInjured) // TODO spawn in another version of the slime with a different face instead of changing this at runtime
        {
            if (aliveHead == null || injuredHead == null)
            {
                print("You need to assign the alive and injured head on the " + gameObject.name);
                return;
            }

            if (isInjured)
            {
                injuredHead.SetActive(true);
                aliveHead.SetActive(false);
            }
            else
            {
                injuredHead.SetActive(false);
                aliveHead.SetActive(true);
            }
        }

    }

}
