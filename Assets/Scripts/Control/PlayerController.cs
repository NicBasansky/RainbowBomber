using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bomber.Items;
using UnityEngine.InputSystem;
using Bomber.Core;
using Bomber.UI;

namespace Bomber.Control
{
    public class PlayerController : MonoBehaviour, IBombExplosion
    {
        [SerializeField] float speed = 5.0f;
        [SerializeField] float thrust = 1.0f;
        [SerializeField] float torque = 100f;
        [SerializeField] float gravity = 9.82f;
        [SerializeField] float boostThrust = 700f;
        [SerializeField] float boostDuration = 2.0f;
        [SerializeField] HealthHUD healthHUD = null;

        [Header("Knockback")]
        [SerializeField] float explosionForceMulitiplier = 1f;
        [SerializeField] float knockbackUpwardsModifier = 3.0f;
        [SerializeField] float knockbackParalisisSeconds = 1.5f;

        [Header("Restarting after death")]
        [SerializeField] float deathWitnessDelay = 3.0f;
        //[SerializeField] [Range(0, 1f)] float restartLerpSpeed = 0.3f;
        [SerializeField] float restartSpeed = 5.0f;
        [SerializeField] float acceptanceDistToStart = 0.4f;
        [SerializeField] GameObject getReadyCanvas = null;
        [SerializeField] float getReadyUIDisplaySeconds = 4.0f;

        float startingThrust;
        bool isBoosting = false;
        bool isDead = false;
        bool isHitByPhysics = false;
        bool isParalized = false;
        bool shouldMoveToStart = false;
        bool isJumping = false;

        Vector3 moveVec = Vector3.zero;
        float horizontal = 0.0f;
        float vertical = 0.0f;

        Rigidbody rb;
        BombDropper bombDropper;
        StartingPad startPad;


        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            bombDropper = GetComponent<BombDropper>();
            startPad = FindObjectOfType<StartingPad>();
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

            if (getReadyCanvas != null)
            {
                StartCoroutine(DisplayGetReadyUI());
            }

        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B) && !isParalized)
            {
                DropBomb();
            }

            if (shouldMoveToStart)
            {
                MoveTowardsStartingPad();
            }
        }

        void FixedUpdate()
        {
            ProcessInputs();
        }

        public void OnMove(InputValue input)
        {
            var inputVec = input.Get<Vector2>();
            moveVec = new Vector3(inputVec.x, 0, inputVec.y);
            Debug.Log("Horiz: " + inputVec.x + " Vert: " + inputVec.y);
        }

        public void OnDropBomb()
        {
            DropBomb();
            print("Dropped bomb");
        }

        void ProcessInputs()
        {
            if (isDead || isParalized) return;


            //horizontal = Input.GetAxis("Horizontal") * Time.deltaTime * speed;
            //vertical = Input.GetAxis("Vertical") * Time.deltaTime * speed;


            //var gamepad = Gamepad.current;
            //if (gamepad != null)
            //{
            //    if (gamepad.bButton.isPressed)
            //    {
            //        DropBomb();
            //    }
            //    //horizontal = gamepad.leftStick.ReadValue().x * Time.deltaTime * speed;
            //    //vertical = gamepad.leftStick.ReadValue().y * Time.deltaTime * speed;



            //    //if (gamepad.leftStick.)
            //}

            horizontal = moveVec.x * speed * Time.deltaTime;
            vertical = moveVec.z * speed * Time.deltaTime;
            //Debug.Log("Horiz: " + horizontal + " Vert: " + vertical);

            rb.AddForce(new Vector3(horizontal, 0, vertical).normalized * thrust);
            rb.AddTorque(new Vector3(vertical, 0, horizontal) * torque);

            // boost movement
            if (Input.GetKeyDown("space") && !isBoosting) // controller??
            {
                StartCoroutine(BoostSpeedCoroutine(new Vector3(horizontal, 0, vertical).normalized, boostThrust));
            }

            

            //Debug.Log("rHoriz: " + rHoriz + " rVert: " + rVert);
        }

        public void BoostForwardSpeed(Vector3 boostDirection, float boostAmount)
        {
            rb.AddForce(boostDirection * boostAmount);
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
            rb.AddExplosionForce(explosionForce * explosionForceMulitiplier, sourcePosition, radius, knockbackUpwardsModifier);

            isParalized = true;

            yield return new WaitForSeconds(knockbackParalisisSeconds);

            isParalized = false;
        }

        public IEnumerator DisplayGetReadyUI()
        {
            getReadyCanvas.SetActive(true);
            yield return new WaitForSeconds(getReadyUIDisplaySeconds);
            getReadyCanvas.SetActive(false);
        }

        private void DropBomb()
        {
            bombDropper.DropBomb();
        }

        private void OnDeath()
        {
            isDead = true;
            StartCoroutine(RestartPlayer());
        }

        IEnumerator RestartPlayer()
        {

            yield return new WaitForSeconds(deathWitnessDelay);

            shouldMoveToStart = true;
            GetComponent<SphereCollider>().enabled = false;

            if (healthHUD != null)
            {
                healthHUD.ResetHealthUI();
            }

            StartCoroutine(DisplayGetReadyUI());

            // make ai ok to attack player
        }

        private void MoveTowardsStartingPad()
        {
            if (startPad == null)
            {
                print("No start pad in scene!");
                return;
            }

            rb.isKinematic = true;

            Vector3 direction = Vector3.Normalize(startPad.transform.position - transform.position);
            transform.position += direction * restartSpeed * Time.deltaTime;
            if (Vector3.Distance(transform.position, startPad.transform.position) <= acceptanceDistToStart)
            {
                GetComponent<SphereCollider>().enabled = true;
                transform.position = startPad.transform.position;
                rb.isKinematic = false;
                shouldMoveToStart = false;

                Health health = GetComponent<Health>();
                health.BodyVisible(true);
                health.ResetHealth();

                isDead = false;
                //StartCoroutine(DisplayGetReadyUI());
            }
        }

        public void SetThrust(float thrust)
        {
            this.thrust = thrust;
        }

        public float GetThrust()
        {
            return thrust;
        }

        public void AffectByExplosion(float explosionForce, Vector3 sourcePosition, float radius)
        {
            StartCoroutine(KnockbackCoroutine(explosionForce, sourcePosition, radius));

            if (healthHUD != null)
            {
                healthHUD.ReduceHealthUI();
            }
        }
    }
}
