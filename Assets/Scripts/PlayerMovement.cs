using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public float moveSpeed = 5f;
    public Rigidbody rb;
    
    void Start()
    {
        // initialize the physics engine
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get WASD / Arrow Key input
        float moveX = InputSystem.actions.FindAction("Move").ReadValue<Vector2>().x;
        
        float moveZ = InputSystem.actions.FindAction("Move").ReadValue<Vector2>().y;
        
        // allows the physics engine to read movements
        Vector3 move = new Vector3(moveX, 0f, moveZ) * moveSpeed;
        
        // assign the variables to the physics engine
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }
}
