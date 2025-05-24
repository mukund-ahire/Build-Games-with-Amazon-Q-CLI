using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Resilience.Crafting;

namespace Resilience.UI
{
    /// <summary>
    /// Manages the UI for the crafting system
    /// </summary>
    public class CraftingUI : MonoBehaviour
    {
        [SerializeField] private CraftingSystem craftingSystem;
        [SerializeField] private GameObject craftingPanel;
        [SerializeField] private Transform recipesContainer;
        [SerializeField] private GameObject recipeSlotPrefab;
        
        private List<GameObject> recipeSlots = new List<GameObject>();
        private bool isCraftingOpen = false;
        
        private void Start()
        {
            if (craftingSystem == null)
                craftingSystem = FindObjectOfType<CraftingSystem>();
                
            if (craftingPanel != null)
                craftingPanel.SetActive(false);
        }
        
        private void Update()
        {
            // Toggle crafting with C key
            if (Input.GetKeyDown(KeyCode.C))
            {
                ToggleCrafting();
            }
        }
        
        /// <summary>
        /// Toggles the crafting panel visibility
        /// </summary>
        public void ToggleCrafting()
        {
            isCraftingOpen = !isCraftingOpen;
            
            if (craftingPanel != null)
                craftingPanel.SetActive(isCraftingOpen);
                
            if (isCraftingOpen)
                UpdateCraftingUI();
                
            // Toggle cursor visibility and lock state
            Cursor.visible = isCraftingOpen;
            Cursor.lockState = isCraftingOpen ? CursorLockMode.None : CursorLockMode.Locked;
        }
        
        /// <summary>
        /// Updates the crafting UI to show available recipes
        /// </summary>
        public void UpdateCraftingUI()
        {
            if (craftingSystem == null || recipesContainer == null || recipeSlotPrefab == null)
                return;
                
            // Clear existing slots
            foreach (GameObject slot in recipeSlots)
            {
                Destroy(slot);
            }
            recipeSlots.Clear();
            
            // Get all recipes
            List<CraftingRecipe> recipes = craftingSystem.GetAvailableRecipes();
            
            // Create slots for each recipe
            for (int i = 0; i < recipes.Count; i++)
            {
                CraftingRecipe recipe = recipes[i];
                GameObject slotObject = Instantiate(recipeSlotPrefab, recipesContainer);
                recipeSlots.Add(slotObject);
                
                // Set recipe name
                Text recipeNameText = slotObject.transform.Find("RecipeName")?.GetComponent<Text>();
                if (recipeNameText != null)
                    recipeNameText.text = recipe.RecipeName;
                    
                // Set recipe result icon
                Image resultIcon = slotObject.transform.Find("ResultIcon")?.GetComponent<Image>();
                if (resultIcon != null && recipe.ResultIcon != null)
                    resultIcon.sprite = recipe.ResultIcon;
                    
                // Set ingredients text
                Text ingredientsText = slotObject.transform.Find("IngredientsText")?.GetComponent<Text>();
                if (ingredientsText != null)
                {
                    string ingredients = "Requires: ";
                    foreach (RecipeIngredient ingredient in recipe.Ingredients)
                    {
                        ingredients += $"{ingredient.ItemName} x{ingredient.Quantity}, ";
                    }
                    // Remove trailing comma and space
                    if (recipe.Ingredients.Count > 0)
                        ingredients = ingredients.Substring(0, ingredients.Length - 2);
                        
                    ingredientsText.text = ingredients;
                }
                
                // Add craft button functionality
                Button craftButton = slotObject.transform.Find("CraftButton")?.GetComponent<Button>();
                if (craftButton != null)
                {
                    int recipeIndex = i;
                    craftButton.onClick.AddListener(() => CraftRecipe(recipeIndex));
                    
                    // Update button interactability based on whether recipe can be crafted
                    craftButton.interactable = craftingSystem.CanCraftRecipe(i);
                }
            }
        }
        
        /// <summary>
        /// Crafts the recipe at the specified index
        /// </summary>
        private void CraftRecipe(int recipeIndex)
        {
            if (craftingSystem.CraftItem(recipeIndex))
            {
                Debug.Log($"Successfully crafted recipe at index {recipeIndex}");
                // Update UI to reflect changes in available materials
                UpdateCraftingUI();
            }
            else
            {
                Debug.Log($"Failed to craft recipe at index {recipeIndex}");
            }
        }
    }
}
