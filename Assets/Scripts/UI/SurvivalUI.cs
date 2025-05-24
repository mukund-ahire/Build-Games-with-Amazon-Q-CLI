using UnityEngine;
using UnityEngine.UI;
using Resilience.Player;

namespace Resilience.UI
{
    /// <summary>
    /// Manages the UI elements for survival stats (health, hunger, thirst, stamina)
    /// </summary>
    public class SurvivalUI : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
        
        [Header("UI Elements")]
        [SerializeField] private Image healthBar;
        [SerializeField] private Image hungerBar;
        [SerializeField] private Image thirstBar;
        [SerializeField] private Image staminaBar;
        
        [Header("UI Colors")]
        [SerializeField] private Color healthyColor = Color.green;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color dangerColor = Color.red;
        [SerializeField] private float warningThreshold = 0.5f;
        [SerializeField] private float dangerThreshold = 0.25f;
        
        private void Start()
        {
            if (playerController == null)
                playerController = FindObjectOfType<PlayerController>();
        }
        
        private void Update()
        {
            if (playerController == null)
                return;
                
            UpdateHealthBar();
            UpdateHungerBar();
            UpdateThirstBar();
            UpdateStaminaBar();
        }
        
        /// <summary>
        /// Updates the health bar UI
        /// </summary>
        private void UpdateHealthBar()
        {
            if (healthBar == null)
                return;
                
            float healthPercentage = playerController.GetHealthPercentage();
            healthBar.fillAmount = healthPercentage;
            healthBar.color = GetColorForValue(healthPercentage);
        }
        
        /// <summary>
        /// Updates the hunger bar UI
        /// </summary>
        private void UpdateHungerBar()
        {
            if (hungerBar == null)
                return;
                
            float hungerPercentage = playerController.GetHungerPercentage();
            hungerBar.fillAmount = hungerPercentage;
            hungerBar.color = GetColorForValue(hungerPercentage);
        }
        
        /// <summary>
        /// Updates the thirst bar UI
        /// </summary>
        private void UpdateThirstBar()
        {
            if (thirstBar == null)
                return;
                
            float thirstPercentage = playerController.GetThirstPercentage();
            thirstBar.fillAmount = thirstPercentage;
            thirstBar.color = GetColorForValue(thirstPercentage);
        }
        
        /// <summary>
        /// Updates the stamina bar UI
        /// </summary>
        private void UpdateStaminaBar()
        {
            if (staminaBar == null)
                return;
                
            float staminaPercentage = playerController.GetStaminaPercentage();
            staminaBar.fillAmount = staminaPercentage;
            staminaBar.color = GetColorForValue(staminaPercentage);
        }
        
        /// <summary>
        /// Gets the appropriate color based on the value percentage
        /// </summary>
        private Color GetColorForValue(float value)
        {
            if (value <= dangerThreshold)
                return dangerColor;
            else if (value <= warningThreshold)
                return warningColor;
            else
                return healthyColor;
        }
    }
}
