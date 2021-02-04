using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bomber.Core
{
    public class AutoDeactivate : MonoBehaviour
    {
        [SerializeField] GameObject targetToDeactivate = null;
        [SerializeField] float delay = 4f;
        float timeSinceActivated = 0;

        private void Update()
        {
            timeSinceActivated += Time.deltaTime;

            if (timeSinceActivated > delay)
            {
                timeSinceActivated = 0;
                targetToDeactivate.SetActive(false);
            }
        }
    }
}
