using Bomber.Control;
using Bomber.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;

public class AI_SmallSlime : BT_AIController
{
    [SerializeField] float contactDamage = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Health health = other.GetComponent<Health>();
            health.AffectHealth(-contactDamage);
            //other.GetComponent<PlayerController>().SlowSpeed();
        }
    }


    //[Task]
    //public override void PickRandomDestination()
    //{
    //    base.PickRandomDestination();
    //}

    //[Task]
    //public override bool SetDestination_RandomInRange(float range)
    //{
    //    return base.SetDestination_RandomInRange(range);

    //}


    //[Task]
    //public override void MoveToDestination()
    //{
    //    base.MoveToDestination();
    //}


    //[Task]
    //public override void TargetPlayer()
    //{
    //    base.TargetPlayer();
    //}


    //[Task]
    //public override void LookAtTarget()
    //{
    //    base.LookAtTarget();
    //}


    //[Task]
    //public override void SetTargetAsDestination()
    //{
    //    base.SetTargetAsDestination();
    //}


    //[Task]
    //public override bool IsInAttackRange()
    //{
    //    return base.IsInAttackRange();
    //}

  

}
