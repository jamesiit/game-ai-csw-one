using UnityEngine;

public class Seeker : MonoBehaviour
{

    [Header("A* Navigation")] 
    public float speed = 3f;
    
    // lock the position
    private float startY;

    public Grid grid;
    private int targetIndex;

    [Header("Links")] 
    public Transform target;
    private BaseNPC body;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // initialize the grid and BaseNPC
        grid = FindFirstObjectByType<Grid>();
        body = GetComponent<BaseNPC>();
        startY = transform.position.y;

        // upon successful loading of the BaseNPC script, update currentState
        if (body != null)
        {
            body.currentState = "CHASING (A*)";
            body.UpdateUI();
        }
        else
        {
            Debug.LogError("BaseNPC couldn't be loaded!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // checks if the grid exists, if the path has been drawn properly and if there is at least one square to step on for pathfinding
        if (grid != null && grid.path != null && grid.path.Count > 0)
        {
            // the first square in the path is saved as the targetNode
            Node targetNode = grid.path[0];

            // match the node's Y position to the NPC's so it doesnt get dragged down into the floor
            Vector3 targetPosition = targetNode.worldPosition;
            
            targetPosition.y = startY;

            // move the npc towards the targetNode in a speed which is constant 
            Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            newPosition.y = startY;
            transform.position = newPosition;
            
            // find the direction from npc to the targetNode
            Vector3 moveDirection = (targetNode.worldPosition - transform.position).normalized;

            // body turns only when its actually moving
            if (moveDirection != Vector3.zero)
            {
                // prevent npc to look up at the waypoint
                moveDirection.y = 0;
                
                // rotate the npc to face the direction
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }

        }
    }
}
