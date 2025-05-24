# Resilience Game Demo Instructions

## Setup Instructions

### Prerequisites
- Unity 2022.1 or newer
- Basic understanding of Unity Editor

### Setting Up the Demo

1. **Create a new Unity project**
   - Open Unity Hub
   - Click "New Project"
   - Select "3D" template
   - Name it "Resilience"
   - Choose a location and click "Create"

2. **Import the project files**
   - Copy all files from the repository into your Unity project folder
   - In Unity, wait for all scripts to compile

3. **Create the basic scene**
   - Create a new scene (File > New Scene)
   - Save it as "GameScene" in the Assets/Scenes folder

4. **Set up the environment**
   - Create a terrain (GameObject > 3D Object > Terrain)
   - Add basic textures and features to the terrain
   - Add trees, rocks, and other environmental objects
   - Create a directional light for the sun

5. **Create the player**
   - Create an empty GameObject named "Player"
   - Add a Character Controller component
   - Add a capsule for the player model
   - Create a camera as a child of the player
   - Position the camera at the top of the capsule

6. **Add required components to the player**
   - Add the PlayerMovement script
   - Add the PlayerStats script
   - Add the PlayerInteraction script
   - Add the InventorySystem script

7. **Create UI elements**
   - Create a Canvas (GameObject > UI > Canvas)
   - Add UI elements for health, hunger, thirst, and stamina bars
   - Add an interaction prompt text
   - Add inventory and crafting UI panels (initially disabled)

8. **Create resource nodes**
   - Create prefabs for different resource types (trees, rocks, plants)
   - Add the ResourceNode script to each prefab
   - Place instances of these prefabs in the scene

9. **Set up the world manager**
   - Create an empty GameObject named "WorldManager"
   - Add the WorldManager script
   - Configure the day/night cycle and weather settings

10. **Set up the game manager**
    - Create an empty GameObject named "GameManager"
    - Add the GameManager script
    - Create basic UI for pause menu and game over screen

## Running the Demo

1. **Play the game in Unity Editor**
   - Click the Play button in Unity
   - The game should start with the player in the world

2. **Basic controls**
   - WASD: Movement
   - Mouse: Look around
   - Space: Jump
   - Left Shift: Sprint
   - C: Crouch
   - E: Interact with objects
   - I: Toggle inventory
   - B: Toggle building mode
   - Escape: Pause menu

3. **Gameplay features to demonstrate**
   - Movement and camera controls
   - Survival mechanics (watch health, hunger, thirst, stamina bars)
   - Resource gathering (approach trees/rocks and press E)
   - Inventory management (press I to open inventory)
   - Crafting (create basic tools and items)
   - Building (place simple structures)
   - Day/night cycle (observe changing lighting)

## Troubleshooting

- **Missing references**: Ensure all script references are properly assigned in the Inspector
- **Null reference exceptions**: Check the console for errors and verify all required components exist
- **Movement issues**: Verify the Character Controller component is properly configured
- **UI not showing**: Make sure the Canvas is set to "Screen Space - Overlay" and has an Event System

## Extending the Demo

- Add more resource types and crafting recipes
- Implement a simple AI for animals or threats
- Add more building pieces and structures
- Implement a basic quest or objective system
- Add sound effects and music

## Presentation Tips

1. Start by explaining the concept of "Resilience" as a survival game
2. Demonstrate the core survival mechanics
3. Show resource gathering and crafting
4. Demonstrate the building system
5. Highlight the day/night cycle and weather effects
6. Discuss future development plans
