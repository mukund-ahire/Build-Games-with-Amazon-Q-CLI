using System.Collections.Generic;
using UnityEngine;
using Resilience.Inventory;

namespace Resilience.Crafting
{
    /// <summary>
    /// Manages the crafting system for creating items from resources
    /// </summary>
    public class CraftingSystem : MonoBehaviour
    {
        [SerializeField] private List<CraftingRecipe> availableRecipes = new List<CraftingRecipe>();
        
        private InventorySystem inventorySystem;
        
        private void Awake()
        {
            inventorySystem = GetComponent<InventorySystem>();
            if (inventorySystem == null)
            {
                inventorySystem = FindObjectOfType<InventorySystem>();
                if (inventorySystem == null)
                {
                    Debug.LogError("No InventorySystem found! Crafting will not work.");
                }
            }
        }
        
        /// <summary>
        /// Attempts to craft an item using the specified recipe
        /// </summary>
        /// <param name="recipeIndex">Index of the recipe to craft</param>
        /// <returns>True if crafting was successful</returns>
        public bool CraftItem(int recipeIndex)
        {
            if (recipeIndex < 0 || recipeIndex >= availableRecipes.Count)
            {
                Debug.LogError("Invalid recipe index!");
                return false;
            }
            
            CraftingRecipe recipe = availableRecipes[recipeIndex];
            
            // Check if player has all required ingredients
            foreach (RecipeIngredient ingredient in recipe.Ingredients)
            {
                if (!inventorySystem.HasItem(ingredient.ItemID, ingredient.Quantity))
                {
                    Debug.Log($"Missing ingredient: {ingredient.ItemID}");
                    return false;
                }
            }
            
            // Remove ingredients from inventory
            foreach (RecipeIngredient ingredient in recipe.Ingredients)
            {
                // Find the ingredient in the inventory
                List<InventoryItem> items = inventorySystem.GetAllItems();
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].ItemID == ingredient.ItemID)
                    {
                        inventorySystem.RemoveItem(i, ingredient.Quantity);
                        break;
                    }
                }
            }
            
            // Create the crafted item
            InventoryItem craftedItem = new InventoryItem
            {
                ItemID = recipe.ResultItemID,
                ItemName = recipe.ResultItemName,
                Description = recipe.ResultDescription,
                Icon = recipe.ResultIcon,
                Quantity = recipe.ResultQuantity,
                Weight = recipe.ResultWeight,
                IsStackable = recipe.ResultIsStackable,
                Type = recipe.ResultType
            };
            
            // Add the crafted item to inventory
            bool added = inventorySystem.AddItem(craftedItem);
            
            if (added)
            {
                Debug.Log($"Successfully crafted: {recipe.ResultItemName}");
            }
            else
            {
                Debug.Log("Inventory full! Cannot add crafted item.");
                // Return ingredients if crafting fails due to full inventory
                ReturnIngredients(recipe);
            }
            
            return added;
        }
        
        /// <summary>
        /// Returns ingredients to the inventory if crafting fails
        /// </summary>
        /// <param name="recipe">The recipe whose ingredients should be returned</param>
        private void ReturnIngredients(CraftingRecipe recipe)
        {
            foreach (RecipeIngredient ingredient in recipe.Ingredients)
            {
                InventoryItem returnItem = new InventoryItem
                {
                    ItemID = ingredient.ItemID,
                    ItemName = ingredient.ItemName,
                    Quantity = ingredient.Quantity,
                    IsStackable = true
                };
                
                inventorySystem.AddItem(returnItem);
            }
        }
        
        /// <summary>
        /// Gets all available crafting recipes
        /// </summary>
        /// <returns>List of available recipes</returns>
        public List<CraftingRecipe> GetAvailableRecipes()
        {
            return availableRecipes;
        }
        
        /// <summary>
        /// Checks if a recipe can be crafted with current inventory
        /// </summary>
        /// <param name="recipeIndex">Index of the recipe to check</param>
        /// <returns>True if the recipe can be crafted</returns>
        public bool CanCraftRecipe(int recipeIndex)
        {
            if (recipeIndex < 0 || recipeIndex >= availableRecipes.Count)
            {
                return false;
            }
            
            CraftingRecipe recipe = availableRecipes[recipeIndex];
            
            foreach (RecipeIngredient ingredient in recipe.Ingredients)
            {
                if (!inventorySystem.HasItem(ingredient.ItemID, ingredient.Quantity))
                {
                    return false;
                }
            }
            
            return true;
        }
    }
    
    /// <summary>
    /// Represents a crafting recipe
    /// </summary>
    [System.Serializable]
    public class CraftingRecipe
    {
        public string RecipeName;
        public List<RecipeIngredient> Ingredients = new List<RecipeIngredient>();
        
        // Result item properties
        public string ResultItemID;
        public string ResultItemName;
        public string ResultDescription;
        public Sprite ResultIcon;
        public int ResultQuantity = 1;
        public float ResultWeight = 1f;
        public bool ResultIsStackable = true;
        public InventoryItem.ItemType ResultType;
    }
    
    /// <summary>
    /// Represents an ingredient in a crafting recipe
    /// </summary>
    [System.Serializable]
    public class RecipeIngredient
    {
        public string ItemID;
        public string ItemName;
        public int Quantity = 1;
    }
}
