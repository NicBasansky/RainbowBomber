using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Panda;
using Bomber.Items;
using Bomber.Core;
using System;


public class BT_AIController : MonoBehaviour, IBombExplosion
{
    public Transform player;
    [SerializeField] float playerTooCloseDistance = 5f;
    [SerializeField] float fovAngle = 45f;
    [SerializeField] float attackDist = 4f;
    [SerializeField] float aggroCoolDownSecs = 3f;
    [SerializeField] float knockbackUpwardsModifier = 3.0f;
    [SerializeField] GameObject shadowGo;
    [SerializeField] Transform pickupTransform;
    [SerializeField] float maxThrowingDistance = 15.0f;
    


    NavMeshAgent agent;
    public Vector3 destination;
    Vector3 lastSeenPosition;
    BombDropper bomber;
    Health health;
    Rigidbody rb;
    EnemyFaceChanger faceChanger;
    GameObject closestBomb = null;
    GameObject pickedUpBomb = null;
    bool isHoldingBomb = false;

    GameObject groundObject = null;
    Vector3 target = Vector3.zero;
    float rotSpeed = 11.0f;
    float pickupSpeed = 5f;
    float bombPickupDist = 3.5f;
    float visibleRange = 20.0f;
    bool aggro = false;
    bool isFleeingFromBombs = false;
    float fleeMultiplier = 10.0f;
    float bombDetectionDist = 3.5f;
    bool beenHit = false;
    bool isDead = false;
    float recoveryTimer = Mathf.Infinity;
    float recoveryDelay = 2.0f;


    private void OnEnable()
    {
        GetComponent<Health>().onDeath += OnDeath;
    }

    private void OnDisable()
    {
        GetComponent<Health>().onDeath += OnDeath;
    }

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        bomber = GetComponent<BombDropper>();
        health = GetComponent<Health>();
        rb = GetComponent<Rigidbody>();
        faceChanger = GetComponent<EnemyFaceChanger>();
        groundObject = GameObject.FindGameObjectWithTag("whatIsGround");
    }

    void Start()
    {
        //TODO appropriate for bomb dropping?
        agent.stoppingDistance = attackDist - 2; // For a little buffer 

    }

    private void Update()
    {
        if (beenHit)
        {
            recoveryTimer += Time.deltaTime;
            HitRecovery();
        }
    }

    

    private void HitRecovery()
    {
        if (recoveryTimer > recoveryDelay)
        {
            if (Physics.Raycast(transform.position, -transform.up, 150.0f, LayerMask.NameToLayer("whatIsGround"))) // if above the ground
            {
                if (Vector3.Distance(transform.position, new Vector3(transform.position.x, groundObject.transform.position.y, transform.position.z)) < 2f)
                {

                    FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/EnemyLaunching/EnemyLand", transform.position);

                    if (!isDead)
                    {
                        EnableComponents(true);
                    }

                    beenHit = false;
                }
            }


        }
    }

    public void ApplyKnockback(float explosionForce, Vector3 sourcePosition, float radius)
    {

        EnableComponents(false);

        rb.AddExplosionForce(explosionForce, sourcePosition, radius, knockbackUpwardsModifier);

        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/EnemyLaunching/EnemyLaunch", transform.position);

        beenHit = true;
        recoveryTimer = 0;
        //yield return true;

        //yield return new WaitForSeconds(knockbackParalisisSeconds);
        // TODO could be the cause of future problems

        //if (!isDead)
        //{
        //    EnableComponents(true);
        //}
    }

    private void EnableComponents(bool isEnabled)
    {
        rb.isKinematic = isEnabled;
        //rb.detectCollisions = isEnabled; // was inverted before

        rb.freezeRotation = isEnabled;

        shadowGo.SetActive(isEnabled);
        agent.enabled = isEnabled;


        if (faceChanger != null)
        {
            faceChanger.ChangeAppearance(!isEnabled);
        }
    }

    private void OnDeath()
    {
        //anim.SetBool("die", true);
        isDead = true;

        EnableComponents(false);

        gameObject.tag = "PhysicsObject"; // TODO make enemies disappear after death
    }

    private bool AreBombsInRange(float distance)
    {
        var bombs = ActiveBombManager.Instance.GetAllActiveBombs();
        if (bombs.Count == 0)
        {
            return false;
        }
        foreach (var b in bombs)
        {
            if (Vector3.Distance(transform.position, b.transform.position) > distance)
                continue;
            else
            {
                return true;
            }
        }
        return false;
    }

    private GameObject GetClosestBomb()
    {
        var bombs = ActiveBombManager.Instance.GetAllActiveBombs();
        if (bombs.Count == 0)
        {
            return null;
        }
        float closestDist = Mathf.Infinity;
        GameObject closeBomb = null;
        foreach (var b in bombs)
        {
            float distance = Vector3.Distance(transform.position, b.transform.position);
            if (distance > bombPickupDist)
            {
                continue;

            }
            else if (distance < closestDist)
            {
                closestDist = distance;
                closeBomb = b;
            }

        }
        if (closeBomb == null)
        {
            return null;
        }

        closestBomb = closeBomb;

        return closeBomb;

    }


    [Task]
    public void PickRandomDestination()
    {
        int numTries = 30;
        float walkPointRange = 30f;

        for (int i = 0; i < numTries; i++)
        {
            float randomX = UnityEngine.Random.Range(-walkPointRange, walkPointRange);
            float randomZ = UnityEngine.Random.Range(-walkPointRange, walkPointRange);

            Vector3 walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

            //if (Physics.Raycast(walkPoint, -Vector3.up, 2f,
                            //LayerMask.NameToLayer("whatIsGround")))
            {
                NavMeshPath path = new NavMeshPath();
                NavMesh.CalculatePath(transform.position, walkPoint, NavMesh.AllAreas, path);
                if (path.status != NavMeshPathStatus.PathComplete)
                {
                    continue;
                }

                if (Task.isInspected)
                {
                    Task.current.debugInfo = string.Format("Dest:{0}", walkPoint);
                }

                destination = walkPoint;

                SetDestIfOnNavMesh(walkPoint);
                Task.current.Succeed();
                break;
            }
        }
    }

    [Task]
    public bool SetDestination_RandomInRange(float range)
    {
        var dst = this.transform.position + (UnityEngine.Random.insideUnitSphere * range);
        SetDestIfOnNavMesh(dst);

        if (Task.isInspected)
        {
            Task.current.debugInfo = string.Format("({0}, {1})",
                    dst.x, dst.z);
            return true;
        }
        return false;

    }


    [Task]
    public void MoveToDestination()
    {
        if (Task.isInspected)
            Task.current.debugInfo = string.Format("t={0:0.00}", Time.time);


        if (agent.isOnNavMesh && agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            Task.current.Succeed();
        }
    }


    [Task]
    public void TargetPlayer()
    {
        target = player.position;
        Task.current.debugInfo = string.Format("pos={0}", target.ToString());
        lastSeenPosition = target;
        Task.current.Succeed();
    }

    [Task]
    public void TargetPlayerOvershot()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        target = player.position + direction * 6f; // overshoot by float
        Task.current.debugInfo = string.Format("pos={0}", target.ToString());
        lastSeenPosition = target;
        Task.current.Succeed();
    }


    [Task]
    public void LookAtTarget()
    {
        Vector3 direction = target - transform.position;
        transform.rotation = Quaternion.Slerp(transform.rotation,
                                        Quaternion.LookRotation(direction),
                                        rotSpeed * Time.deltaTime);
        if (Task.isInspected)
        {
            Task.current.debugInfo = string.Format("angle={0}", Vector3.Angle(transform.forward, direction));
        }

        if (Vector3.Angle(transform.forward, direction) <= 20.0f)
        {
            Task.current.Succeed();
        }
    }


    [Task]
    public void SetTargetAsDestination()
    {
        SetDestIfOnNavMesh(target);
        Task.current.Succeed();
    }


    [Task] public bool IsInAttackRange()
    {
        Vector3 distance = player.position - transform.position;
        if (distance.sqrMagnitude < (attackDist * attackDist))
        {
            Task.current.debugInfo = string.Format("InAttackRange={0}", distance.sqrMagnitude < (attackDist * attackDist));
            return true;
        }
        else
        {
            Task.current.debugInfo = string.Format("InAttackRange={0}", distance.sqrMagnitude < (attackDist * attackDist));
            return false;
        }
    }


    [Task] bool BombsNearToPickup()
    {
        return AreBombsInRange(bombPickupDist);

    }

    [Task] void TargetClosestBomb()
    {
        if (isHoldingBomb)
        {
            Task.current.Succeed();
            return;
        }
        GameObject bomb = GetClosestBomb();
        if (bomb == null)
        {
            Task.current.Fail();
            return;
        }
        target = bomb.transform.position;
        Task.current.Succeed();
    }

    [Task] void PickupBomb()
    {
        if (!closestBomb.activeSelf) // if the bomb is no longer active
        {
            isHoldingBomb = false;
            Task.current.Fail();
            return;
        }
        if (isHoldingBomb)
        {
            Task.current.Succeed();
            return;
        }
      
        closestBomb.GetComponent<Rigidbody>().isKinematic = true;

        closestBomb.transform.rotation = Quaternion.identity;
        closestBomb.transform.position = Vector3.Lerp(closestBomb.transform.position, pickupTransform.position, Time.deltaTime * pickupSpeed);

        pickedUpBomb = closestBomb;

        if (Vector3.Distance(pickedUpBomb.transform.position, pickupTransform.position) < 1.5f)
        {
            pickedUpBomb.transform.parent = pickupTransform;
            isHoldingBomb = true;
            Task.current.Succeed();
        }
    }

    [Task] bool IsPlayerInThrowingDistance()
    {
        return Vector3.Distance(player.transform.position, transform.position) < maxThrowingDistance;
    }

    //[Task] bool IsHoldingBomb()
    //{
    //    return isHoldingBomb;
    //}


    //float throwForce = 50.0f;
    [Task] void ThrowBomb()
    {
        if (!pickedUpBomb.activeSelf)
        {
            Task.current.Fail();
            isHoldingBomb = false;
            return;
        }


        float gravity = Physics.gravity.magnitude;
        float initialAngle = 35f;
        float angle = initialAngle * Mathf.Deg2Rad;
        //float overshootAmount = 8.0f;
        //// Get the direction vector so bomb can slightly overshoot the player
        //Vector3 direction = (player.transform.position - pickedUpBomb.transform.position).normalized;
        Vector3 targetPos = new Vector3(player.position.x, groundObject.transform.position.y,
                                            player.position.z);

        // Positions of this object and the target on the same plane
        Vector3 planarTarget = new Vector3(targetPos.x, groundObject.transform.position.y, targetPos.z);
        Vector3 planarPosition = new Vector3(pickedUpBomb.transform.position.x, groundObject.transform.position.y, pickedUpBomb.transform.position.z);

        if (Task.isInspected)
        {
            Task.current.debugInfo = string.Format("targetPos:{0}", planarTarget);
        }


        // Planar distance between objects
        float distance = Vector3.Distance(planarTarget, planarPosition);
        // distance along the y axis between objects
        float yOffset = transform.position.y - targetPos.y;

        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity *
                        Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));
        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

        // Rotate our velocity to match the direction between the two objects
        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPosition);
        Vector3 finalVelocity;
        if (targetPos.x - pickedUpBomb.transform.position.x < 0) // if target is to the left then inverse the angle
        {
            finalVelocity = Quaternion.AngleAxis(angleBetweenObjects * -1, Vector3.up) * velocity;
        }
        else
        {
            finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;
        }

        pickedUpBomb.transform.parent = null;
        pickedUpBomb.GetComponent<Rigidbody>().isKinematic = false;

        // fire
        pickedUpBomb.GetComponent<Rigidbody>().velocity = finalVelocity * 1.3f; // overshoot a little;

        isHoldingBomb = false;

        Task.current.Succeed();
    }

    [Task] void FleeFromBombs()
    {
        var bombs = ActiveBombManager.Instance.GetAllActiveBombs();
        if (bombs.Count == 0)
        {
            Task.current.Fail();
            isFleeingFromBombs = false;
            return;
        }

        Vector3 SumPosition = Vector3.zero;
        int numBombs = 0;
        foreach (var b in bombs)
        {
            if (Vector3.Distance(transform.position, b.transform.position) > bombDetectionDist)
                continue;

            SumPosition += b.transform.position;
            numBombs++;
        }

        if (numBombs == 0)
        {
            Task.current.Fail();
            //isFleeingFromBombs = false;
            return;
        }

        if (Task.isInspected)
            Task.current.debugInfo = string.Format("isFleeing={0}", isFleeingFromBombs);

        // TODO increase agent speed when fleeing
        Vector3 avgPos = SumPosition / numBombs;
        Vector3 fleeVector = transform.position - avgPos;
        for (int i = 0; i < 10; i++)
        {
            Vector3 dest = GetRandomOffsetPosition(transform.position) + fleeVector.normalized * fleeMultiplier;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(dest, out hit, 3.0f, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();
                if (NavMesh.CalculatePath(transform.position, dest, NavMesh.AllAreas, path))
                {
                    if (path.status == NavMeshPathStatus.PathComplete)
                    {
                        target = hit.position;
                        SetDestIfOnNavMesh(hit.position);

                        isFleeingFromBombs = true;
                        Invoke("ResetIsFleeingFromBombs", UnityEngine.Random.Range(3.5f, 5.0f));

                        Task.current.Succeed();
                        return;
                    }               
                }
            }
        }
        Task.current.Fail();
    }

    private void ResetIsFleeingFromBombs()
    {
        isFleeingFromBombs = false;
    }

    private Vector3 GetRandomOffsetPosition(Vector3 position)
    {
        float range = 6f;
        Vector3 offset = new Vector3(position.x + UnityEngine.Random.Range(-range, range), position.y + UnityEngine.Random.Range(-range, range), position.z + UnityEngine.Random.Range(-range, range));
        return offset;
    }

    [Task] bool ShotLinedUp()
    {
        Vector3 distance = target - transform.position;
        if (distance.magnitude < attackDist &&
                Vector3.Angle(transform.forward, distance) < 5.0f)
        {
            return true;
        }
        else
            return false;
    }


    [Task]
    private bool Attack()
    {
        // TODO cool down time or wait node in the BT
        bomber.DropBomb();
        return true;
    }


    [Task]
    private bool IsPlayerInFOV()
    {
        Vector3 distance = player.position - transform.position;

        if (Vector3.Angle(transform.forward, distance) < fovAngle
                    && distance.sqrMagnitude < (visibleRange * visibleRange)) // sqrMag is more performant
        {
            return true;
        }
        return false;
    }

    [Task]
    private bool CanSeePlayer()
    {
        Vector3 distance = player.position - transform.position;
        RaycastHit hit;
        bool seeWall = false;

        if (Physics.Raycast(transform.position, distance, out hit))
        {
            if (hit.collider.gameObject.tag == "Environment")
            {
                seeWall = true;
            }
        }
        if (Task.isInspected)
        {
            Task.current.debugInfo = string.Format("seeWall={0}", seeWall);
        }

        if (distance.magnitude < visibleRange && !seeWall)
        {
            //Debug.DrawRay(transform.position, player.position - transform.position, Color.red);
            return true;
        }
        else
            return false;
    }

    [Task]
    private void TargetLastSeenPosition()
    {
        target = lastSeenPosition;
        Task.current.Succeed();
    }

    [Task]
    private bool Turn(float angle)
    {
        var p = transform.position + Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
        target = p;

        return true;
    }

    [Task]
    private bool IsHealthLessThan(float amount)
    {
        return this.health.health < amount;
    }

    [Task]
    private bool IsPlayerTooClose()
    {
        Vector3 distance = player.position - transform.position;
        return (distance.magnitude < playerTooCloseDistance);
    }

    [Task]
    private void Flee() // TODO take cover from bombs as well
    {
        Vector3 awayFromPlayer = transform.position - player.position;
        Vector3 dest = transform.position + awayFromPlayer.normalized * fleeMultiplier;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(dest, out hit, 3f, NavMesh.AllAreas))
        {
            target = hit.position;
            SetDestIfOnNavMesh(hit.position);

            Task.current.Succeed();
        }
        //agent.SetDestination(dest);
        //Task.current.Succeed();


    }

    [Task]
    private bool IsAmmoLessThan(int num)
    {
        return bomber.GetCurrentNumBombs() < num;
    }

    [Task]
    private void Die()
    {
        
        // TODO
        Task.current.Succeed();
    }

    [Task]
    void Aggrovate()
    {
        aggro = true;

        if (Task.isInspected)
            Task.current.debugInfo = string.Format("isAggro={0}", aggro);

        Task.current.Succeed();
    }

    [Task]
    private bool IsAggro()
    {
        return aggro;
    }

    [Task]
    private void ClearEnemy()
    {
        aggro = false;
        Task.current.Succeed();
    }

    [Task]
    private bool BombsNearby()
    {
        bool near = false;
        if (Task.isInspected)
            Task.current.debugInfo = string.Format("bombsNearby={0}", near);

        var bombs = ActiveBombManager.Instance.GetAllActiveBombs();
        if (bombs.Count == 0)
        {
            return false;      
        }

        int numBombs = 0;
        foreach (var b in bombs)
        {
            if (Vector3.Distance(transform.position, b.transform.position) > bombDetectionDist + 5.0f)
                continue;

            numBombs++;

            if (numBombs > 0)
            {
                near = true;
                return true;
            }
        }
        return false;
    }

    [Task] // TODO remove
    private bool IsFleeingFromBombs()
    {
        return isFleeingFromBombs;
    }

    private bool SetDestIfOnNavMesh(Vector3 dest)
    {
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(dest);
            return true;
        }
        return false;
    }

    // Interface
    public void AffectByExplosion(float explosionForce, Vector3 sourcePosition, float radius)
    {
        ApplyKnockback(explosionForce, sourcePosition, radius);
    }
}
