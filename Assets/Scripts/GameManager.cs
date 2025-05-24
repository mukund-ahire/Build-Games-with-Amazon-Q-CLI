using UnityEngine;
using UnityEngine.SceneManagement;

namespace Resilience.Core
{
    /// <summary>
    /// Central manager for game state and systems
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // Singleton instance
        public static GameManager Instance { get; private set; }
        
        [Header("Game Settings")]
        [SerializeField] private bool isPaused = false;
        [SerializeField] private GameObject pauseMenuUI;
        [SerializeField] private GameObject gameOverUI;
        [SerializeField] private GameObject mainMenuUI;
        
        [Header("Save/Load")]
        [SerializeField] private float autoSaveInterval = 300f; // 5 minutes
        
        private float autoSaveTimer = 0f;
        private bool isGameOver = false;
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start()
        {
            // Initialize UI
            if (pauseMenuUI != null)
                pauseMenuUI.SetActive(false);
                
            if (gameOverUI != null)
                gameOverUI.SetActive(false);
                
            // Show main menu if we're in the main menu scene
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                if (mainMenuUI != null)
                    mainMenuUI.SetActive(true);
            }
            else
            {
                if (mainMenuUI != null)
                    mainMenuUI.SetActive(false);
            }
            
            // Lock cursor for gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        private void Update()
        {
            // Handle pause menu
            if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
            {
                TogglePause();
            }
            
            // Auto-save
            if (!isPaused && !isGameOver)
            {
                autoSaveTimer += Time.deltaTime;
                if (autoSaveTimer >= autoSaveInterval)
                {
                    autoSaveTimer = 0f;
                    SaveGame();
                }
            }
        }
        
        /// <summary>
        /// Toggles the game pause state
        /// </summary>
        public void TogglePause()
        {
            isPaused = !isPaused;
            
            if (isPaused)
            {
                Time.timeScale = 0f;
                if (pauseMenuUI != null)
                    pauseMenuUI.SetActive(true);
                    
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Time.timeScale = 1f;
                if (pauseMenuUI != null)
                    pauseMenuUI.SetActive(false);
                    
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        
        /// <summary>
        /// Triggers game over state
        /// </summary>
        public void GameOver()
        {
            isGameOver = true;
            
            if (gameOverUI != null)
                gameOverUI.SetActive(true);
                
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        /// <summary>
        /// Starts a new game
        /// </summary>
        public void StartNewGame()
        {
            isGameOver = false;
            isPaused = false;
            Time.timeScale = 1f;
            
            SceneManager.LoadScene("GameScene");
        }
        
        /// <summary>
        /// Returns to the main menu
        /// </summary>
        public void ReturnToMainMenu()
        {
            isGameOver = false;
            isPaused = false;
            Time.timeScale = 1f;
            
            SceneManager.LoadScene("MainMenu");
        }
        
        /// <summary>
        /// Quits the game
        /// </summary>
        public void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        /// <summary>
        /// Saves the current game state
        /// </summary>
        public void SaveGame()
        {
            Debug.Log("Game saved!");
            // Implementation will depend on save system
        }
        
        /// <summary>
        /// Loads a saved game
        /// </summary>
        public void LoadGame()
        {
            Debug.Log("Game loaded!");
            // Implementation will depend on save system
        }
    }
}
