using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bomber.Items;
using static Bomber.Items.Pickup;

namespace Bomber.Control
{
    public class PlayerController : MonoBehaviour, IPowerUp
    {

        [SerializeField] float speed = 5.0f;
        [SerializeField] float thrust = 1.0f;
        [SerializeField] float gravity = 9.82f;

        [SerializeField] float boostThrust = 700f;
        [SerializeField] float boostDuration = 2.0f;
        float startingThrust;
        bool isBoosting = false;

        Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        // Start is called before the first frame update
        void Start()
        {
            // todo see if this affects things other than the player
            Physics.gravity = new Vector3(0, -gravity, 0);
            startingThrust = thrust;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            ProcessInputs();
        }

        void ProcessInputs()
        {
            float horizontal = Input.GetAxis("Horizontal") * Time.deltaTime * speed;
            float vertical = Input.GetAxis("Vertical") * Time.deltaTime * speed;

            //transform.Translate(new Vector3(horizontal, 0, vertical));
            rb.AddForce(new Vector3(horizontal, 0, vertical).normalized * thrust);

            // boost movement
            if (Input.GetKeyDown("space") && !isBoosting)
            {
                StartCoroutine(BoostSpeedCoroutine(new Vector3(horizontal, 0, vertical).normalized, boostThrust));
            }
        }

        public void BoostForwardSpeed(Vector3 boostDirection, float boostAmount)
        {
            rb.AddForce(boostDirection * boostAmount);
        }

        private void PermanentlyIncreaseSpeed(float multiplier)
        {
            thrust *= multiplier;
        }


        private IEnumerator BoostSpeedCoroutine(Vector3 boostDirection, float boostAmount)
        {
            isBoosting = true;
            rb.AddForce(boostDirection * boostAmount);
            print(boostDirection.ToString());

            yield return new WaitForSeconds(boostDuration);
            isBoosting = false;

        }

        public void ApplyPowerUp(PowerUp details)
        {
            if (details.powerUpType == PowerUpType.SpeedBuff)
            {
                PermanentlyIncreaseSpeed(details.speedMultiplier);
            }
        }


    }

}
