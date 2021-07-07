using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Bomber.Items;
using Random = UnityEngine.Random;
using Bomber.Core;
using Neon2.SlimeSystem;

namespace Bomber.Control
{
    public class AIController : MonoBehaviour, IBombExplosion
    {
        [SerializeField] float chaseRadius = 5f;
        [SerializeField] float stoppingDist = 1.5f; // must be less than attackRadius
        [SerializeField] float attackRadius = 2f;
        [SerializeField] float walkPointRange = 30.0f;
        [SerializeField] float maxNavPathLength = 20.0f;
        [SerializeField] float maxSpeed = 7.0f;
        [SerializeField] float minAllowedSecondsOnCurrentPath = 3.0f;
        [SerializeField] float maxAllowedSecondsOnCurrentPath = 7.0f;
        [SerializeField] float attackCooldown = 3.5f;
        [SerializeField] float rotSpeed = 1f;
        [SerializeField] float fleeDistance = 11f;
        GameObject groundObject;
        public LayerMask whatIsGround;

        [Header("Knockback")]
        [SerializeField] float knockbackUpwardsModifier = 3.0f;
        [SerializeField] float knockbackParalisisSeconds = 1.5f;

        bool maxAllowedTimeSpecified = false;
        Vector3 walkPoint;
        bool walkPointSet = false;
        float timeOnCurrentPath = Mathf.Infinity;
        float timeSinceLastAttack = Mathf.Infinity;
        float maxTimeOnCurrentPath = 0;
        bool isDead = false;
        bool hasAttacked = false;
        bool beenHit = false;

        BombDropper bombDropper = null;
        GameObject target;
        NavMeshAgent agent;
        Rigidbody rb;
        Animator anim;
        EnemyFaceChanger faceChanger;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            anim = GetComponentInChildren<Animator>(); // todo remove inChildren
            bombDropper = GetComponent<BombDropper>();
            rb = GetComponent<Rigidbody>();
            target = GameObject.FindWithTag("Player");
            faceChanger = GetComponent<EnemyFaceChanger>();
            groundObject = GameObject.FindGameObjectWithTag("whatIsGround");
        }

        private void OnEnable()
        {
            GetComponent<Health>().onDeath += OnDeath;
        }

        private void OnDisable()
        {
            GetComponent<Health>().onDeath += OnDeath;
        }

        private void Update()
        {
            if (beenHit)
            {
                HitRecovery();
            }
        }

        private void HitRecovery()
        {
            if (Vector3.Distance(transform.position, new Vector3(transform.position.x, groundObject.transform.position.y, transform.position.z)) < 1f)
            {

                FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/EnemyLaunching/EnemyLand", transform.position);

                if (!isDead)
                {
                    EnableComponents(true);
                }
                beenHit = false;
            }
        }

        void LateUpdate()
        {
            if (isDead) return;

            if (IsInChaseRange())
            {
                MoveToPlayer();

                if (IsInAttackRange())
                {
                    AttackBehaviour();
                }
                else
                {
                    anim.SetBool("isAttacking", false);
                }
            }
            else
            {
                PatrolBehaviour();
                timeOnCurrentPath += Time.deltaTime;
                timeSinceLastAttack += Time.deltaTime;
            }
        }

        private void MoveToPlayer()
        {
            anim.SetTrigger("idle");
            if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

            agent.isStopped = false;

            TurnToFace(target.transform.position);


            // This bit makes enemies move back a bit after attacking
            if (!hasAttacked)
            {
                agent.SetDestination(target.transform.position);

            }
            if (hasAttacked && timeSinceLastAttack < attackCooldown)
            {
                Flee();
               
            }
            else if (hasAttacked && timeSinceLastAttack > attackCooldown)
            {
                hasAttacked = false;
            }

            
            agent.speed = maxSpeed; // to change speeds when patrolling
            anim.SetTrigger("walk");

            if (Vector3.Distance(transform.position, target.transform.position) <= stoppingDist)
            {
                agent.isStopped = true;
            }
        }

        private void Flee()
        {

            Vector3 fleeVector = transform.position - target.transform.position;
            Vector3 fleePosition = transform.position + fleeVector * fleeDistance;
            agent.SetDestination(fleePosition);// * -1);
        }

        private void TurnToFace(Vector3 location)
        {
            Vector3 lookAtGoal = new Vector3(location.x - transform.position.x,
                                            0,
                                            location.z - transform.position.z);
            transform.rotation = Quaternion.Slerp(transform.rotation, 
                                            Quaternion.LookRotation(lookAtGoal),
                                            Time.deltaTime * rotSpeed);
        }

        private void AttackBehaviour()
        {
            if (!agent.isOnNavMesh) return;

            rb.freezeRotation = true;
            anim.ResetTrigger("idle");
            anim.SetBool("isAttacking", true);


            if (IsPlayerInLineOfSight())
            {
                bombDropper.DropBomb();

                // Uncomment to have enemies run away after dropping bomb
                
                //hasAttacked = true;
                //timeSinceLastAttack = 0;
            } 
           
        }

        private void PatrolBehaviour()
        {
            if (!agent.isOnNavMesh) return;

            rb.freezeRotation = false;

            if (agent.isActiveAndEnabled && !walkPointSet)
            {
                SearchWalkPoint();
            }

            if (walkPointSet)
            {
                agent.SetDestination(walkPoint);
                TurnToFace(walkPoint);
            }

            float distanceToWalkPoint = Vector3.Distance(transform.position, walkPoint);
            if (distanceToWalkPoint <= 1.0f || IsTakingTooLongOnCurrentpath()) // prevents getting stuck
            {
                walkPointSet = false;
                maxAllowedTimeSpecified = false;
                timeOnCurrentPath = 0;
            }
        }

        private bool SearchWalkPoint()
        {
            float randomX = Random.Range(-walkPointRange, walkPointRange);
            float randomZ = Random.Range(-walkPointRange, walkPointRange);

            walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

            if (Physics.Raycast(walkPoint, -Vector3.up, 2f, whatIsGround))
            {
                // TODO see if this nav mesh testing is necessary, could be good in case they choose a random spot on top of a pillar
                NavMeshHit hit;
                Vector3 testLocation;
                if (NavMesh.SamplePosition(walkPoint, out hit, 2.0f, NavMesh.AllAreas))
                {
                    testLocation = hit.position;

                    NavMeshPath path = new NavMeshPath();
                    bool hasPath = NavMesh.CalculatePath(transform.position, testLocation, NavMesh.AllAreas, path);

                    if (!hasPath)
                        return false;

                    if (path.status != NavMeshPathStatus.PathComplete)
                    {
                        return false;
                    }

                    if (GetPathLength(path) > maxNavPathLength)
                        return false;

                    walkPointSet = true;
                    return true;
                }
            }
            return false;
        }

        private float GetPathLength(NavMeshPath path)
        {
            float total = 0;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                total += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            }
            return total;
        }

        private bool IsPlayerInLineOfSight()
        {
            RaycastHit hitInfo;
            bool hasLineOfSight = false;
            if (Physics.Raycast(transform.position, target.transform.position - transform.position, out hitInfo, bombDropper.GetExplosionRadius()))
            {
                if (hitInfo.collider == target.GetComponent<Collider>())
                {
                    hasLineOfSight = true;
                }
            }
            return hasLineOfSight;
        }

        private bool IsTakingTooLongOnCurrentpath()
        {
            return timeOnCurrentPath > GetMaxTimeAllowedOnPath();
        }

        private float GetMaxTimeAllowedOnPath()
        {
            if (!maxAllowedTimeSpecified)
            {
                maxTimeOnCurrentPath =
                        Random.Range(minAllowedSecondsOnCurrentPath, maxAllowedSecondsOnCurrentPath);
                maxAllowedTimeSpecified = true;
            }
            return maxTimeOnCurrentPath;
        }

        public IEnumerator KnockbackCoroutine(float explosionForce, Vector3 sourcePosition, float radius)
        {
            EnableComponents(false);

            rb.AddExplosionForce(explosionForce, sourcePosition, radius, knockbackUpwardsModifier);

            FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/EnemyLaunching/EnemyLaunch", transform.position);

            beenHit = true;

            yield return true;

            /*yield return new WaitForSeconds(knockbackParalisisSeconds); // TODO could be the cause of future problems

            if (!isDead)
            {
                EnableComponents(true);
            }*/
        }

        private void EnableComponents(bool isEnabled)
        {
            rb.freezeRotation = isEnabled;
            agent.enabled = isEnabled;
            //rb.isKinematic = isEnabled;

            if (faceChanger != null)
            {
                faceChanger.ChangeAppearance(!isEnabled);
            }
        }

        private void OnDeath()
        {
            anim.SetBool("die", true);
            isDead = true;

            EnableComponents(false);

            gameObject.tag = "PhysicsObject"; // TODO make enemies disappear after death
        }

        private bool IsInAttackRange()
        {
            return Vector3.Distance(target.transform.position, transform.position) < attackRadius;
        }

        private bool IsInChaseRange()
        {
            return Vector3.Distance(target.transform.position, transform.position) < chaseRadius;
        }

        // private void OnDrawGizmosSelected()
        // {
        //     Gizmos.color = Color.blue;
        //     Gizmos.DrawWireSphere(transform.position, chaseRadius);

        //     Gizmos.color = Color.red;
        //     Gizmos.DrawWireSphere(transform.position, attackRadius);
        // }

        // Interface
        public void AffectByExplosion(float explosionForce, Vector3 sourcePosition, float radius)
        {
            StartCoroutine(KnockbackCoroutine(explosionForce, sourcePosition, radius));
        }

    }
}
