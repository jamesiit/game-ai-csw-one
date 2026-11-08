using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    public float life = 3;
    private void Awake()
    {
        Destroy(gameObject, life);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // bullet gets destroyed when it hits a wall or an obstacle. Leaving this here for now
        Destroy(gameObject);
    }
}
