using UnityEngine;

namespace Resilience.Player
{
    /// <summary>
    /// Handles player movement and camera controls
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float crouchSpeed = 2.5f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float gravity = -9.81f;
        
        [Header("Camera Settings")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float lookUpLimit = 90f;
        [SerializeField] private float standingHeight = 2f;
        [SerializeField] private float crouchingHeight = 1f;
        
        // Components
        private CharacterController characterController;
        private PlayerStats playerStats;
        
        // Movement variables
        private Vector3 moveDirection = Vector3.zero;
        private float verticalVelocity = 0f;
        private float cameraPitch = 0f;
        private bool isCrouching = false;
        private bool isGrounded = false;
        
        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            playerStats = GetComponent<PlayerStats>();
            
            if (playerCamera == null)
                playerCamera = Camera.main;
                
            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        private void Update()
        {
            // Check if game is paused
            if (Time.timeScale == 0f)
                return;
                
            HandleMouseLook();
            HandleMovement();
            HandleJump();
            HandleCrouch();
        }
        
        /// <summary>
        /// Handles mouse look (camera rotation)
        /// </summary>
        private void HandleMouseLook()
        {
            // Get mouse input
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            
            // Rotate camera up/down
            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, -lookUpLimit, lookUpLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
            
            // Rotate player left/right
            transform.Rotate(Vector3.up * mouseX);
        }
        
        /// <summary>
        /// Handles player movement
        /// </summary>
        private void HandleMovement()
        {
            // Check if grounded
            isGrounded = characterController.isGrounded;
            
            if (isGrounded && verticalVelocity < 0)
            {
                verticalVelocity = -0.5f;
            }
            
            // Get movement input
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
            
            // Calculate movement direction
            Vector3 move = transform.right * moveX + transform.forward * moveZ;
            
            // Determine speed based on sprint/crouch state
            float currentSpeed = walkSpeed;
            
            if (Input.GetKey(KeyCode.LeftShift) && !isCrouching && moveZ > 0)
            {
                // Sprint
                currentSpeed = sprintSpeed;
                
                // Use stamina when sprinting
                if (playerStats != null)
                {
                    playerStats.UseStamina(Time.deltaTime * 10f);
                    
                    // If out of stamina, revert to walking
                    if (playerStats.GetStaminaPercentage() <= 0f)
                    {
                        currentSpeed = walkSpeed;
                    }
                }
            }
            else if (isCrouching)
            {
                // Crouch
                currentSpeed = crouchSpeed;
            }
            
            // Apply movement
            characterController.Move(move * currentSpeed * Time.deltaTime);
            
            // Apply gravity
            verticalVelocity += gravity * Time.deltaTime;
            characterController.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
        }
        
        /// <summary>
        /// Handles jumping
        /// </summary>
        private void HandleJump()
        {
            if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
            {
                verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
                
                // Use stamina when jumping
                if (playerStats != null)
                {
                    playerStats.UseStamina(20f);
                    
                    // If out of stamina, reduce jump height
                    if (playerStats.GetStaminaPercentage() <= 0f)
                    {
                        verticalVelocity *= 0.5f;
                    }
                }
            }
        }
        
        /// <summary>
        /// Handles crouching
        /// </summary>
        private void HandleCrouch()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                isCrouching = !isCrouching;
                
                // Adjust character controller height
                characterController.height = isCrouching ? crouchingHeight : standingHeight;
                
                // Adjust camera position
                Vector3 cameraPos = playerCamera.transform.localPosition;
                cameraPos.y = isCrouching ? crouchingHeight / 2f : standingHeight / 2f;
                playerCamera.transform.localPosition = cameraPos;
            }
        }
    }
}
