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
