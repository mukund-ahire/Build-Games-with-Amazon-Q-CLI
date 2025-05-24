using System.Collections.Generic;
using UnityEngine;

namespace Resilience.Inventory
{
    /// <summary>
    /// Manages the player's inventory system
    /// </summary>
    public class InventorySystem : MonoBehaviour
    {
        [SerializeField] private int maxInventorySlots = 20;
        
        private List<InventoryItem> inventoryItems = new List<InventoryItem>();
        
        // Events
        public delegate void InventoryChangedHandler();
        public event InventoryChangedHandler OnInventoryChanged;
        
        /// <summary>
        /// Attempts to add an item to the inventory
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <returns>True if the item was added successfully, false if inventory is full</returns>
        public bool AddItem(InventoryItem item)
        {
            // Check if inventory is full
            if (inventoryItems.Count >= maxInventorySlots)
            {
                Debug.Log("Inventory is full!");
                return false;
            }
            
            // Check if item is stackable and already exists in inventory
            if (item.IsStackable)
            {
                foreach (InventoryItem existingItem in inventoryItems)
                {
                    if (existingItem.ItemID == item.ItemID)
                    {
                        existingItem.Quantity += item.Quantity;
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }
            
            // Add new item to inventory
            inventoryItems.Add(item);
            OnInventoryChanged?.Invoke();
            return true;
        }
        
        /// <summary>
        /// Removes an item from the inventory
        /// </summary>
        /// <param name="itemIndex">Index of the item to remove</param>
        /// <param name="quantity">Quantity to remove</param>
        /// <returns>True if the item was removed successfully</returns>
        public bool RemoveItem(int itemIndex, int quantity = 1)
        {
            if (itemIndex < 0 || itemIndex >= inventoryItems.Count)
            {
                Debug.LogError("Invalid inventory index!");
                return false;
            }
            
            InventoryItem item = inventoryItems[itemIndex];
            
            if (item.Quantity <= quantity)
            {
                // Remove the entire item
                inventoryItems.RemoveAt(itemIndex);
            }
            else
            {
                // Reduce the quantity
                item.Quantity -= quantity;
            }
            
            OnInventoryChanged?.Invoke();
            return true;
        }
        
        /// <summary>
        /// Gets all items in the inventory
        /// </summary>
        /// <returns>List of inventory items</returns>
        public List<InventoryItem> GetAllItems()
        {
            return inventoryItems;
        }
        
        /// <summary>
        /// Checks if the inventory contains a specific item
        /// </summary>
        /// <param name="itemID">ID of the item to check</param>
        /// <param name="quantity">Minimum quantity required</param>
        /// <returns>True if the inventory contains the specified quantity of the item</returns>
        public bool HasItem(string itemID, int quantity = 1)
        {
            foreach (InventoryItem item in inventoryItems)
            {
                if (item.ItemID == itemID && item.Quantity >= quantity)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Gets the current number of items in the inventory
        /// </summary>
        /// <returns>Current inventory count</returns>
        public int GetCurrentInventoryCount()
        {
            return inventoryItems.Count;
        }
        
        /// <summary>
        /// Gets the maximum number of inventory slots
        /// </summary>
        /// <returns>Maximum inventory slots</returns>
        public int GetMaxInventorySlots()
        {
            return maxInventorySlots;
        }
    }
    
    /// <summary>
    /// Represents an item in the inventory
    /// </summary>
    [System.Serializable]
    public class InventoryItem
    {
        public string ItemID;
        public string ItemName;
        public string Description;
        public Sprite Icon;
        public int Quantity = 1;
        public float Weight = 1f;
        public bool IsStackable = true;
        public ItemType Type;
        
        public enum ItemType
        {
            Resource,
            Tool,
            Weapon,
            Food,
            Water,
            Medical,
            Clothing,
            Building
        }
    }
}
