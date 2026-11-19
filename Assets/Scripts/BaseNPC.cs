using System;
using TMPro;
using UnityEngine;

public class BaseNPC : MonoBehaviour
{
    // group the variables under NPC Data in the inspector
    [Header("NPC Data")] 
    public string npcId = "NPC_1";
    public string currentState = "PATROL";
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI")] 
    public TextMeshPro statusText;
    
    // Start is called and we instantly display the currentHealth value
    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    private void OnCollisionEnter(Collision collision)
    {
        /* when it collides, we check if it was a bullet, and if so, 
        we let it take some damage and we destroy the bullet */
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            
            // only take damage if the player fired the bullet
            if (bullet != null && bullet.isPlayerBullet)
            {
                TakeDamage(25);
            }

            Destroy(collision.gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"Damage taken! HP was {currentHealth}, minus {damage}...");
        currentHealth -= damage;

        // safety check to make sure that the HP doesn't drop below zero
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        UpdateUI();

        if (currentHealth <= 0)
        {
            Debug.Log(npcId + " has died!");
            
            // destroy the npc for now
            Destroy(gameObject);
        }
    }

    public void UpdateUI()
    {
        if (statusText != null)
        {
            statusText.text = $"ID: {npcId}\nSTATE: {currentState}\nHP: {currentHealth}";
        }
    }
}
