using UnityEngine;

namespace Resilience.Player
{
    /// <summary>
    /// Main player controller class that handles movement and basic interactions
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintMultiplier = 1.5f;
        [SerializeField] private float jumpForce = 5f;
        
        [Header("Survival Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float maxHunger = 100f;
        [SerializeField] private float maxThirst = 100f;
        [SerializeField] private float maxStamina = 100f;
        
        // Private variables
        private float currentHealth;
        private float currentHunger;
        private float currentThirst;
        private float currentStamina;
        
        private Rigidbody rb;
        private bool isGrounded;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            
            // Initialize survival stats
            currentHealth = maxHealth;
            currentHunger = maxHunger;
            currentThirst = maxThirst;
            currentStamina = maxStamina;
        }
        
        private void Update()
        {
            HandleMovementInput();
            HandleJumpInput();
            UpdateSurvivalStats();
        }
        
        private void HandleMovementInput()
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            
            Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;
            
            // Apply sprint if player is holding shift and has stamina
            float currentSpeed = moveSpeed;
            if (Input.GetKey(KeyCode.LeftShift) && currentStamina > 0)
            {
                currentSpeed *= sprintMultiplier;
                currentStamina -= Time.deltaTime * 10f; // Drain stamina while sprinting
            }
            else if (!Input.GetKey(KeyCode.LeftShift) && currentStamina < maxStamina)
            {
                currentStamina += Time.deltaTime * 5f; // Recover stamina when not sprinting
            }
            
            // Clamp stamina
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
            
            // Apply movement
            transform.Translate(movement * currentSpeed * Time.deltaTime);
        }
        
        private void HandleJumpInput()
        {
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;
            }
        }
        
        private void UpdateSurvivalStats()
        {
            // Decrease hunger and thirst over time
            currentHunger -= Time.deltaTime * 0.05f;
            currentThirst -= Time.deltaTime * 0.08f;
            
            // If hunger or thirst are critically low, decrease health
            if (currentHunger <= 0 || currentThirst <= 0)
            {
                currentHealth -= Time.deltaTime * 2f;
            }
            
            // Clamp values
            currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
            currentThirst = Mathf.Clamp(currentThirst, 0f, maxThirst);
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            
            // Check for death
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            // Check if player is on the ground
            if (collision.gameObject.CompareTag("Ground"))
            {
                isGrounded = true;
            }
        }
        
        private void Die()
        {
            Debug.Log("Player has died!");
            // Implement death mechanics here
        }
        
        // Public methods for other systems to interact with player stats
        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
        }
        
        public void Heal(float amount)
        {
            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        }
        
        public void ConsumeFood(float nutritionalValue)
        {
            currentHunger += nutritionalValue;
            currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
        }
        
        public void ConsumeWater(float hydrationValue)
        {
            currentThirst += hydrationValue;
            currentThirst = Mathf.Clamp(currentThirst, 0f, maxThirst);
        }
        
        // Getters for UI and other systems
        public float GetHealthPercentage() => currentHealth / maxHealth;
        public float GetHungerPercentage() => currentHunger / maxHunger;
        public float GetThirstPercentage() => currentThirst / maxThirst;
        public float GetStaminaPercentage() => currentStamina / maxStamina;
    }
}
