using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections; // Needed for IEnumerator coroutines

/// <summary>
/// GameManager is responsible for:
///  - Managing the overall game state (Title, Playing, GameOver)
///  - Tracking score and player lives
///  - Spawning and respawning the player
///  - Scene transitions
/// It uses the Singleton pattern so only one instance exists.
/// </summary>
[DefaultExecutionOrder(-10)] // Makes sure this initializes before most other scripts
public class GameManager : MonoBehaviour
{
    #region Singleton Pattern
    private static GameManager _instance;
    public static GameManager Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameManager] Created and marked as DontDestroyOnLoad.");
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("[GameManager] Duplicate instance destroyed.");
        }
    }
    #endregion

    #region Game State
    public enum GameState
    {
        Title,     // Title menu scene
        Playing,   // Actively playing the game
        GameOver   // Game over screen
    }

    public GameState currentState;
    #endregion

    #region Player
    [Header("Player Settings")]
    public PlayerController playerPrefab;      // Prefab for creating the player
    private PlayerController _playerInstance;  // Reference to the active player
    private Vector3 currentCheckpoint;         // Last checkpoint position

    public event Action<PlayerController> OnPlayerControllerCreated;
    #endregion

    #region Respawn
    [Header("Respawn Settings")]
    [SerializeField] private Transform RespawnPoint; // Default respawn location in the scene
    #endregion

    #region Score & Lives
    [Header("Stats")]
    [SerializeField] private int maxLives = 3;
    private int _lives = 3;
    private int _score = 0;

    public int Score => _score;
    public int Lives => _lives;

    public event Action<int> OnLivesChanged;

    public void AddScore(int amount)
    {
        _score = Mathf.Max(0, _score + amount);
        Debug.Log($"[GameManager] Score updated: {_score}");
    }

    public void SetLives(int newLives)
    {
        if (newLives < 0)
        {
            _lives = 0;
            Debug.Log("[GameManager] Game Over! No lives left.");
            GameOver();
        }
        else if (newLives < _lives)
        {
            _lives = newLives;
            Debug.Log("[GameManager] Ouch! Lost a life.");
            Respawn();
        }
        else if (newLives > maxLives)
        {
            _lives = maxLives;
            Debug.Log("[GameManager] Lives capped at max.");
        }
        else
        {
            _lives = newLives;
            Debug.Log("[GameManager] Lives updated.");
        }

        Debug.Log($"[GameManager] Current Lives: {_lives}");
        OnLivesChanged?.Invoke(_lives);
    }
    #endregion

    #region Game Flow
    private void GameOver()
    {
        SetState(GameState.GameOver);
    }

    private void Respawn()
    {
        if (_playerInstance != null)
        {
            Vector3 respawnPos = currentCheckpoint != Vector3.zero
                ? currentCheckpoint
                : (RespawnPoint != null ? RespawnPoint.position : Vector3.zero);

            _playerInstance.transform.position = respawnPos;
            Debug.Log($"[GameManager] Player respawned at {respawnPos}");
        }
        else
        {
            Debug.LogWarning("[GameManager] Tried to respawn, but no player instance exists.");
        }
    }

    public void StartLevel(Vector3 startPosition)
    {
        currentCheckpoint = startPosition;
        _playerInstance = Instantiate(playerPrefab, currentCheckpoint, Quaternion.identity);

        Debug.Log($"[GameManager] Player spawned at {startPosition}");
        OnPlayerControllerCreated?.Invoke(_playerInstance);
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        Debug.Log($"[GameManager] State changed to: {newState}");

        switch (newState)
        {
            case GameState.Title:
                SceneManager.LoadScene("TitleMenu");
                break;

            case GameState.Playing:
                _lives = maxLives;
                _score = 0;
                Debug.Log("[GameManager] Starting game: lives and score reset.");
                SceneManager.LoadScene("GameScene");
                StartCoroutine(AssignRespawnPointAfterSceneLoad());
                break;

            case GameState.GameOver:
                SceneManager.LoadScene("GameOverMenu");
                break;
        }
    }

    //private IEnumerator AssignRespawnPointAfterSceneLoad()
    //{
    //    yield return null;

    //    GameObject found = GameObject.Find("RespawnPoint");
    //    if (found != null)
    //    {
    //        RespawnPoint = found.transform;
    //        Debug.Log("[GameManager] RespawnPoint assigned: " + RespawnPoint.position);
    //    }
    //    else
    //    {
    //        Debug.LogError("[GameManager] RespawnPoint not found in GameScene!");
    //    }
    //}

    private IEnumerator AssignRespawnPointAfterSceneLoad()
    {
        // Wait one frame for the new scene to load
        yield return null;

        GameObject found = GameObject.Find("RespawnPoint");
        if (found != null)
        {
            RespawnPoint = found.transform;
            Debug.Log("RespawnPoint assigned: " + RespawnPoint.position);
        }
        else
        {
            Debug.LogError("RespawnPoint not found in GameScene!");
        }
    }

    public void LoadTitleMenu()
    {
        Debug.Log("[GameManager] Returning to Title Menu.");
        SetState(GameState.Title);
    }

    public void StartGame()
    {
        Debug.Log("[GameManager] Starting Game.");
        SetState(GameState.Playing);
    }

    public void ExitGame()
    {
        Debug.Log("[GameManager] Quitting Game.");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    #endregion

    #region Unity Callbacks
    private void Update()
    {
        if (currentState == GameState.GameOver && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("[GameManager] Escape pressed during GameOver → Loading Title Menu.");
            LoadTitleMenu();
        }
    }
    #endregion
}
