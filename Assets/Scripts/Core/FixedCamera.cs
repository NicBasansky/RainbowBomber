using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bomber.Core
{
    public class FixedCamera : MonoBehaviour
    {
        [SerializeField] Transform target;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            transform.position = target.transform.position;
        }
    }

}
