using UnityEngine;
using Resilience.Core;

namespace Resilience.Player
{
    /// <summary>
    /// Manages player survival statistics
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float healthRegenRate = 0.5f;
        [SerializeField] private float healthRegenDelay = 5f;
        
        [Header("Hunger Settings")]
        [SerializeField] private float maxHunger = 100f;
        [SerializeField] private float hungerDecreaseRate = 0.05f;
        [SerializeField] private float hungerDamageThreshold = 0f;
        [SerializeField] private float hungerDamageRate = 1f;
        
        [Header("Thirst Settings")]
        [SerializeField] private float maxThirst = 100f;
        [SerializeField] private float thirstDecreaseRate = 0.08f;
        [SerializeField] private float thirstDamageThreshold = 0f;
        [SerializeField] private float thirstDamageRate = 1.5f;
        
        [Header("Temperature Settings")]
        [SerializeField] private float maxTemperature = 40f; // Body temperature in Celsius
        [SerializeField] private float minTemperature = 34f;
        [SerializeField] private float optimalTemperature = 37f;
        [SerializeField] private float temperatureDamageRate = 2f;
        
        [Header("Stamina Settings")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaRegenRate = 5f;
        [SerializeField] private float staminaRegenDelay = 1f;
        
        // Current stat values
        private float currentHealth;
        private float currentHunger;
        private float currentThirst;
        private float currentTemperature;
        private float currentStamina;
        
        // Timers
        private float lastDamageTime;
        private float lastStaminaUseTime;
        
        private void Start()
        {
            // Initialize stats
            currentHealth = maxHealth;
            currentHunger = maxHunger;
            currentThirst = maxThirst;
            currentTemperature = optimalTemperature;
            currentStamina = maxStamina;
            
            lastDamageTime = -healthRegenDelay;
            lastStaminaUseTime = -staminaRegenDelay;
        }
        
        private void Update()
        {
            UpdateHunger();
            UpdateThirst();
            UpdateTemperature();
            UpdateHealth();
            UpdateStamina();
            
            CheckForDeath();
        }
        
        /// <summary>
        /// Updates hunger stat
        /// </summary>
        private void UpdateHunger()
        {
            // Decrease hunger over time
            currentHunger -= hungerDecreaseRate * Time.deltaTime;
            currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
            
            // Apply damage if hunger is critically low
            if (currentHunger <= hungerDamageThreshold)
            {
                TakeDamage(hungerDamageRate * Time.deltaTime);
            }
        }
        
        /// <summary>
        /// Updates thirst stat
        /// </summary>
        private void UpdateThirst()
        {
            // Decrease thirst over time
            currentThirst -= thirstDecreaseRate * Time.deltaTime;
            currentThirst = Mathf.Clamp(currentThirst, 0f, maxThirst);
            
            // Apply damage if thirst is critically low
            if (currentThirst <= thirstDamageThreshold)
            {
                TakeDamage(thirstDamageRate * Time.deltaTime);
            }
        }
        
        /// <summary>
        /// Updates temperature stat
        /// </summary>
        private void UpdateTemperature()
        {
            // Temperature would normally be affected by environment, clothing, etc.
            // For now, we'll assume it stays at optimal temperature
            
            // Apply damage if temperature is too high or too low
            if (currentTemperature > maxTemperature || currentTemperature < minTemperature)
            {
                float tempDifference = Mathf.Max(
                    Mathf.Abs(currentTemperature - maxTemperature),
                    Mathf.Abs(currentTemperature - minTemperature)
                );
                
                TakeDamage(tempDifference * temperatureDamageRate * Time.deltaTime);
            }
        }
        
        /// <summary>
        /// Updates health stat
        /// </summary>
        private void UpdateHealth()
        {
            // Regenerate health if not recently damaged
            if (Time.time - lastDamageTime >= healthRegenDelay)
            {
                // Only regenerate if hunger and thirst are above threshold
                if (currentHunger > hungerDamageThreshold && currentThirst > thirstDamageThreshold)
                {
                    currentHealth += healthRegenRate * Time.deltaTime;
                    currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
                }
            }
        }
        
        /// <summary>
        /// Updates stamina stat
        /// </summary>
        private void UpdateStamina()
        {
            // Regenerate stamina if not recently used
            if (Time.time - lastStaminaUseTime >= staminaRegenDelay)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
            }
        }
        
        /// <summary>
        /// Checks if player should die
        /// </summary>
        private void CheckForDeath()
        {
            if (currentHealth <= 0f)
            {
                Die();
            }
        }
        
        /// <summary>
        /// Handles player death
        /// </summary>
        private void Die()
        {
            Debug.Log("Player has died!");
            
            // Trigger game over
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
        
        /// <summary>
        /// Applies damage to the player
        /// </summary>
        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
            lastDamageTime = Time.time;
        }
        
        /// <summary>
        /// Heals the player
        /// </summary>
        public void Heal(float amount)
        {
            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        }
        
        /// <summary>
        /// Consumes food to increase hunger
        /// </summary>
        public void ConsumeFood(float amount)
        {
            currentHunger += amount;
            currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
        }
        
        /// <summary>
        /// Consumes water to increase thirst
        /// </summary>
        public void ConsumeWater(float amount)
        {
            currentThirst += amount;
            currentThirst = Mathf.Clamp(currentThirst, 0f, maxThirst);
        }
        
        /// <summary>
        /// Uses stamina
        /// </summary>
        public void UseStamina(float amount)
        {
            currentStamina -= amount;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
            lastStaminaUseTime = Time.time;
        }
        
        /// <summary>
        /// Changes player temperature
        /// </summary>
        public void ChangeTemperature(float amount)
        {
            currentTemperature += amount;
        }
        
        // Getters for UI and other systems
        public float GetHealthPercentage() => currentHealth / maxHealth;
        public float GetHungerPercentage() => currentHunger / maxHunger;
        public float GetThirstPercentage() => currentThirst / maxThirst;
        public float GetStaminaPercentage() => currentStamina / maxStamina;
        public float GetTemperature() => currentTemperature;
    }
}
