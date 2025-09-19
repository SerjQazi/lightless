using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages UI-related actions such as starting the game,
/// quitting, or loading the title menu.
/// Attach this script to your UI Canvas (or a dedicated UI Manager object).
/// Then, hook up the buttons in the Inspector to call these methods.
/// </summary>
public class CanvasManager : MonoBehaviour
{
    /// <summary>
    /// Called when the "Start Game" button is pressed.
    /// This tells the GameManager to begin gameplay.
    /// </summary>
    public void StartGame()
    {
        if (GameManager.Instance != null)
        {
            Debug.Log("[CanvasManager] Start Game pressed.");
            GameManager.Instance.StartGame();
        }
        else
        {
            Debug.LogError("[CanvasManager] GameManager not found! Make sure it exists in the scene.");
        }
    }

    /// <summary>
    /// Called when the "Exit Game" button is pressed.
    /// Quits the game in a build, or stops play mode in the Unity Editor.
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("[CanvasManager] Exit Game pressed → Quitting game...");

        Application.Quit();

#if UNITY_EDITOR
        // Only works in the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    /// <summary>
    /// Called when the "Back to Title" button is pressed.
    /// Switches scenes back to the Title Menu using the GameManager.
    /// </summary>
    public void LoadTitleMenu()
    {
        if (GameManager.Instance != null)
        {
            Debug.Log("[CanvasManager] Load Title Menu pressed.");
            GameManager.Instance.LoadTitleMenu();
        }
        else
        {
            Debug.LogError("[CanvasManager] GameManager not found! Make sure it exists in the scene.");
        }
    }
}
