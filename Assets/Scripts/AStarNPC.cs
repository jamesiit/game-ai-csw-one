using UnityEngine;

// the five states
public enum  AStarState
{
    Roam,
    Alert,
    Flank,
    BurstAttack,
    RetreatHeal
}
public class AStarNPC : MonoBehaviour
{
    public AStarState currentState = AStarState.Roam;
    
    [Header("Links")] 
    public BaseNPC body;
    public Grid grid;
    // we randomly pick a position
    public Transform dummyTarget;
    public Transform player;

    // we use these variables to give the npc clean thinking pauses
    [Header("Roam Settings")] 
    public float waitTime = 2f;
    public float waitTimer = 0f;
    private bool isWaiting = false;
    private float flankTimer = 0f;

    [Header("Tactical Settings")] 
    public float visionRange = 15f;

    private float alertDuration = 1.5f;
    private float alertTimer = 0f;
    private PathFinding pathFinder;
    private Seeker seeker;

    // burst attack variables
    [Header("Burst Attack Settings")] 
    public GameObject bulletPrefab;

    public Transform bulletSpawnPoint;
    public float timeBetweenShots = 0.2f;

    private int burstCount = 0;
    private float shootTimer = 0f;
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        body = GetComponent<BaseNPC>();
        seeker = GetComponent<Seeker>();
        grid = FindFirstObjectByType<Grid>();
        pathFinder = FindFirstObjectByType<PathFinding>();
        
        PickRandomRoamPoint();
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case AStarState.Roam:
                Roam();
                break;
            
            case AStarState.Alert:
                Alert();
                break;
            
            case AStarState.Flank:
                Flank();
                break;
            
            case AStarState.BurstAttack:
                BurstAttack();
                break;
            
            case AStarState.RetreatHeal:
                RetreatHeal();
                break;
        }
    }

    void Roam()
    {
        UpdateUI("ROAMING (A*)");
        
        CheckForPlayer();

        // set the speed of movement
        seeker.speed = 3f;

        // check distance from npc to the dummy target
        float distanceToTarget = Vector3.Distance(transform.position, dummyTarget.position);

        // if in waiting mode, start a countdown
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0)
            {
                // once the countdown is 0, switch to walk and pick a new destination
                isWaiting = false;
                PickRandomRoamPoint();
            }
        }
        
        // if it has reached the dummyTarget, turn on waiting mode and wind up the stopwatch
        else if (distanceToTarget <= 1.5f)
        {
            isWaiting = true;
            waitTimer = waitTime;
        }
    }

    void PickRandomRoamPoint()
    {
        // we include a safety net here to limit the number of guesses and prevent unity from crashing
        bool foundPoint = false;
        int safetyNet = 0;

        while (!foundPoint && safetyNet < 100)
        {
            safetyNet++;

            // get random coordinates on the grid
            int randomX = Random.Range(0, grid.gridSizeX);
            int randomY = Random.Range(0, grid.gridSizeY);

            // assign the values to a Node
            Node randomNode = grid.nodeGrid[randomX, randomY];

            // if the node is walkable, transport it to the location of the node
            if (randomNode.walkable)
            {

                // accept the point only if the A* can actually build a route to it
                var testPath = pathFinder.FindPath(transform.position, randomNode.worldPosition);
                if (testPath != null && testPath.Count > 0)
                {
                    dummyTarget.position = randomNode.worldPosition;
                    foundPoint = true;
                }
                
                
            }
        }
    }

    void CheckForPlayer()
    {
        //
        if (player == null)
        {
            Debug.LogError("Player GameObject not found!");
            return;
        }
        
        // check distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // if the distance to the player is lesser than the range we set, 
        // 
        if (distanceToPlayer <= visionRange)
        {
            // calculate direction to the player
            Vector3 directionToPlayer = (player.position - transform.position).normalized;

            // project a raycast
            if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, visionRange))
            {
                // if it hits and the tag is player, set the state to alert
                if (hit.collider.CompareTag("Player"))
                {
                    
                    // state flips to alert and the clock winds up again
                    currentState = AStarState.Alert;
                    alertTimer = alertDuration;

                    // freeze the NPC
                    dummyTarget.position = transform.position;
                }
            }
        }
    }

    void Alert()
    {
        UpdateUI("ALERT");

        // stop the movement
        seeker.speed = 0f;
        
        // give the npc some thinking state where it looks at the player and rotates
        Vector3 lookDir = (player.position - transform.position).normalized;
        lookDir.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDir);

        // wind up the stopwatch
        alertTimer -= Time.deltaTime;

        /* if the timer goes below zero, run a raycast: 
        if the player is detected, flank. Else, go back to roam state */
        if (alertTimer <= 0)
        {
            // check direction to the player
            Vector3 dirToPlayer = (player.position - transform.position).normalized;

            // project a raycast
            if (Physics.Raycast(transform.position, dirToPlayer, out RaycastHit hit, visionRange))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    CalculateFlankPosition();
                    
                    /* give the npc 4 seconds to reach the spot
                    this is a check to make sure it doesn't freeze in place if something goes wrong! */
                    flankTimer = 4f;
                    
                    currentState = AStarState.Flank;
                    return;
                }
            }
            
            // if the raycast has hit a brick wall, it means we (player) have hidden well
            UpdateUI("LOST TARGET");
            currentState = AStarState.Roam;


        }
    }

    void Flank()
    {
        UpdateUI("FLANKING");
        
        // increase the speed from 3f to 6f
        seeker.speed = 6f;

        // count down the flank timer
        flankTimer -= Time.deltaTime;

        if (flankTimer <= 0f)
        {
            MakeTacticalDecision();
            return;
        }
        
        // flatten coordinates to the floor
        Vector3 flattenNPCPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flattenTarget = new Vector3(dummyTarget.position.x, 0, dummyTarget.position.z);
        
        float distanceToFlank = Vector3.Distance(flattenNPCPos, flattenTarget);

        // when it gets in the range of the flank position, update UI
        if (distanceToFlank <= 2.0f)
        {
            MakeTacticalDecision();
        }
    }
    
    void CalculateFlankPosition()
    {
        // calculate the right flank position 5 units to the right of the player and validate that its walkable
        Node rightNode = grid.NodeFromWorldPoint(player.position + (player.right * 5f));

        if (rightNode.walkable)
        {
            dummyTarget.position = rightNode.worldPosition;
            return;
        }
        
        // calculate the left flank position 5 units to the right of the player and validate that its walkable
        Node leftNode = grid.NodeFromWorldPoint(player.position - (player.right * 5f));

        if (leftNode.walkable)
        {
            dummyTarget.position = leftNode.worldPosition;
            return;
        }
        
        // try the player's own tile and validate
        Node playerNode = grid.NodeFromWorldPoint(player.position);

        if (playerNode.walkable)
        {
            dummyTarget.position = playerNode.worldPosition;
            return;
        }
        
        // if player is next to a wall, pick the first walkable tile right next to them
        foreach (Node neighbour in grid.GetNeighbours(playerNode))
        {
            if (neighbour.walkable)
            {
                dummyTarget.position = neighbour.worldPosition;
                return;
            }
        }
        
        // the last resort where the npc stays put so seeker skips pathfinding cleanly
        dummyTarget.position = transform.position;

    }

    void UpdateUI(string stateName)
    {
        if (body == null)
        {
            Debug.LogError("Could not load BaseNPC script!");
            return;
        }

        if (body.currentState != stateName)
        {
            body.currentState = stateName;
            body.UpdateUI();
        }
    }

    void MakeTacticalDecision()
    {
        // normalize the health
        float max = Mathf.Max(1f, body.maxHealth);
        
        // get a value for the health in float
        float healthPct = body.currentHealth / max;

        /* define the probability weights, like if it is 0.80 (if health is 80%, say, from above calculation healthPct comes to 0.80),
        then here uAttack means there is a 0.80 chance to attack and uHeal means there is a 0.20 chance to retreat */
        float uAttack = healthPct;
        float uHeal = 1f - healthPct;

        // roll a random value from 0.0 to 1.0
        float roll = Random.value;
        
        // check what we get!
        Debug.Log($"[TACTICAL FSM] Health: {healthPct * 100}% | U(Attack): {uAttack:F2} | U(Heal) : {uHeal:F2} | Roll: {roll:F2}");
        
        /* If the random roll is lesser than the utility of the attack, state changes to burst attack state
         else, retreat */
        if (roll <= uAttack)
        {
            UpdateUI("BURST ATTACK!");
            currentState = AStarState.BurstAttack;
        }
        else
        {
            UpdateUI("RETREATING");
            
            CalculateRetreatPosition();
            
            // wind the timer again
            flankTimer = 4f;
            
            currentState = AStarState.RetreatHeal;
        }

    }

    void BurstAttack()
    {
        // stop movement
        seeker.speed = 0f;

        // find direction towards the player
        Vector3 lookDir = (player.position - transform.position).normalized;
        lookDir.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDir);

        // configure clock
        shootTimer -= Time.deltaTime;

        if (shootTimer <= 0f)
        {
            if (bulletPrefab != null && bulletSpawnPoint != null)
            {
                GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
                bullet.GetComponent<Rigidbody>().linearVelocity = bulletSpawnPoint.forward * 15f;
            }

            shootTimer = timeBetweenShots;
            
            // increment count
            burstCount++;
            
            if (burstCount >= 3)
            {
                burstCount = 0;

                // wind back the stopwatch after the bullets are over
                alertTimer = 1.5f;
                
                UpdateUI("ASSESSING");
                currentState = AStarState.Alert;
            }

        }
    }

    Transform FindNearestMedKit()
    {
        
        // store the Health kits prefabs as a gameobject array
        GameObject[] medkits = GameObject.FindGameObjectsWithTag("Health");

        Transform nearest = null;
        
        // assign infinity to the distance so actual distance can beat it
        float shortestDistance = Mathf.Infinity;
        
        
        foreach (GameObject medkit in medkits)
        {
            // continue if the object doesn't exist
            if (medkit == null)
            {
                continue;
            }

            // calculate the distance between the medkit and the npc
            float distance = Vector3.Distance(transform.position, medkit.transform.position);

            // take the distance and assign it to shortestDistance if it is lesser
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearest = medkit.transform;
            }
        }

        return nearest;
    }

    void CalculateRetreatPosition()
    {

        // find the nearest medical kit
        Transform nearestMedkit = FindNearestMedKit();

        if (nearestMedkit != null)
        {
            /* assign the node to the medkit position
            if it is in a wall, NodeFromWorldPoint will move it to an actual walkable node */
            Node safeNode = grid.NodeFromWorldPoint(nearestMedkit.position);
            dummyTarget.position = safeNode.worldPosition;
        }
        else
        {
            Vector3 awayFromPlayer = (transform.position - player.position).normalized;
            
            // calculate the retreat spot
            Vector3 retreatSpot = transform.position + (awayFromPlayer * 10f);

            Node safeNode = grid.NodeFromWorldPoint(retreatSpot);
            dummyTarget.position = safeNode.worldPosition;
        }

    }

    void RetreatHeal()
    {

        seeker.speed = 8f;

        // decrement the timer so it doesn't freeze anywhere
        flankTimer -= Time.deltaTime;

        Vector3 flatNPC = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatTarget = new Vector3(dummyTarget.position.x, 0, dummyTarget.position.z);

        if (Vector3.Distance(flatNPC, flatTarget) <= 2.0f || flankTimer <= 0f)
        {
            seeker.speed = 0f;

            Transform usedMedKit = FindNearestMedKit();

            if (usedMedKit != null && Vector3.Distance(transform.position, usedMedKit.position) <= 2.5f)
            {
                // increase health
                body.currentHealth = body.maxHealth;
                Destroy(usedMedKit.gameObject);
                UpdateUI("HEALED -> ASSESSING");
            }
            else
            {
                UpdateUI("No MEDKITS -> ASSESSING");
            }
            
            // reset the combat 
            alertTimer = 1.5f;
            
            currentState = AStarState.Alert;
        }

    }
}
