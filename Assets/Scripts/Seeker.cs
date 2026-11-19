using System.Collections.Generic;
using UnityEngine;

public class Seeker : MonoBehaviour
{
    [Header("A* Navigation")] public float speed = 3f;

    // lock the position
    private float startY;

    public Grid grid;
    public PathFinding pathFinder;

    private List<Node> myPath = new List<Node>();

    [Header("Links")] public Transform target;
    private BaseNPC body;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // initialize the grid and BaseNPC
        grid = FindFirstObjectByType<Grid>();
        pathFinder = FindFirstObjectByType<PathFinding>();
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
        if (pathFinder == null)
        {
            Debug.LogWarning($"[{gameObject.name}] ERROR: PathFinder is NULL!");
            return;
        }

        if (target == null)
        {
            Debug.LogWarning($"[{gameObject.name}] ERROR: Target is NULL!");
            return;
        }

        // if the npc has stopped or the distance between it and the target is same, skip pathfinding
        if (speed <= 0 || grid.NodeFromWorldPoint(transform.position) == grid.NodeFromWorldPoint(target.position))
        {
            return;
        }

        myPath = pathFinder.FindPath(transform.position, target.position);

        if (myPath == null || myPath.Count == 0)
        {
            Node startN = grid.NodeFromWorldPoint(transform.position);
            Node targetN = grid.NodeFromWorldPoint(target.position);
            
            Debug.LogWarning($"[{gameObject.name}] Empty path! State: {body.currentState} | StartWalkable: {startN.walkable} | TargetWalkable: {targetN.walkable}");
            return;
        }
        
        Node targetNode = myPath[0];

        Vector3 targetPosition = targetNode.worldPosition;
        targetPosition.y = startY;

        Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        newPosition.y = startY;
        transform.position = newPosition;

        Vector3 moveDirection = (targetNode.worldPosition - transform.position).normalized;

        if (moveDirection != Vector3.zero)
        {
            moveDirection.y = 0;
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }
}