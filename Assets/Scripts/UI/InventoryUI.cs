using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Resilience.Inventory;

namespace Resilience.UI
{
    /// <summary>
    /// Manages the UI for the inventory system
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private InventorySystem inventorySystem;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private GameObject itemSlotPrefab;
        
        private List<GameObject> itemSlots = new List<GameObject>();
        private bool isInventoryOpen = false;
        
        private void Start()
        {
            if (inventorySystem == null)
                inventorySystem = FindObjectOfType<InventorySystem>();
                
            if (inventorySystem != null)
                inventorySystem.OnInventoryChanged += UpdateInventoryUI;
                
            if (inventoryPanel != null)
                inventoryPanel.SetActive(false);
        }
        
        private void Update()
        {
            // Toggle inventory with I key
            if (Input.GetKeyDown(KeyCode.I))
            {
                ToggleInventory();
            }
        }
        
        /// <summary>
        /// Toggles the inventory panel visibility
        /// </summary>
        public void ToggleInventory()
        {
            isInventoryOpen = !isInventoryOpen;
            
            if (inventoryPanel != null)
                inventoryPanel.SetActive(isInventoryOpen);
                
            if (isInventoryOpen)
                UpdateInventoryUI();
                
            // Toggle cursor visibility and lock state
            Cursor.visible = isInventoryOpen;
            Cursor.lockState = isInventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;
        }
        
        /// <summary>
        /// Updates the inventory UI to reflect current inventory contents
        /// </summary>
        public void UpdateInventoryUI()
        {
            if (inventorySystem == null || itemsContainer == null || itemSlotPrefab == null)
                return;
                
            // Clear existing slots
            foreach (GameObject slot in itemSlots)
            {
                Destroy(slot);
            }
            itemSlots.Clear();
            
            // Get all items from inventory
            List<InventoryItem> items = inventorySystem.GetAllItems();
            
            // Create slots for each item
            foreach (InventoryItem item in items)
            {
                GameObject slotObject = Instantiate(itemSlotPrefab, itemsContainer);
                itemSlots.Add(slotObject);
                
                // Set item name
                Text itemNameText = slotObject.transform.Find("ItemName")?.GetComponent<Text>();
                if (itemNameText != null)
                    itemNameText.text = item.ItemName;
                    
                // Set item quantity
                Text itemQuantityText = slotObject.transform.Find("ItemQuantity")?.GetComponent<Text>();
                if (itemQuantityText != null)
                    itemQuantityText.text = item.Quantity.ToString();
                    
                // Set item icon
                Image itemIcon = slotObject.transform.Find("ItemIcon")?.GetComponent<Image>();
                if (itemIcon != null && item.Icon != null)
                    itemIcon.sprite = item.Icon;
                    
                // Add button functionality
                Button useButton = slotObject.transform.Find("UseButton")?.GetComponent<Button>();
                if (useButton != null)
                {
                    int itemIndex = items.IndexOf(item);
                    useButton.onClick.AddListener(() => UseItem(itemIndex));
                }
                
                Button dropButton = slotObject.transform.Find("DropButton")?.GetComponent<Button>();
                if (dropButton != null)
                {
                    int itemIndex = items.IndexOf(item);
                    dropButton.onClick.AddListener(() => DropItem(itemIndex));
                }
            }
        }
        
        /// <summary>
        /// Uses the item at the specified index
        /// </summary>
        private void UseItem(int itemIndex)
        {
            // Implementation depends on item type
            Debug.Log($"Using item at index {itemIndex}");
            
            // Example implementation for consumable items
            List<InventoryItem> items = inventorySystem.GetAllItems();
            if (itemIndex >= 0 && itemIndex < items.Count)
            {
                InventoryItem item = items[itemIndex];
                
                if (item.Type == InventoryItem.ItemType.Food)
                {
                    // Find player and call ConsumeFood
                    PlayerController player = FindObjectOfType<PlayerController>();
                    if (player != null)
                    {
                        player.ConsumeFood(20f); // Example value
                        inventorySystem.RemoveItem(itemIndex, 1);
                    }
                }
                else if (item.Type == InventoryItem.ItemType.Water)
                {
                    // Find player and call ConsumeWater
                    PlayerController player = FindObjectOfType<PlayerController>();
                    if (player != null)
                    {
                        player.ConsumeWater(20f); // Example value
                        inventorySystem.RemoveItem(itemIndex, 1);
                    }
                }
                else if (item.Type == InventoryItem.ItemType.Medical)
                {
                    // Find player and call Heal
                    PlayerController player = FindObjectOfType<PlayerController>();
                    if (player != null)
                    {
                        player.Heal(25f); // Example value
                        inventorySystem.RemoveItem(itemIndex, 1);
                    }
                }
            }
        }
        
        /// <summary>
        /// Drops the item at the specified index
        /// </summary>
        private void DropItem(int itemIndex)
        {
            Debug.Log($"Dropping item at index {itemIndex}");
            inventorySystem.RemoveItem(itemIndex, 1);
        }
    }
}
