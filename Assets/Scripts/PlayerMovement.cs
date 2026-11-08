using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public float moveSpeed = 5f;
    public Rigidbody rb;
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10;

    public int maxHealth = 100;
    public int currentHealth;

    void Start()
    {
        // initialize the physics engine
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        // Get WASD / Arrow Key input
        float moveX = InputSystem.actions.FindAction("Move").ReadValue<Vector2>().x;

        float moveZ = InputSystem.actions.FindAction("Move").ReadValue<Vector2>().y;

        /* the movement direction of the player. Since you can travel faster when moving diagonally, we use normalized here so
        speed is even in the 8 directions */
        Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

        // allows the physics engine to read movements
        Vector3 move = moveDirection * moveSpeed;

        // assign the variables to the physics engine
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);

        // avoid player from snapping to north direction if there is no input
        if (moveDirection != Vector3.zero)
        {
            // calculate by how many units the rotation should work so the player is facing its direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            
            // instruct physics engine to apply rotation logic
            rb.MoveRotation(targetRotation);
        }

        // shooting logic

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // clone the bullet
            var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

            // physics engine will fire the bullet at the facing direction by a certain speed
            bullet.GetComponent<Rigidbody>().linearVelocity = bulletSpawnPoint.forward * bulletSpeed;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log("Player took damage of: " + damageAmount + " Current HP is: " + currentHealth);
    }
}