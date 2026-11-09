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
        // the bullet destroys itself only when it hits something which is not a player and an NPC
        if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.GetComponent<BaseNPC>())
        {
            Destroy(gameObject);
        }
    }
}
