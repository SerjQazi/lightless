using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using UnityEngine.Audio;

[DefaultExecutionOrder(-10)]
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
    public enum GameState { Title, Playing, GameOver }
    public GameState currentState;
    #endregion

    #region Player
    [Header("Player Settings")]
    public PlayerController playerPrefab;   // prefab to spawn when starting a level
    private PlayerController _playerInstance;   // reference to the active player instance
    private Vector3 currentCheckpoint;  // keeps track of the last checkpoint

    public event Action<PlayerController> OnPlayerControllerCreated;
    #endregion

    #region Respawn
    [Header("Respawn Settings")]
    [SerializeField] private Transform RespawnPoint;
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private float waterDeathDelay = 5f;
    #endregion

    #region Audio
    public AudioMixerGroup masterMixGroup;
    public AudioMixerGroup sfxMixGroup;
    public AudioMixerGroup musicMixGroup;
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
        if (newLives <= 0)   // trigger GameOver when reaching 0
        {
            _lives = 0;
            Debug.Log("[GameManager] Game Over! No lives left.");
            GameOver();
        }
        else if (newLives < _lives)
        {
            _lives = newLives;
            Debug.Log("[GameManager] Lost a life.");
        }
        else if (newLives > maxLives)
        {
            _lives = maxLives;
            Debug.Log("[GameManager] Lives capped at max.");
        }
        else
        {
            _lives = newLives;
        }

        Debug.Log($"[GameManager] Current Lives: {_lives}");
        OnLivesChanged?.Invoke(_lives);
    }
    #endregion

    #region Powerups
    private Coroutine jumpForceChange = null;
    private int defaultJumpForce = 8;
    private int boostedJumpForce = 14;
    private float boostDuration = 5f;
    private int damage = 1;

    // Called by pickups to activate jump boost
    public void ActivateJumpForceChange()
    {
        if (_playerInstance == null) return;

        if (jumpForceChange != null)
        {
            // cancel old boost if active
            StopCoroutine(jumpForceChange);
            jumpForceChange = null;
            _playerInstance.jumpForce = defaultJumpForce;
        }

        // start new boost
        jumpForceChange = StartCoroutine(ChangeJumpForce());
    }

    private IEnumerator ChangeJumpForce()
    {
        _playerInstance.jumpForce = boostedJumpForce;
        Debug.Log("💥 Jump force increased!");
        yield return new WaitForSeconds(boostDuration);

        _playerInstance.jumpForce = defaultJumpForce;
        Debug.Log("⏳ Jump force reset.");
        jumpForceChange = null;
    }
    #endregion

    #region Death & Respawn
    public void HandlePlayerDeath()
    {
        Debug.Log("[GameManager] Player hit a death zone.");
        AudioManager.Instance.PlaySFX(AudioManager.Instance.Death);
        SetLives(_lives - 1);
        if (_lives > 0)
            
        StartCoroutine(RespawnAfterDelay(respawnDelay));
    }

    public void HandleWaterDeath()
    {
        Debug.Log("[GameManager] Player drowned.");
        SetLives(_lives - 1);
        if (_lives > 0)
            StartCoroutine(WaterDeathSequence());
    }

    public void HandlePlayerHitByProjectile(int damage = 1)
    {
        Debug.Log($"[GameManager] Player hit by projectile! Damage: {damage}");

        int newLives = _lives - damage;
        SetLives(newLives);

        Debug.Log($"[GameManager] Player lives after hit: {_lives}");

        if (_lives > 0)
        {
            Debug.Log("Player took damage but is still alive.");
            _playerInstance.GetComponent<Animator>()?.SetTrigger("Impact");
        }
        else
        {
            Debug.Log("💀 Player has died from projectile hits.");
            AudioManager.Instance.PlaySFX(AudioManager.Instance.Death);
            GameOver();
        }
    }

    private bool isInvincible = false;
    [SerializeField] private float invincibleTime = 1.0f;

    public void HandlePlayerHitByEnemy(int damage = 1)
    {
        if (isInvincible) return; // ignore hits while invincible

        Debug.Log($"[GameManager] Player hit by enemy! Damage: {damage}");

        SetLives(_lives - damage);
        Debug.Log($"[GameManager] Lives after hit: {_lives}");

        if (_lives > 0)
        {
            _playerInstance?.GetComponent<Animator>()?.SetTrigger("Impact");
            StartCoroutine(InvincibilityCoroutine());
        }
        else
        {
            Debug.Log("💀 Player has died.");
            AudioManager.Instance.PlaySFX(AudioManager.Instance.Death);
            GameOver();
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }

    private IEnumerator RespawnAfterDelay(float delay)
    {
        if (_playerInstance != null)
        {
            // Hide and disable player
            SpriteRenderer sr = _playerInstance.GetComponent<SpriteRenderer>();
            Rigidbody2D rb = _playerInstance.GetComponent<Rigidbody2D>();
            sr.enabled = false;
            rb.bodyType = RigidbodyType2D.Kinematic;

            yield return new WaitForSeconds(delay);

            // Respawn
            Vector3 respawnPos = currentCheckpoint != Vector3.zero
                ? currentCheckpoint
                : (RespawnPoint != null ? RespawnPoint.position : Vector3.zero);

            _playerInstance.transform.position = respawnPos;
            rb.bodyType = RigidbodyType2D.Dynamic;
            sr.enabled = true;

            Debug.Log($"[GameManager] Player respawned at {respawnPos}");
        }
    }

    private IEnumerator WaterDeathSequence()
    {
        if (_playerInstance != null)
        {
            Animator animator = _playerInstance.GetComponent<Animator>();
            SpriteRenderer sr = _playerInstance.GetComponent<SpriteRenderer>();
            Rigidbody2D rb = _playerInstance.GetComponent<Rigidbody2D>();

            // Play drowning struggle animation
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            animator.SetTrigger("struggleInWater");

            yield return new WaitForSeconds(waterDeathDelay);

            // Respawn
            sr.enabled = false;
            Vector3 respawnPos = currentCheckpoint != Vector3.zero
                ? currentCheckpoint
                : (RespawnPoint != null ? RespawnPoint.position : Vector3.zero);

            _playerInstance.transform.position = respawnPos;
            rb.bodyType = RigidbodyType2D.Dynamic;
            sr.enabled = true;

            Debug.Log($"[GameManager] Player respawned after drowning at {respawnPos}");
        }
    }
    #endregion

    #region Game Flow
    private void GameOver()
    {
        SetState(GameState.GameOver);
    }

    public void StartLevel(Vector3 startPosition)
    {
        currentCheckpoint = startPosition;
        _playerInstance = Instantiate(playerPrefab, currentCheckpoint, Quaternion.identity);
        Debug.Log($"[GameManager] Player spawned at {startPosition}");
        OnPlayerControllerCreated?.Invoke(_playerInstance);
    }

    public void ResetStats()
    {
        _lives = maxLives;
        _score = 0;
        Debug.Log("[GameManager] Stats reset: Lives=" + _lives + " Score=" + _score);
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        Debug.Log($"[GameManager] State changed to: {newState}");

        switch (newState)
        {
            case GameState.Title:
                SceneManager.LoadScene("TitleMenu");
                AudioManager.Instance.PlayMusic(AudioManager.Instance.MenuTrack);
                break;

            case GameState.Playing:
                ResetStats();
                SceneManager.LoadScene("GameScene");
                StartCoroutine(AssignRespawnPointAfterSceneLoad());
                AudioManager.Instance.PlayMusic(AudioManager.Instance.BackgroundTrack);
                break;

            case GameState.GameOver:
                SceneManager.LoadScene("GameOverMenu");
                AudioManager.Instance.PlayMusic(AudioManager.Instance.GameOverTrack);
                break;
        }
    }

    // ------------------- NEW SECTION -------------------
    [Header("Canvas Manager Setup")]
    [SerializeField] private CanvasManager canvasManagerPrefab;  // drag prefab here in inspector
    private CanvasManager _canvasManager; // runtime instance

    private IEnumerator AssignRespawnPointAfterSceneLoad()
    {
        yield return null; // wait one frame so scene objects are initialized

        // --- Assign respawn point ---
        GameObject found = GameObject.Find("RespawnPoint");
        if (found != null)
        {
            RespawnPoint = found.transform;
            Debug.Log("[GameManager] RespawnPoint assigned: " + RespawnPoint.position);
        }
        else
        {
            Debug.LogError("[GameManager] RespawnPoint not found in GameScene!");
        }

        // --- Ensure we always have a player instance ---
        if (_playerInstance == null)
        {
            _playerInstance = FindAnyObjectByType<PlayerController>();
            if (_playerInstance != null)
            {
                Debug.Log("[GameManager] Found player in scene.");
                OnPlayerControllerCreated?.Invoke(_playerInstance);
            }
        }

        // --- Ensure we always have a CanvasManager ---
        _canvasManager = FindAnyObjectByType<CanvasManager>();
        if (_canvasManager == null && canvasManagerPrefab != null)
        {
            _canvasManager = Instantiate(canvasManagerPrefab);
            Debug.Log("[GameManager] CanvasManager instantiated in GameScene.");
        }
        else if (_canvasManager != null)
        {
            Debug.Log("[GameManager] CanvasManager already present in scene.");
        }
        else
        {
            Debug.LogWarning("[GameManager] No CanvasManager prefab assigned!");
        }
    }
    // ---------------------------------------------------

    public void LoadTitleMenu() => SetState(GameState.Title);
    public void StartGame() => SetState(GameState.Playing);

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
    private void Start()
    {
        // Fallback for manually placed player in scene
        if (_playerInstance == null)
        {
            _playerInstance = FindAnyObjectByType<PlayerController>();
            if (_playerInstance != null)
            {
                Debug.Log("[GameManager] Found player in scene at Start.");
                OnPlayerControllerCreated?.Invoke(_playerInstance);
            }
        }
    }

    private void Update()
    {
        // quick way back to Title from GameOver
        if (currentState == GameState.GameOver && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("[GameManager] Escape pressed → Back to Title.");
            LoadTitleMenu();
        }
    }
    #endregion
}
