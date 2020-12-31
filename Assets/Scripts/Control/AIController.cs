using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Bomber.Control
{
    public class AIController : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] float chaseRadius = 5f;
        [SerializeField] float stoppingDist = 1.5f; // must be less than attackRadius
        [SerializeField] float attackRadius = 2f;
        [SerializeField] float maxSpeed = 7.0f;


        bool attacking = false;
        NavMeshAgent agent;
        Animator anim;

        // Start is called before the first frame update
        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            anim = GetComponent<Animator>();
        }

        void Update()
        {
            if (IsInChaseRange())
            {
                MoveTo();

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
                IdleBehaviour();
            }

        }

        private void IdleBehaviour()
        {
            //attacking = false;
            anim.SetTrigger("idle");
            // todo reset attack animation?
        }

        private void AttackBehaviour()
        {
            attacking = true;
            transform.LookAt(target.position);
            anim.ResetTrigger("idle");
            anim.SetBool("isAttacking", true);
        }

        private void MoveTo()
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);
            agent.speed = maxSpeed; // to change speeds when patrolling
            anim.SetTrigger("walk");

            if (Vector3.Distance(transform.position, target.transform.position) <= stoppingDist)
            {
                agent.isStopped = true;
            }
        }

        private bool IsInAttackRange()
        {
            return Vector3.Distance(target.position, transform.position) < attackRadius;
        }

        private bool IsInChaseRange()
        {
            return Vector3.Distance(target.position, transform.position) < chaseRadius;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, chaseRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRadius);
        }
    }
}
