using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ArtefactSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // the array of the six artefact prefabs
    public GameObject[] artefactPrefabs;
    
    public int itemsPerType = 3;

    // total game arena size is 30 x 30 units
    public float mapSizeX = 15f;
    public float mapSizeZ = 15f;
    public Transform playerTransform;

    // minimum distance between objects
    public float minSpacing = 5f;
    
    void Start()
    {
        SpawnArtefacts();
    }

    void SpawnArtefacts()
    {
        
        // remember every spawned position
        List<Vector3> spawnedPositions = new List<Vector3>();
        
        // iterate through the prefab objects
        foreach (GameObject prefab in artefactPrefabs)
        {
            int spawnedCount = 0;
            int attempts = 0;
            
            while (spawnedCount < itemsPerType)
            {
                attempts++;
                if (attempts > 500)
                {
                    Debug.LogError("Could not find a valid path!");
                    break;
                }
                // random spots is picked inside the arena
                float randomX = Random.Range(-mapSizeX, mapSizeX);
                float randomZ = Random.Range(-mapSizeZ, mapSizeZ);

                // create a new random position from the above random x and z values
                Vector3 testPosition = transform.position + new Vector3(randomX, 0.4f, randomZ);

                // check if the random position landed inside a solid wall, and we raise the Y coordinate to 1.5f so the floor is ignored
                Vector3 radarHeight = new Vector3(testPosition.x, 1.5f, testPosition.z);
                bool isInsideWall = Physics.CheckSphere(radarHeight, 0.5f);

                // check if it's close to another item
                bool isTooCloseToAnotherItem = false;

                /* check the spawnedPositions and for each position, check if the distance between 
                the spawned position and the ones that exist in the List are within the boundaries of the minimum spacing! */
                foreach (Vector3 existingPosition in spawnedPositions)
                {
                    if (Vector3.Distance(testPosition, existingPosition) < minSpacing)
                    {
                        isTooCloseToAnotherItem = true;
                        break;
                    }
                }

                /* we create a navmesh where the path is evaluated from the players position to the random position.
                 This way, we would know if the artefact that spawned, actually is in the walkable area! */
                NavMeshPath path = new NavMeshPath();
                bool isReachable = false;

                NavMeshHit itemHit;
                NavMeshHit playerHit;

                /* we use SamplePosition here to lock the player's and item's positions into the actual walkable NavMeshArea 
                 so the ai won't panic when it has to do the path calculations */
                bool itemIsOnGrid = NavMesh.SamplePosition(testPosition, out itemHit, 2.0f, NavMesh.AllAreas);
                bool playerIsOnGrid = NavMesh.SamplePosition(playerTransform.position, out playerHit, 2.0f, NavMesh.AllAreas);

                if (itemIsOnGrid && playerIsOnGrid)
                {
                    
                    // draws a path from the player to the item
                    NavMesh.CalculatePath(playerHit.position, itemHit.position, NavMesh.AllAreas, path);

                    // after the path has been calculated, the path variable is now updated with the information the AI found
                    if (path.status == NavMeshPathStatus.PathComplete)
                    {
                        isReachable = true;
                    }
                }

                // simple debugging in case the grid snap didn't work out 
                else
                {
                    Debug.LogError("Grid Snap Failed! Item on grid: " + itemIsOnGrid + " | Player on grid: " + playerIsOnGrid);
                }
                
                /* if the item to be spawned is not inside a wall and if it is not too close to another item and if it is
                reachable by the navmesh path, let it through and update the List */
                if (!isInsideWall && !isTooCloseToAnotherItem && isReachable)
                {
                    Instantiate(prefab, testPosition, Quaternion.identity);
                    spawnedPositions.Add(testPosition);
                    spawnedCount++;

                    // we use lines to connect every spawned object to the player
                    GameObject lineObject = new GameObject("PathLine_" + prefab.name);
                    LineRenderer line = lineObject.AddComponent<LineRenderer>();

                    line.startWidth = 0.1f;
                    line.endWidth = 0.1f;
                    
                    // path.corners is the "turns" that the AI saves when it has to calculate the path and avoid the walls and obstacles and such
                    line.positionCount = path.corners.Length;
                    
                    // draws the paths
                    line.SetPositions(path.corners);
                }
            }
        }
    }
}