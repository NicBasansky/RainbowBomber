using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bomber.Items
{
    public class DestroyAfterAnimation : MonoBehaviour
    {
        [SerializeField] GameObject targetToDestroy = null;

        public void OnFinishedPickupAnimation()
        {
            Destroy(targetToDestroy, 0.1f);
        }
    }

}