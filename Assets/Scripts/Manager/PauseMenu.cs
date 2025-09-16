using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI; 



    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        pauseMenuUI.SetActive(false);
    }

    void Pause()
    {
        Time.timeScale = 0f;
        GameIsPaused = true;
        pauseMenuUI.SetActive(true);
    }

    public void LoadMenu()
    {
        Debug.Log("Loading Menu...");
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleMenu");
        //SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        //Debug.Log("Quitting Game...");
        //Application.Quit();
        Debug.Log("Quitting game...");
        Application.Quit();
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // Stops play mode in the editor
    #endif
    }
}
