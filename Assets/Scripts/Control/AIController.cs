using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Bomber.Items;
using Random = UnityEngine.Random;
using Bomber.Core;

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
        public LayerMask whatIsGround;

        [Header("Knockback")]
        // [SerializeField] float knockbackPower = 10f;
        // [SerializeField] float knockbackRadius = 3f;
        [SerializeField] float knockbackUpwardsModifier = 3.0f;
        [SerializeField] float knockbackParalisisSeconds = 1.5f;

        BombDropper bombDropper = null;
        GameObject target;
        bool attacking = false;
        bool isPatrolling = false;
        bool maxAllowedTimeSpecified = false;
        Vector3 walkPoint;
        bool walkPointSet = false;
        float timeOnCurrentPath = Mathf.Infinity;
        float maxTimeOnCurrentPath = 0;
        bool hitByPhysics = false;
        bool isDead = false;

        NavMeshAgent agent;
        Rigidbody rb;
        Animator anim;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            anim = GetComponent<Animator>();
            bombDropper = GetComponent<BombDropper>();
            rb = GetComponent<Rigidbody>();
            target = GameObject.FindWithTag("Player");
        }

        private void OnEnable()
        {
            GetComponent<Health>().onDeath += OnDeath;
        }

        private void OnDisable()
        {
            GetComponent<Health>().onDeath += OnDeath;
        }

        void Update()
        {
            if (isDead) return;

            if (IsInChaseRange())
            {
                MoveTo();

                if (IsInAttackRange())
                {
                    isPatrolling = false;
                    AttackBehaviour();
                }
                else
                {
                    anim.SetBool("isAttacking", false);
                }
            }
            else
            {
                // IdleBehaviour();
                PatrolBehaviour();
                timeOnCurrentPath += Time.deltaTime;

            }

        }

        private void IdleBehaviour()
        {
            anim.SetTrigger("idle");
            // todo reset attack animation?
        }

        private void AttackBehaviour()
        {
            attacking = true;
            transform.LookAt(target.transform.position);
            anim.ResetTrigger("idle");
            anim.SetBool("isAttacking", true);

            if (IsPlayerInLineOfSight())
            {
                bombDropper.DropBomb();
            }

            // move away?
        }

        private void PatrolBehaviour()
        {
            if (!agent.isOnNavMesh) return;

            hitByPhysics = false;

            if (agent.isActiveAndEnabled && !walkPointSet)
            {
                SearchWalkPoint();
            }

            if (walkPointSet)
            {
                agent.SetDestination(walkPoint);
                transform.LookAt(walkPoint);
            }

            float distanceToWalkPoint = Vector3.Distance(transform.position, walkPoint);
            if (distanceToWalkPoint <= 1.0f || IsTakingTooLongOnCurrentpath())
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

            walkPoint = new Vector3(transform.position.x + randomX, 21.0f, transform.position.z + randomZ);

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

        private void MoveTo()
        {
            if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

            agent.isStopped = false;
            agent.SetDestination(target.transform.position);
            agent.speed = maxSpeed; // to change speeds when patrolling
            anim.SetTrigger("walk");

            if (Vector3.Distance(transform.position, target.transform.position) <= stoppingDist)
            {
                agent.isStopped = true;
            }
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
            hitByPhysics = true;
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            agent.enabled = false;
            anim.enabled = false;
            rb.AddExplosionForce(explosionForce, sourcePosition, radius, knockbackUpwardsModifier);

            yield return new WaitForSeconds(knockbackParalisisSeconds); // TODO could be the cause of future problems

            agent.enabled = true;
            anim.enabled = true;
        }

        private void OnDeath()
        {
            agent.enabled = false;
            anim.SetBool("die", true);
            isDead = true;
        }

        private bool IsInAttackRange()
        {
            return Vector3.Distance(target.transform.position, transform.position) < attackRadius;
        }

        private bool IsInChaseRange()
        {
            return Vector3.Distance(target.transform.position, transform.position) < chaseRadius;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, chaseRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRadius);
        }

        public void AffectByExplosion(float explosionForce, Vector3 sourcePosition, float radius)
        {
            if (!hitByPhysics)
            {
                StartCoroutine(KnockbackCoroutine(explosionForce, sourcePosition, radius));
            }
        }


    }
}
