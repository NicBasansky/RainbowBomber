using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bomber.Items;
using UnityEngine.InputSystem;
using Bomber.Core;

namespace Bomber.Control
{
    public class PlayerController : MonoBehaviour, IPowerUp, IBombExplosion
    {
        [SerializeField] float speed = 5.0f;
        [SerializeField] float thrust = 1.0f;
        [SerializeField] float gravity = 9.82f;
        [SerializeField] float boostThrust = 700f;
        [SerializeField] float boostDuration = 2.0f;

        [Header("Knockback")]
        [SerializeField] float knockbackPower = 10f;
        [SerializeField] float knockbackRadius = 3f;
        [SerializeField] float knockbackUpwardsModifier = 3.0f;
        [SerializeField] float knockbackParalisisSeconds = 1.5f;

        float startingThrust;
        bool isBoosting = false;
        bool isDead = false;
        bool isHitByPhysics = false;
        bool isParalized = false;

        Rigidbody rb;
        BombDropper bombDropper;


        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            bombDropper = GetComponent<BombDropper>();
        }

        private void OnEnable()
        {
            GetComponent<Health>().onDeath += OnDeath;
        }

        private void OnDisable()
        {
            GetComponent<Health>().onDeath += OnDeath;
        }

        void Start()
        {
            // todo see if this affects things other than the player
            Physics.gravity = new Vector3(0, -gravity, 0);
            startingThrust = thrust;

        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B) && !isParalized)
            {
                DropBomb();
            }
        }

        void FixedUpdate()
        {
            ProcessInputs();
        }

        void ProcessInputs()
        {
            if (isDead || isParalized) return;

            float horizontal = Input.GetAxis("Horizontal") * Time.deltaTime * speed;
            float vertical = Input.GetAxis("Vertical") * Time.deltaTime * speed;

            rb.AddForce(new Vector3(horizontal, 0, vertical).normalized * thrust);

            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                if (gamepad.bButton.isPressed)
                {
                    DropBomb();
                }
            }

            // boost movement
            if (Input.GetKeyDown("space") && !isBoosting) // controller??
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
            //print(boostDirection.ToString());

            yield return new WaitForSeconds(boostDuration);
            isBoosting = false;
        }

        public IEnumerator KnockbackCoroutine(float explosionForce, Vector3 sourcePosition, float radius)
        {
            rb.AddExplosionForce(explosionForce, sourcePosition, radius, knockbackUpwardsModifier);

            isParalized = true;

            yield return new WaitForSeconds(knockbackParalisisSeconds);

            isParalized = false;
        }

        private void DropBomb()
        {
            bombDropper.DropBomb();
        }

        private void OnDeath()
        {
            isDead = true;
        }

        public void ApplyPowerUp(PowerUp details)
        {
            if (details.powerUpType == PowerUpType.SpeedBuff)
            {
                PermanentlyIncreaseSpeed(details.speedMultiplier);
            }
        }

        public void AffectByExplosion(float explosionForce, Vector3 sourcePosition, float radius)
        {
            StartCoroutine(KnockbackCoroutine(explosionForce, sourcePosition, radius));
        }
    }
}
