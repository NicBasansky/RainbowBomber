using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bomber.Core
{
    [DisallowMultipleComponent]
    public class Oscillator : MonoBehaviour
    {
        [SerializeField] Vector3 movementVector;
        [Range(0, 1)] [SerializeField] float movementFactor;
        [SerializeField] float period = 2f;
        [SerializeField] float delay = 0f;

        Vector3 startPos;

        void Start()
        {
            startPos = transform.position;
        }


        void Update()
        {
            if (period <= Mathf.Epsilon) return;


            float cycles = (Time.time - delay) / period;
            const float tau = Mathf.PI * 2;
            float rawSineWave = Mathf.Sin(cycles * tau);
            movementFactor = (rawSineWave / 2) + 0.5f;

            Vector3 offset = movementVector * movementFactor;
            transform.position = startPos + offset;
        }
    }
}
