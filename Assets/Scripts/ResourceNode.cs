using UnityEngine;
using Resilience.Inventory;

namespace Resilience.Resources
{
    /// <summary>
    /// Represents a harvestable resource in the game world
    /// </summary>
    public class ResourceNode : MonoBehaviour, IInteractable
    {
        [Header("Resource Settings")]
        [SerializeField] private string resourceName = "Resource";
        [SerializeField] private string resourceID = "resource_default";
        [SerializeField] private int resourceAmount = 3;
        [SerializeField] private int maxResourceAmount = 3;
        [SerializeField] private float respawnTime = 300f; // Time in seconds for resource to respawn
        [SerializeField] private GameObject visualObject;
        [SerializeField] private GameObject depletedVisual;
        
        [Header("Harvesting Settings")]
        [SerializeField] private string requiredToolType = ""; // Leave empty if no tool required
        [SerializeField] private float harvestTime = 2f; // Time in seconds to harvest
        [SerializeField] private AudioClip harvestSound;
        [SerializeField] private ParticleSystem harvestEffect;
        
        private bool isDepleted = false;
        private float respawnTimer = 0f;
        
        private void Update()
        {
            // Handle respawning if depleted
            if (isDepleted)
            {
                respawnTimer += Time.deltaTime;
                if (respawnTimer >= respawnTime)
                {
                    Respawn();
                }
            }
        }
        
        /// <summary>
        /// Called when player interacts with this resource
        /// </summary>
        public void Interact(GameObject player)
        {
            if (isDepleted)
                return;
                
            // Check if player has required tool
            if (!string.IsNullOrEmpty(requiredToolType))
            {
                InventorySystem inventory = player.GetComponent<InventorySystem>();
                if (inventory == null || !HasRequiredTool(inventory))
                {
                    Debug.Log($"Need a {requiredToolType} to harvest this resource");
                    return;
                }
            }
            
            // Start harvesting coroutine
            StartCoroutine(HarvestCoroutine(player));
        }
        
        /// <summary>
        /// Coroutine for harvesting the resource
        /// </summary>
        private System.Collections.IEnumerator HarvestCoroutine(GameObject player)
        {
            // Play harvest effect and sound
            if (harvestEffect != null)
                harvestEffect.Play();
                
            if (harvestSound != null)
            {
                AudioSource.PlayClipAtPoint(harvestSound, transform.position);
            }
            
            // Wait for harvest time
            yield return new WaitForSeconds(harvestTime);
            
            // Give resource to player
            InventorySystem inventory = player.GetComponent<InventorySystem>();
            if (inventory != null)
            {
                InventoryItem item = new InventoryItem
                {
                    ItemID = resourceID,
                    ItemName = resourceName,
                    Quantity = 1,
                    Type = InventoryItem.ItemType.Resource
                };
                
                inventory.AddItem(item);
                Debug.Log($"Harvested {resourceName}");
            }
            
            // Reduce resource amount
            resourceAmount--;
            
            // Check if depleted
            if (resourceAmount <= 0)
            {
                Deplete();
            }
        }
        
        /// <summary>
        /// Depletes the resource
        /// </summary>
        private void Deplete()
        {
            isDepleted = true;
            respawnTimer = 0f;
            
            // Change visual representation
            if (visualObject != null)
                visualObject.SetActive(false);
                
            if (depletedVisual != null)
                depletedVisual.SetActive(true);
                
            Debug.Log($"{resourceName} depleted");
        }
        
        /// <summary>
        /// Respawns the resource
        /// </summary>
        private void Respawn()
        {
            isDepleted = false;
            resourceAmount = maxResourceAmount;
            
            // Change visual representation back
            if (visualObject != null)
                visualObject.SetActive(true);
                
            if (depletedVisual != null)
                depletedVisual.SetActive(false);
                
            Debug.Log($"{resourceName} respawned");
        }
        
        /// <summary>
        /// Checks if the player has the required tool
        /// </summary>
        private bool HasRequiredTool(InventorySystem inventory)
        {
            foreach (InventoryItem item in inventory.GetAllItems())
            {
                if (item.Type == InventoryItem.ItemType.Tool && item.ItemID.Contains(requiredToolType))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Gets the interaction prompt text
        /// </summary>
        public string GetInteractionPrompt()
        {
            if (isDepleted)
                return $"{resourceName} (Depleted)";
                
            return $"Harvest {resourceName}";
        }
    }
    
    /// <summary>
    /// Interface for objects that can be interacted with
    /// </summary>
    public interface IInteractable
    {
        void Interact(GameObject player);
        string GetInteractionPrompt();
    }
}
