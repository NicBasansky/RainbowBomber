using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bomber.Core
{
    public class AutoRotator : MonoBehaviour
    {
        [SerializeField] float speed = 1f;
        [SerializeField] bool clockwise = true;


        void Update()
        {
            if (clockwise)
            {
                transform.Rotate(0, Time.deltaTime * speed, 0, Space.Self);
            }
            else
            {
                transform.Rotate(0, -Time.deltaTime * speed, 0, Space.Self);
            }

        }
    }

}