using UnityEngine;
using UnityEngine.AI;

public class NavMeshNPC : MonoBehaviour
{
    // the fsm states
    public enum NPCStates
    {
        Patrol,
        Chase,
        Attack,
        Investigate,
        SeekCover
    }

    [Header("FSM")] public NPCStates currentState = NPCStates.Patrol;

    [SerializeField] Transform[] PatrolPoints;
    [SerializeField] Transform Player;
    public float ChaseRange = 7f;
    public float AttackRange = 4f;
    private int currentPatrolIndex = 0;
    private NavMeshAgent navMeshAgent;
    private BaseNPC body;

    private Vector3 lastKnownPlayerPosition;

    [Header("Combat - Cover")] public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float fireRate = 1.5f;
    public Transform[] CoverPoints;
    public int lowHealthThreshold = 30;
    private float nextShootTime = 0f;


    void Start()
    {
        // get the NavMeshAgent component from Unity which was assigned
        navMeshAgent = GetComponent<NavMeshAgent>();

        // something like dependency injection in spring boot
        body = GetComponent<BaseNPC>();

        /* get the npc to move to the first patrol point
         we check if the number of patrol points are greater than zero (safety check) */
        if (PatrolPoints.Length > 0)
        {
            navMeshAgent.SetDestination(PatrolPoints[currentPatrolIndex].position);
        }
        else
        {
            Debug.LogError("You have zero patrol points! Add more!");
        }

        // ?
        UpdateStateUI();
    }

    void Update()
    {
        // every frame, check criteria to seek cover. We use the third argument here to check if the npc is already in SeekCover state
        if (body.currentHealth > 0 && body.currentHealth <= lowHealthThreshold && currentState != NPCStates.SeekCover)
        {
            ChangeState(NPCStates.SeekCover);
        }

        // evaluate the state every frame
        switch (currentState)
        {
            case NPCStates.Patrol:
                Patrol();
                break;
            case NPCStates.Chase:
                Chase();
                break;
            case NPCStates.Attack:
                Attack();
                break;
            case NPCStates.Investigate:
                Investigate();
                break;
            case NPCStates.SeekCover:
                SeekCover();
                break;
            default:
                Patrol();
                break;
        }
    }

    private void Patrol()
    {
        if (CanSeePlayer())
        {
            ChangeState(NPCStates.Chase);
            return;
        }
        
        /* when the path finding is done to the destination and the distance to the target location is close enough
         change to the next patrol position */
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % PatrolPoints.Length;
            navMeshAgent.SetDestination(PatrolPoints[currentPatrolIndex].position);
        }
    }

    private void Chase()
    {
        if (CanSeePlayer())
        {
            float distance = Vector3.Distance(transform.position, Player.position);
            
            Debug.Log("Distance to player is: " + distance);

            if (distance <= AttackRange)
            {
                ChangeState(NPCStates.Attack);
                return;
            }

            // save the players last known position
            lastKnownPlayerPosition = Player.position;

            // go to the player
            navMeshAgent.SetDestination(Player.position);
        }
        else
        {
            ChangeState(NPCStates.Investigate);
            navMeshAgent.SetDestination(lastKnownPlayerPosition);
        }
    }

    private void Attack()
    {
        navMeshAgent.isStopped = true;

        // calculate the exact direction to the players position
        Vector3 directionToPlayer = (Player.position - transform.position).normalized;
        
        // prevent the npc from "looking up" at the player
        directionToPlayer.y = 0;
        
        // rotate the npc towards the player's direction
        transform.rotation = Quaternion.LookRotation(directionToPlayer);

        if (!CanSeePlayer())
        {
            navMeshAgent.isStopped = false;
            ChangeState(NPCStates.Investigate);
            return;
        }

        if (Vector3.Distance(transform.position, Player.position) > AttackRange)
        {
            navMeshAgent.isStopped = false;
            ChangeState(NPCStates.Chase);
            return;
        }

        /* we add the fire rate to the master clock in the game
         so that the gun has a cooldown period, rather than repeatedly shooting */
        if (Time.time >= nextShootTime)
        {
            Shoot();
            nextShootTime = Time.time + fireRate;
        }
    }

    private void Investigate()
    {
        if (CanSeePlayer())
        {
            ChangeState(NPCStates.Chase);
            return;
        }

        /* when the path finding is done to the destination and the distance to the target location is close enough
         change to patrol */
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            ChangeState(NPCStates.Patrol);
        }
    }

    private void SeekCover()
    {
        navMeshAgent.isStopped = false;

        Transform bestCover = null;
        float farthestDistance = 0f;

        /* we loop through each cover point added and
         we calculate the distance from the player to the cover positions and we save it.
         The position that is furthest away, is the best position */
        foreach (Transform cover in CoverPoints)
        {
            float distFromPlayer = Vector3.Distance(Player.position, cover.position);

            if (distFromPlayer > farthestDistance)
            {
                farthestDistance = distFromPlayer;
                bestCover = cover;
            }
        }

        // npc travels to the best cover
        if (bestCover != null)
        {
            navMeshAgent.SetDestination(bestCover.position);
        }
    }

    // shooting logic
    private void Shoot()
    {
        if (bulletPrefab != null && bulletSpawnPoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

            bullet.GetComponent<Rigidbody>().linearVelocity = bulletSpawnPoint.forward * 15f;
        }
    }

    private bool CanSeePlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);

        if (distanceToPlayer <= ChaseRange)
        {
            Vector3 directionToPlayer = (Player.position - transform.position).normalized;

            // draw a ray cast to the players direction and if it is a Player tag, return true
            if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, ChaseRange))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void ChangeState(NPCStates newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            UpdateStateUI();
        }
    }

    // 
    private void UpdateStateUI()
    {
        // update the baseNPC's UI
        if (body != null)
        {
            body.currentState = currentState.ToString().ToUpper();
            body.UpdateUI();
        }
    }
}