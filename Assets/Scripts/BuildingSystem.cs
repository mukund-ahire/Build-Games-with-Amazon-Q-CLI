using System.Collections.Generic;
using UnityEngine;
using Resilience.Inventory;

namespace Resilience.Building
{
    /// <summary>
    /// Manages the building system for constructing structures
    /// </summary>
    public class BuildingSystem : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float buildDistance = 5f;
        [SerializeField] private LayerMask buildSurfaceMask;
        [SerializeField] private LayerMask buildCollisionMask;
        [SerializeField] private Material validPlacementMaterial;
        [SerializeField] private Material invalidPlacementMaterial;
        [SerializeField] private List<BuildingPiece> availableBuildingPieces = new List<BuildingPiece>();
        
        private InventorySystem inventorySystem;
        private BuildingPiece currentBuildingPiece;
        private GameObject buildPreview;
        private bool isBuildMode = false;
        private int currentPieceIndex = 0;
        
        private void Start()
        {
            if (playerCamera == null)
                playerCamera = Camera.main;
                
            inventorySystem = GetComponent<InventorySystem>();
            if (inventorySystem == null)
                inventorySystem = FindObjectOfType<InventorySystem>();
        }
        
        private void Update()
        {
            // Toggle build mode with B key
            if (Input.GetKeyDown(KeyCode.B))
            {
                ToggleBuildMode();
            }
            
            if (!isBuildMode)
                return;
                
            // Cycle through building pieces with scroll wheel
            float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
            if (scrollDelta > 0f)
            {
                CycleBuildingPiece(1);
            }
            else if (scrollDelta < 0f)
            {
                CycleBuildingPiece(-1);
            }
            
            // Update preview position
            UpdateBuildPreview();
            
            // Place building piece on left click
            if (Input.GetMouseButtonDown(0))
            {
                PlaceBuildingPiece();
            }
            
            // Cancel build mode on right click
            if (Input.GetMouseButtonDown(1))
            {
                ToggleBuildMode();
            }
        }
        
        /// <summary>
        /// Toggles building mode on/off
        /// </summary>
        public void ToggleBuildMode()
        {
            isBuildMode = !isBuildMode;
            
            if (isBuildMode)
            {
                // Enter build mode
                if (availableBuildingPieces.Count > 0)
                {
                    currentPieceIndex = 0;
                    CreateBuildPreview();
                }
                else
                {
                    Debug.LogWarning("No building pieces available!");
                    isBuildMode = false;
                }
            }
            else
            {
                // Exit build mode
                DestroyBuildPreview();
            }
            
            Debug.Log($"Build mode: {(isBuildMode ? "Enabled" : "Disabled")}");
        }
        
        /// <summary>
        /// Cycles to the next/previous building piece
        /// </summary>
        private void CycleBuildingPiece(int direction)
        {
            if (availableBuildingPieces.Count == 0)
                return;
                
            currentPieceIndex += direction;
            
            // Wrap around
            if (currentPieceIndex >= availableBuildingPieces.Count)
                currentPieceIndex = 0;
            else if (currentPieceIndex < 0)
                currentPieceIndex = availableBuildingPieces.Count - 1;
                
            // Update preview
            DestroyBuildPreview();
            CreateBuildPreview();
        }
        
        /// <summary>
        /// Creates a preview of the current building piece
        /// </summary>
        private void CreateBuildPreview()
        {
            if (currentPieceIndex < 0 || currentPieceIndex >= availableBuildingPieces.Count)
                return;
                
            currentBuildingPiece = availableBuildingPieces[currentPieceIndex];
            
            if (currentBuildingPiece.Prefab != null)
            {
                buildPreview = Instantiate(currentBuildingPiece.Prefab);
                
                // Make preview semi-transparent
                Renderer[] renderers = buildPreview.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    Material[] materials = new Material[renderer.materials.Length];
                    for (int i = 0; i < materials.Length; i++)
                    {
                        materials[i] = validPlacementMaterial;
                    }
                    renderer.materials = materials;
                }
                
                // Disable colliders
                Collider[] colliders = buildPreview.GetComponentsInChildren<Collider>();
                foreach (Collider collider in colliders)
                {
                    collider.enabled = false;
                }
            }
        }
        
        /// <summary>
        /// Destroys the current building preview
        /// </summary>
        private void DestroyBuildPreview()
        {
            if (buildPreview != null)
            {
                Destroy(buildPreview);
                buildPreview = null;
            }
        }
        
        /// <summary>
        /// Updates the position and rotation of the building preview
        /// </summary>
        private void UpdateBuildPreview()
        {
            if (buildPreview == null || playerCamera == null)
                return;
                
            RaycastHit hit;
            bool validPlacement = false;
            
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, buildDistance, buildSurfaceMask))
            {
                // Position preview at hit point
                buildPreview.transform.position = hit.point;
                
                // Rotate preview based on surface normal
                buildPreview.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                
                // Rotate preview around normal based on mouse movement
                if (Input.GetKey(KeyCode.R))
                {
                    buildPreview.transform.Rotate(Vector3.up, 15f * Time.deltaTime, Space.Self);
                }
                
                // Check for collisions
                validPlacement = !CheckForCollisions();
            }
            
            // Update preview material based on placement validity
            Renderer[] renderers = buildPreview.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Material material = validPlacement ? validPlacementMaterial : invalidPlacementMaterial;
                Material[] materials = new Material[renderer.materials.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = material;
                }
                renderer.materials = materials;
            }
        }
        
        /// <summary>
        /// Checks if the current building preview collides with anything
        /// </summary>
        private bool CheckForCollisions()
        {
            if (buildPreview == null)
                return false;
                
            // Get bounds of preview
            Bounds bounds = new Bounds(buildPreview.transform.position, Vector3.zero);
            Renderer[] renderers = buildPreview.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            
            // Check for collisions
            return Physics.CheckBox(bounds.center, bounds.extents, buildPreview.transform.rotation, buildCollisionMask);
        }
        
        /// <summary>
        /// Places the current building piece in the world
        /// </summary>
        private void PlaceBuildingPiece()
        {
            if (buildPreview == null || currentBuildingPiece == null)
                return;
                
            // Check if placement is valid
            if (CheckForCollisions())
            {
                Debug.Log("Cannot place building piece: collision detected");
                return;
            }
            
            // Check if player has required resources
            if (inventorySystem != null)
            {
                foreach (BuildingResource resource in currentBuildingPiece.RequiredResources)
                {
                    if (!inventorySystem.HasItem(resource.ItemID, resource.Amount))
                    {
                        Debug.Log($"Missing resource: {resource.ItemID} x{resource.Amount}");
                        return;
                    }
                }
                
                // Consume resources
                foreach (BuildingResource resource in currentBuildingPiece.RequiredResources)
                {
                    List<InventoryItem> items = inventorySystem.GetAllItems();
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i].ItemID == resource.ItemID)
                        {
                            inventorySystem.RemoveItem(i, resource.Amount);
                            break;
                        }
                    }
                }
            }
            
            // Instantiate actual building piece
            GameObject placedPiece = Instantiate(currentBuildingPiece.Prefab, buildPreview.transform.position, buildPreview.transform.rotation);
            
            // Enable colliders
            Collider[] colliders = placedPiece.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = true;
            }
            
            Debug.Log($"Placed building piece: {currentBuildingPiece.PieceName}");
            
            // Create new preview
            DestroyBuildPreview();
            CreateBuildPreview();
        }
    }
    
    /// <summary>
    /// Represents a building piece that can be placed in the world
    /// </summary>
    [System.Serializable]
    public class BuildingPiece
    {
        public string PieceName;
        public GameObject Prefab;
        public List<BuildingResource> RequiredResources = new List<BuildingResource>();
    }
    
    /// <summary>
    /// Represents a resource required for building
    /// </summary>
    [System.Serializable]
    public class BuildingResource
    {
        public string ItemID;
        public int Amount = 1;
    }
}
