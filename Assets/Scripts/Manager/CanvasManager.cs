//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.SceneManagement;
//using TMPro;

///// <summary>
///// CanvasManager handles all UI interactions:
/////  - Menu navigation (Main, Settings, Credits, Pause)
/////  - Button click events (Play, Quit, Back, etc.)
/////  - Updating UI text (e.g., lives counter)
/////  - Pause/Resume game logic
///// Keeps UI code separate from GameManager logic.
///// </summary>
//public class CanvasManager : MonoBehaviour
//{
//    #region UI References
//    [Header("Buttons")]
//    public Button playButton;
//    public Button settingsButton;
//    public Button backFromSettingsButton;
//    public Button creditsButton;
//    public Button backFromCreditsButton;
//    public Button quitButton;

//    public Button resumeGameButton;
//    public Button returnToMenuButton;

//    [Header("Panels")]
//    public GameObject mainMenuPanel;
//    public GameObject settingsPanel;
//    public GameObject creditsPanel;
//    public GameObject pauseMenuPanel;

//    [Header("Text Elements")]
//    public TMP_Text livesText;
//    #endregion

//    #region Unity Callbacks
//    private void Start()
//    {
//        // --- MAIN MENU ---
//        if (playButton)
//            playButton.onClick.AddListener(() => LoadGameScene());

//        if (settingsButton)
//            settingsButton.onClick.AddListener(() => SetMenus(settingsPanel, mainMenuPanel));

//        if (backFromSettingsButton)
//            backFromSettingsButton.onClick.AddListener(() => SetMenus(mainMenuPanel, settingsPanel));

//        if (creditsButton)
//            creditsButton.onClick.AddListener(() => SetMenus(creditsPanel, mainMenuPanel));

//        if (backFromCreditsButton)
//            backFromCreditsButton.onClick.AddListener(() => SetMenus(mainMenuPanel, creditsPanel));

//        if (quitButton)
//            quitButton.onClick.AddListener(QuitGame);

//        // --- PAUSE MENU ---
//        if (resumeGameButton)
//            resumeGameButton.onClick.AddListener(() => ResumeGame());

//        if (returnToMenuButton)
//            returnToMenuButton.onClick.AddListener(() => LoadTitleMenu());

//        // --- LIVES UI ---
//        if (livesText)
//        {
//            livesText.text = $"Lives: {GameManager.Instance.Lives}";
//            GameManager.Instance.OnLivesChanged += (lives) =>
//            {
//                livesText.text = $"Lives: {lives}";
//                Debug.Log($"[CanvasManager] Lives UI updated: {lives}");
//            };
//        }

//        Debug.Log("[CanvasManager] UI initialized.");
//    }

//    private void Update()
//    {
//        // Toggle pause menu with P key
//        if (pauseMenuPanel && Input.GetKeyDown(KeyCode.P))
//        {
//            if (pauseMenuPanel.activeSelf)
//                ResumeGame();
//            else
//                PauseGame();
//        }
//    }
//    #endregion

//    #region Menu Navigation
//    /// <summary>
//    /// Activates one menu panel while deactivating another.
//    /// </summary>
//    private void SetMenus(GameObject menuToActivate, GameObject menuToDeactivate)
//    {
//        if (menuToActivate)
//        {
//            menuToActivate.SetActive(true);
//            Debug.Log($"[CanvasManager] Activated panel: {menuToActivate.name}");
//        }

//        if (menuToDeactivate)
//        {
//            menuToDeactivate.SetActive(false);
//            Debug.Log($"[CanvasManager] Deactivated panel: {menuToDeactivate.name}");
//        }
//    }
//    #endregion

//    #region Game Controls
//    /// <summary>
//    /// Loads the main game scene.
//    /// </summary>
//    private void LoadGameScene()
//    {
//        Debug.Log("[CanvasManager] Play button pressed → Loading GameScene.");
//        SceneManager.LoadScene("GameScene");
//    }

//    /// <summary>
//    /// Returns to the title menu scene.
//    /// </summary>
//    private void LoadTitleMenu()
//    {
//        Debug.Log("[CanvasManager] Returning to Title Menu.");
//        SceneManager.LoadScene("TitleMenu");
//    }

//    /// <summary>
//    /// Quits the game (stops Play Mode in Editor).
//    /// </summary>
//    private void QuitGame()
//    {
//        Debug.Log("[CanvasManager] Quit button pressed → Closing game.");

//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPlaying = false;
//#else
//        Application.Quit();
//#endif
//    }

//    /// <summary>
//    /// Pauses the game (shows pause menu and freezes time).
//    /// </summary>
//    private void PauseGame()
//    {
//        SetMenus(pauseMenuPanel, null);
//        Time.timeScale = 0f; // Freeze gameplay
//        Debug.Log("[CanvasManager] Game Paused.");
//    }

//    /// <summary>
//    /// Resumes the game (hides pause menu and unfreezes time).
//    /// </summary>
//    private void ResumeGame()
//    {
//        SetMenus(null, pauseMenuPanel);
//        Time.timeScale = 1f; // Resume gameplay
//        Debug.Log("[CanvasManager] Game Resumed.");
//    }
//    #endregion
//}


using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public void StartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        else
        {
            Debug.LogError("GameManager not found! Make sure it exists in the scene.");
        }
    }

    public void ExitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stops play mode in the editor
#endif
    }

    public void LoadTitleMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadTitleMenu();
        }
        else
        {
            Debug.LogError("GameManager not found! Make sure it exists in the scene.");
        }
    }
}
