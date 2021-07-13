using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Panda;
using Bomber.Items;
using Bomber.Core;

public class BT_AIController : MonoBehaviour, IBombExplosion
{
    public Transform player;
    [SerializeField] float playerTooCloseDistance = 5f;
    [SerializeField] float fovAngle = 45f;
    [SerializeField] float attackDist = 4f;
    [SerializeField] float aggroCoolDownSecs = 3f;
    [SerializeField] float knockbackUpwardsModifier = 3.0f;

    NavMeshAgent agent;
    public Vector3 destination;
    Vector3 lastSeenPosition;
    BombDropper bomber;
    Health health;
    Rigidbody rb;
    EnemyFaceChanger faceChanger;

    GameObject groundObject = null;
    Vector3 target = Vector3.zero;
    float rotSpeed = 8.0f;
    float visibleRange = 20.0f;
    bool aggro = false;
    bool isFleeingFromBombs = false;
    float fleeMultiplier = 10.0f;
    float bombDetectionDist = 3.5f;
    bool beenHit = false;
    bool isDead = false;


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
            HitRecovery();
        }
    }

    private void HitRecovery()
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

    public void ApplyKnockback(float explosionForce, Vector3 sourcePosition, float radius)
    {
        EnableComponents(false);
        
        rb.AddExplosionForce(explosionForce, sourcePosition, radius, knockbackUpwardsModifier);

        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/EnemyLaunching/EnemyLaunch", transform.position);

        beenHit = true;

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
        //anim.SetBool("die", true);
        isDead = true;

        EnableComponents(false);

        gameObject.tag = "PhysicsObject"; // TODO make enemies disappear after death
    }


    [Task]
    public void PickRandomDestination()
    {
        int numTries = 30;
        float walkPointRange = 50f;

        for (int i = 0; i < numTries; i++)
        {
            float randomX = Random.Range(-walkPointRange, walkPointRange);
            float randomZ = Random.Range(-walkPointRange, walkPointRange);

            Vector3 walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

            if (Physics.Raycast(walkPoint, -Vector3.up, 2f,
                            LayerMask.NameToLayer("whatIsGround")))
            {
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
        var dst = this.transform.position + (Random.insideUnitSphere * range);
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
    private void TargetPlayer()
    {
        target = player.position;
        Task.current.debugInfo = string.Format("pos={0}", target.ToString());
        lastSeenPosition = target;
        Task.current.Succeed();
    }


    [Task]
    private void LookAtTarget()
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
    private void SetTargetAsDestination()
    {
        SetDestIfOnNavMesh(target);
        Task.current.Succeed();
    }


    [Task] bool IsInAttackRange()
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
            isFleeingFromBombs = false;
            return;
        }

        // TODO increase agent speed when fleeing
        Vector3 avgPos = SumPosition / numBombs;
        Vector3 fleeVector = transform.position - avgPos;
        Vector3 dest = transform.position + fleeVector.normalized * fleeMultiplier;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(dest, out hit, 3.0f, NavMesh.AllAreas))
        {
            target = hit.position;
            SetDestIfOnNavMesh(hit.position);

            isFleeingFromBombs = true;

            Task.current.Succeed();
        }

        if (Task.isInspected)
            Task.current.debugInfo = string.Format("isFleeing={0}", isFleeingFromBombs);
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

        Debug.DrawRay(transform.position, player.position - transform.position, Color.red);
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
