using UnityEngine;
using UnityEngine.AI;

public class Test : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Check for left mouse button click
        if (Input.GetMouseButtonDown(0))
        {
            // Create a ray from the camera to the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            // If the ray hits something...
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Set the agent's destination to the hit point
                agent.SetDestination(hit.point);
            }
        }
    }
}
