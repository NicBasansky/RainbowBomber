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
        [SerializeField] SphereCollider sphere = null;
        [SerializeField] PauseHandler pauseHandler;
        [SerializeField] TimeManager timeManager;
        [SerializeField] GameObject mainCamera;
        [SerializeField] GameObject aimCamera;
        [SerializeField] Transform aimLookTransform;

        [SerializeField] InputActionAsset controls;
        private InputAction inputAction;
        

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
        //bool isJumping = false;
        bool allowRightStickMovement = false;
        bool isAiming = false;
        bool aimInputCalled = false;
        bool isReleasingAim = false;
        float minHeightToAim = 5.0f;
        GameObject ground;

        Vector3 moveVec = Vector3.zero;
        float horizontal = 0.0f;
        float vertical = 0.0f;
        Vector3 rightStickVec = Vector3.zero;
        float aimRotPower = 1.5f;
        Quaternion lookTransformStartingRotation;

        Rigidbody rb;
        BombDropper bombDropper;
        StartingPad startPad;


        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            bombDropper = GetComponent<BombDropper>();
            startPad = FindObjectOfType<StartingPad>();
            ground = GameObject.FindGameObjectWithTag("whatIsGround");

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

            lookTransformStartingRotation = transform.rotation;
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

            //if (aimInputCalled)
            //{
            //    AimWhileInAir();
        
            //}
            //if (isReleasingAim)
            //{
            //    ReleaseAirAim();
            //}
            //print("TimeScale: " + Time.timeScale);
        }

        void FixedUpdate()
        {
            ProcessInputs();
        }

        // Input message
        public void OnMove(InputValue input)
        {
            var inputVec = input.Get<Vector2>();
            moveVec = new Vector3(inputVec.x, 0, inputVec.y);
 
        }

        public void OnMoveRightStick(InputValue input)
        {
            var inputVec = input.Get<Vector2>();
            rightStickVec = new Vector3(inputVec.x, 0, inputVec.y);
            
        }

        // Input message
        public void OnDropBomb()
        {
            DropBomb();
            
        }

        public void OnPause()
        {
            pauseHandler.Pause();
        }

        // Input message
        public void OnAirTarget()
        {
            aimInputCalled = !aimInputCalled;
            if (aimInputCalled)
            {
                AimWhileInAir();
            }
            else
            {    
                ReleaseAirAim();
            }     
        }


        void ProcessInputs()
        {
            if (isDead || isParalized) return;

            horizontal = moveVec.x * speed * Time.deltaTime;
            vertical = moveVec.z * speed * Time.deltaTime;

            rb.AddForce(new Vector3(horizontal, 0, vertical).normalized * thrust);
            rb.AddTorque(new Vector3(vertical, 0, horizontal) * torque);

            if (allowRightStickMovement)
            {
                aimLookTransform.rotation *= Quaternion.AngleAxis(rightStickVec.x * aimRotPower, Vector3.up);
                aimLookTransform.rotation *= Quaternion.AngleAxis(-rightStickVec.z * aimRotPower, Vector3.right);
            }

            MoveRightStickCameraWhileAiming();

            // boost movement
            if (Input.GetKeyDown("space") && !isBoosting) // controller??
            {
                StartCoroutine(BoostSpeedCoroutine(new Vector3(horizontal, 0, vertical).normalized, boostThrust));
            }

        }

        private void MoveRightStickCameraWhileAiming()
        {
            Vector3 angles = aimLookTransform.transform.localEulerAngles;
            angles.z = 0;
            // clamp the vertical rotation
            var angle = aimLookTransform.transform.localEulerAngles.x;
            if (angle < 180 && angle < 35)
            {
                angles.x = 35;
            }
            else if (angle < 180 && angle > 80)
            {
                angles.x = 80;
            }

            // clamp the horizontal rotation -25 to 25
            var hAngle = aimLookTransform.transform.localEulerAngles.y;
            if (hAngle > 25 && hAngle < 180)
            {
                angles.y = 25;
            }
            else if (hAngle > 180 && hAngle < 335)
            {
                angles.y = 335;
            }

            aimLookTransform.localEulerAngles = angles;
        }


        // TODO is this called in an update somewhere?? maybe should only be called once in the pressed event callback
        private void AimWhileInAir()
        {
            // if not grounded and a certain height off the ground
            if (!IsGrounded() && (transform.position.y - ground.transform.position.y) > minHeightToAim)
            {
                
                allowRightStickMovement = true;
                timeManager.BulletTime();
               
            }
            else
                ReleaseAirAim();

            // adjust camera

            mainCamera.SetActive(false);
            aimCamera.SetActive(true);
            // start a coroutine to show the reticle a little after the cam has had a chance to blend

            // make a target reticle appear
            // move the reticle with right stick
            // if Right Trigger then 
            // launch player towards reticle

            // once makes contact with anything, make a big explosion



        }

        //float releaseRotSpeed = 10.0f;
        private void ReleaseAirAim()
        {
            //isAiming = false;
            allowRightStickMovement = false;
            mainCamera.SetActive(true);
            aimCamera.SetActive(false);
            aimLookTransform.rotation = Quaternion.identity;

            // start a coroutine to show the reticle a little after the cam has had a chance to blend
            timeManager.ReleaseBulletTime();
           

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
            sphere.enabled = false;
            //GetComponent<SphereCollider>().enabled = false;

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
                sphere.enabled = true;
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

        private bool IsGrounded()
        {
            if (Physics.Raycast(transform.position, -Vector3.up, 1.5f, LayerMask.NameToLayer("whatIsGround")))
            {
                //Debug.DrawRay(transform.position, -Vector3.up, Color.red);
                return true;
            }
            else
                return false;
        }
    }
}
