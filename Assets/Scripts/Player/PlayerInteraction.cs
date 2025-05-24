using UnityEngine;
using UnityEngine.UI;
using Resilience.Resources;

namespace Resilience.Player
{
    /// <summary>
    /// Handles player interaction with objects in the game world
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private float interactionDistance = 3f;
        [SerializeField] private LayerMask interactionLayer;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Text interactionPromptText;
        
        private IInteractable currentInteractable;
        
        private void Start()
        {
            if (playerCamera == null)
                playerCamera = Camera.main;
                
            if (interactionPromptText != null)
                interactionPromptText.gameObject.SetActive(false);
        }
        
        private void Update()
        {
            // Check if game is paused
            if (Time.timeScale == 0f)
                return;
                
            CheckForInteractable();
            
            // Handle interaction input
            if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
            {
                currentInteractable.Interact(gameObject);
            }
        }
        
        /// <summary>
        /// Checks for interactable objects in front of the player
        /// </summary>
        private void CheckForInteractable()
        {
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, interactionDistance, interactionLayer))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                
                if (interactable != null)
                {
                    // Found an interactable
                    if (currentInteractable != interactable)
                    {
                        currentInteractable = interactable;
                        UpdateInteractionPrompt();
                    }
                    return;
                }
            }
            
            // No interactable found
            if (currentInteractable != null)
            {
                currentInteractable = null;
                UpdateInteractionPrompt();
            }
        }
        
        /// <summary>
        /// Updates the interaction prompt UI
        /// </summary>
        private void UpdateInteractionPrompt()
        {
            if (interactionPromptText == null)
                return;
                
            if (currentInteractable != null)
            {
                interactionPromptText.text = $"[E] {currentInteractable.GetInteractionPrompt()}";
                interactionPromptText.gameObject.SetActive(true);
            }
            else
            {
                interactionPromptText.gameObject.SetActive(false);
            }
        }
    }
}
