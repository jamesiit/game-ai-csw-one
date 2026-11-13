using UnityEngine;

public class Seeker : MonoBehaviour
{

    [Header("A* Navigation")] 
    public float speed = 3f;

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

            // move the npc towards the targetNode in a speed which is constant 
            transform.position = Vector3.MoveTowards(transform.position, targetNode.worldPosition, speed * Time.deltaTime);

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
