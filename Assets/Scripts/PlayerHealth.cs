using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    // we need System.Serializable here so unity includes the DamageOutcome fields in the inspector
    [System.Serializable]
    public class DamageOutcome
    {
        public string hitCategory;
        public int damageAmount;
        public float weight;
    }

    public class PlayerHealth : MonoBehaviour
    {
        public int currentHealth = 100;

        // create an array of damage types
        public DamageOutcome[] damageTable = new DamageOutcome[]
        {
            new DamageOutcome { hitCategory = "Glancing Blow", damageAmount = 5, weight = 60f },
            new DamageOutcome { hitCategory = "Solid Hit", damageAmount = 15, weight = 30f },
            new DamageOutcome { hitCategory = "Critical Headshot", damageAmount = 40, weight = 10f }
        };

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Bullet"))
            {
                TakeProbabalisticDamage();
                Destroy(collision.gameObject);
            }
        }

        public void TakeProbabalisticDamage()
        {
            
            /* calculate the totalWeight so that the probabilities are balanced (if you were to change them)
            later even, it would hold up */
            float totalWeight = 0f;

            foreach (DamageOutcome outcome in damageTable)
            {
                totalWeight += outcome.weight;
            }

            // pick a random value
            float randomRoll = Random.Range(0f, totalWeight);

            float runningTotal = 0f;

            /* once the runningTotal is same value as randomRoll, the damage amount is that
            value so that amount is dealt */
            for (int i = 0; i < damageTable.Length; i++)
            {
                runningTotal += damageTable[i].weight;

                if (randomRoll < runningTotal)
                {
                    
                    int finalDamage = damageTable[i].damageAmount;
                    currentHealth -= finalDamage;
                    
                    float chancePct = (damageTable[i].weight / totalWeight) * 100f;

                    Debug.Log($"[PROBABILITY HIT] {damageTable[i].hitCategory} ({chancePct}%) | Rolled: {randomRoll:F1}/{totalWeight} | HP: {currentHealth}");

                    // stop the loop once the condition is properly met 
                    return;
                }
            }
        }
    }
}