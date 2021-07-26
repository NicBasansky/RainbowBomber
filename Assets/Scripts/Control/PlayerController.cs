using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bomber.Items;
using UnityEngine.InputSystem;
using Bomber.Core;
using Bomber.UI;
//using UnityEngine.AI;
using System;

namespace Bomber.Control
{
    public class PlayerController : MonoBehaviour, IBombExplosion
    {
        [SerializeField] float speed = 5.0f;
        [SerializeField] float thrust = 105f;
        [SerializeField] float torque = 40f;
        [SerializeField] float gravity = 20f;
        [SerializeField] float boostThrust = 4000f;
        [SerializeField] float boostDuration = 2.0f;
        [SerializeField] HealthHUD healthHUD = null;
        [SerializeField] SphereCollider sphere = null;
        [SerializeField] PauseHandler pauseHandler;
        [SerializeField] TimeManager timeManager;
        [SerializeField] GameObject mainCamera;
        [SerializeField] GameObject aimCamera;
        [SerializeField] GameObject launchCam;
        [SerializeField] Transform aimLookTransform;
        [SerializeField] GameObject launchSmashFX;
        [SerializeField] Transform parentSpawnedFX;
        

        [Header("Knockback")]
        [SerializeField] float explosionForceMulitiplier = 1.5f;
        [SerializeField] float knockbackUpwardsModifier = 15f;
        [SerializeField] float knockbackParalisisSeconds = 1.5f;

        [Header("Restarting after death")]
        [SerializeField] float deathWitnessDelay = 2.5f;
        [SerializeField] float restartSpeed = 30f;
        [SerializeField] float acceptanceDistToStart = 1.2f;

        [Header("UI")]
        [SerializeField] GameObject getReadyCanvas = null;
        [SerializeField] float getReadyUIDisplaySeconds = 4.0f;
        const float reticleVisDelay = 0.2f;
        [SerializeField] AimReticleUI aimUI;

        float startingThrust;
        bool isBoosting = false;
        bool isDead = false;
        bool isHitByPhysics = false;
        bool isParalized = false;
        bool slowed = false;
        bool shouldMoveToStart = false;
        //bool isJumping = false;
        bool allowRightStickMovement = false;
        bool isAiming = false;
        bool aimInputCalled = false;
        bool isReleasingAim = false;
        bool isLaunching = false;
        float minHeightToAim = 5.0f;
        GameObject ground;

        Vector3 moveVec = Vector3.zero;
        float initialThrust = 0.0f;     
        float thrustRecoveryAmount = 17.0f;
        float horizontal = 0.0f;
        float vertical = 0.0f;
        Vector3 rightStickVec = Vector3.zero;
        float aimRotPower = 1.5f;
        Quaternion lookTransformStartingRotation;
        Vector3 launchDirection;
        float launchSpeed = 65.0f;
        Vector3 launchTarget;
        float launchExplosionRadius = 7.0f;
        float launchExplosionForce = 2000f; // TODO tune?
   

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

            initialThrust = thrust;
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

            if (isAiming)
            {
                DetectEnemyInReticle();
            }
            else
            {
                aimInputCalled = false; // TODO useful?
            }

            if (slowed)
            {
                thrust += thrustRecoveryAmount * Time.deltaTime;
                if (thrust >= initialThrust)
                {
                    thrust = initialThrust;
                    slowed = false;
                }
            }

        }

        void FixedUpdate()
        {
            ProcessInputs();
        }


        private void LateUpdate()
        {
            if (isLaunching)
            {
                // if heading toward the ground and far enough away
                if (!IsGrounded() && (transform.position.y - ground.transform.position.y) > 1.5f)
                {
                    // move
                    transform.position += launchDirection * launchSpeed * Time.deltaTime;

                }
                else // if we are within acceptable distance from ground
                {
                    rb.isKinematic = false;
                    isParalized = false;
                    isLaunching = false;
                    launchDirection = Vector3.zero;
                    launchCam.SetActive(false);                  
                    mainCamera.SetActive(true);
                    ExplodeFromLaunch();                  
                }

                // prevent player from falling through the floor while launching
                // TODO could potentially interfere with the kill zone or launching off the map
                if (transform.position.y <= ground.transform.position.y)
                {
                    transform.position = new Vector3(transform.position.x,
                            ground.transform.position.y + 1.5f, transform.position.z);
                }

            }
        }

        private void DetectEnemyInReticle()
        {
            RaycastHit hit;
            float maxDistance = 100.0f;
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if(Physics.SphereCast(ray, 5.0f, out hit, maxDistance))
            {
                if (hit.collider.tag == "Slime")               
                {
                    print("Detect Enemy's trying to set has target");
                    aimUI.SetHasTarget(true);
                }
                else
                {
                    aimUI.SetHasTarget(false);
                }
            }
        }

        

        private void ExplodeFromLaunch()
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, launchExplosionRadius);
            foreach (Collider c in cols)
            {
                var ai = c.GetComponent<BT_AIController>();
                if (ai != null)
                {
                    ai.GetComponent<Health>().AffectHealth(-1);
                    ai.AffectByExplosion(launchExplosionForce,
                            new Vector3(transform.position.x, ground.transform.position.y, transform.position.z), launchExplosionRadius);// transform.position, launchExplosionRadius);
                    var smashFx = Instantiate(launchSmashFX, transform.position, Quaternion.identity, parentSpawnedFX);
             
                    continue;
                }

                if (c.attachedRigidbody != null && c.attachedRigidbody != rb)
                {
                    c.attachedRigidbody.AddExplosionForce(launchExplosionForce, transform.position, launchExplosionRadius);

                }

                var smashFX = Instantiate(launchSmashFX, transform.position, Quaternion.identity, parentSpawnedFX);
            }
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
                ReleaseAirAim(false);
            }     
        }

        private void OnAirLaunch()
        {
            if (isAiming)
            {
                timeManager.ReleaseBulletTime();

                AirLaunch();
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

        private void AimWhileInAir()
        {
            // if not grounded and a certain height off the ground
            if (!IsGrounded() && (transform.position.y - ground.transform.position.y) > minHeightToAim)
            {
                isAiming = true;
                allowRightStickMovement = true;
                timeManager.BulletTime();
                StartCoroutine(ShowAimReticle());

                aimCamera.SetActive(true);
                mainCamera.SetActive(false);
               
            }
            else
                ReleaseAirAim(false);
        }

        private void ReleaseAirAim(bool useLaunchCam)
        {
            isAiming = false;
            allowRightStickMovement = false;
            aimLookTransform.rotation = Quaternion.identity;
            aimUI.EnableAimUI(false);

            timeManager.ReleaseBulletTime();
           
            aimCamera.SetActive(false);
            if (useLaunchCam)
            {
                launchCam.SetActive(true);
               
            }
            else
            {
                mainCamera.SetActive(true);
            }
        }
      
        private IEnumerator ShowAimReticle()
        {
            yield return new WaitForSeconds(reticleVisDelay);

            aimUI.EnableAimUI(true);       
        }

        private void AirLaunch()
        {
            RaycastHit hit;        
            if(Physics.Raycast(Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)), out hit, 100.0f, LayerMask.NameToLayer("whatIsGround")))
            {           
                launchTarget = hit.point;
                launchDirection = (launchTarget - transform.position).normalized;
                rb.isKinematic = true;
                isParalized = true; // prevent player inputs while launching
                isLaunching = true;
                ReleaseAirAim(true);

            }
        }

        public void SlowSpeed(float slowDownSpeedFraction)
        {
            slowed = true;
            thrust = thrust * slowDownSpeedFraction;
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
