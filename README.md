# Game AI Combat Arena

A Unity combat arena developed for the **6CCGD007W Game AI** coursework at the University of Westminster.

The project demonstrates two distinct enemy NPCs, advanced finite state machines, two navigation methods, procedural artefact placement, and probabilistic combat behaviour.

## Features

- Custom enclosed arena with rooms, corridors, doorways, narrow passages, open spaces, and cover.
- Controllable player with health, movement, and projectile attacks.
- Two NPC types with different navigation and decision-making systems.
- Five-state finite state machine for each NPC type.
- Floating labels showing each NPC's ID, current state, and health.
- Six procedurally spawned artefact types with three instances each.
- NavMesh validation to prevent artefacts spawning in unreachable locations.
- Probabilistic player damage and weighted NavMesh terrain costs.

## NPC Type 1: NavMesh Guard

Uses Unity's `NavMeshAgent` for navigation.

**States:**

1. Patrol
2. Chase
3. Investigate
4. Attack
5. Seek Cover

The NPC patrols between waypoints, detects the player using raycasts, remembers the player's last known position, attacks within range, and seeks cover when its health is low.

## NPC Type 2: Tactical A* NPC

Uses a custom grid-based A* pathfinding system.

**States:**

1. Roam
2. Alert
3. Flank
4. Burst Attack
5. Retreat and Heal

The NPC selects reachable roaming locations, flanks the player, fires three-shot bursts, and uses health-weighted decisions to choose between attacking and retreating to a nearby medkit.

## Advanced Features

### Probabilistic Damage

NPC attacks use three weighted outcomes:

- Glancing Blow: 5 damage, 60% chance
- Solid Hit: 15 damage, 30% chance
- Critical Hit: 40 damage, 10% chance

### NavMesh Terrain Costs

The NavMesh contains three terrain categories:

- Open Ground: cost 1
- Narrow Corridor: cost 3
- Danger Zone: cost 10

These costs influence the routes selected by NavMesh NPCs.

## Procedural Artefacts

The arena generates 18 artefacts at runtime:

- Health
- Ammo
- Weapon
- Shield
- Trap
- Collectable

Spawn locations are checked for obstacle collisions, minimum spacing, and player accessibility using NavMesh path queries.

## Running the Project

### Requirements

- Unity Hub
- Unity Editor `6000.3.19f1`
- Unity AI Navigation package
- TextMeshPro

### Steps

1. Clone or download the repository.
2. Open the project through Unity Hub using Unity `6000.3.19f1`.
3. Open the main arena scene.
4. Press **Play**.

## Controls

| Action | Input |
|---|---|
| Move | WASD or Arrow Keys |
| Fire | Spacebar |

## Author

**James Motha**  
6CCGD007W Game AI  
University of Westminster

## Academic Notice

This repository contains university coursework. Do not copy or submit this work as your own.
