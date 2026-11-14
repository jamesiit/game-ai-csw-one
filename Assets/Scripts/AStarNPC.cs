using UnityEngine;

// the five states
public enum  AStarState
{
    Roam,
    Alert,
    Flank,
    BurstAttack,
    Retreat
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

    [Header("Tactical Settings")] 
    public float visionRange = 15f;

    private float alertDuration = 1.5f;
    private float alertTimer = 0f;

    private Seeker seeker;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        body = GetComponent<BaseNPC>();
        seeker = GetComponent<Seeker>();
        grid = FindFirstObjectByType<Grid>();
        
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
                dummyTarget.position = randomNode.worldPosition;
                foundPoint = true;
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

        // give the npc some thinking state where it looks at the player and rotates
        Vector3 lookDir = (player.position - transform.position).normalized;
        lookDir.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDir);

        // wind up the stopwatch
        alertTimer -= Time.deltaTime;

        // once the timer is up, moves the npc to the flank position
        if (alertTimer <= 0)
        {
            CalculateFlankPosition();
            currentState = AStarState.Flank;
        }
    }

    void Flank()
    {
        UpdateUI("FLANKING");
        
        // increase the speed from 3f to 6f
        seeker.speed = 6f;

        
        float distanceToFlank = Vector3.Distance(transform.position, dummyTarget.position);

        // when it gets in the range of the flank position, update UI
        if (distanceToFlank <= 1.5f)
        {
            UpdateUI("IN POSITION");
        }
    }
    
    void CalculateFlankPosition()
    {
        // calculate the flank position 5 units to the right of the player
        Vector3 rightFlank = player.position + (player.right * 5f);
        
        // validate the flank position on the grid so it is walkable
        Node flankNode = grid.NodeFromWorldPoint(rightFlank);

        // transport the dummyTarget to the valid flank position
        dummyTarget.position = flankNode.worldPosition;
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
}
