using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    #region Variables
    [Header("UI Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private TMP_Text scoreText;

    [Header("Level Rules")]
    [SerializeField] private int milksRequired = 10;
    [SerializeField] private float levelTime = 60f;

    [Header("Timer Warning")]
    [SerializeField] private float warningThreshold = 10f;
    [SerializeField] private AudioSource tickAudio;

    [Header("Game Event Sounds")]
    [SerializeField] private AudioSource winAudio;
    [SerializeField] private AudioSource loseAudio;

    private int _currentScore = 0;
    private int _lastTickSecond = -1;
    private float _timeRemaining;
    private bool _timerRunning = false;
    private bool _gameEnded = false;
    private bool _isPaused = false;

    public int MilksRequired => milksRequired;
    public float TimeRemaining => _timeRemaining;
    public float WarningThreshold => warningThreshold;
    public bool GameEnded => _gameEnded;
    #endregion

    #region Unity Callbacks
    void Start()
    {
        _timeRemaining = levelTime;
        _lastTickSecond = -1;
        _timerRunning = true;
        UpdateScoreUI();
    }

    void Update()
    {
        if (!_timerRunning || _gameEnded) return;

        _timeRemaining -= Time.deltaTime;

        if (_timeRemaining <= 0f)
        {
            _timeRemaining = 0f;
            _timerRunning = false;
            TimerGameOver();
            return;
        }

        if (_timeRemaining <= warningThreshold)
        {
            int currentSecond = Mathf.CeilToInt(_timeRemaining);
            if (currentSecond != _lastTickSecond)
            {
                _lastTickSecond = currentSecond;
                if (tickAudio && tickAudio.clip != null)
                    tickAudio.PlayOneShot(tickAudio.clip);
            }
        }

        // Keyboard ESC
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            Pause();

        // Controller Start button (the little one, also called Menu button on Xbox)
        if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
            Pause();
    }
    #endregion

    #region Public Methods
    public void AddScore()
    {
        _currentScore++;
        UpdateScoreUI();
        if (_currentScore >= milksRequired)
            WinGame();
    }

    public void WinGame()
    {
        if (_gameEnded) return;
        _gameEnded = true;
        _timerRunning = false;

        // Stop background/chase music immediately
        if (MusicManager.Instance != null)
            MusicManager.Instance.StopAll();

        // Play win sound and use its length as the delay
        float winClipLength = 0.5f; // fallback
        if (winAudio && winAudio.clip != null)
        {
            winAudio.PlayOneShot(winAudio.clip);
            winClipLength = winAudio.clip.length;
        }

        StartCoroutine(ShowWinDelayed(winClipLength));
    }

    // Called by PlayerCollision with the death clip length
    // Called by Update (timer) with no argument — defaults to loseAudio length
    public void GameOver(float delay = -1f)
    {
        if (_gameEnded) return;
        _gameEnded = true;
        _timerRunning = false;

        // Stop background/chase music immediately
        if (MusicManager.Instance != null)
            MusicManager.Instance.StopAll();

        // If delay was passed (death clip), use it
        // Otherwise play the lose sound and use its length
        if (delay < 0f)
        {
            delay = 0.5f; // fallback
            if (loseAudio && loseAudio.clip != null)
            {
                loseAudio.PlayOneShot(loseAudio.clip);
                delay = loseAudio.clip.length;
            }
        }

        StartCoroutine(ShowLoseDelayed(delay));
    }

    public void Pause()
    {
        _isPaused = !_isPaused;
        pausePanel.SetActive(_isPaused);

        // 1. Pause Time
        Time.timeScale = _isPaused ? 0f : 1f;

        // 2. Pause All Audio
        AudioListener.pause = _isPaused;
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        if (MusicManager.Instance != null)
            MusicManager.Instance.StopAll();
        SceneManager.LoadScene(0);
    }
    #endregion

    #region Private Methods
    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = _currentScore + "/" + milksRequired;
    }
    private void TimerGameOver()
    {
        if (_gameEnded) return;
        _gameEnded = true;
        _timerRunning = false;

        if (MusicManager.Instance != null)
            MusicManager.Instance.StopAll();

        StartCoroutine(ShowLoseDelayed(0.3f));
    }
    #endregion

    #region Coroutines
    IEnumerator ShowLoseDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Play lose sound when panel appears, works for both scenarios
        if (loseAudio && loseAudio.clip != null)
        {
            loseAudio.ignoreListenerPause = true; // Survives Time.timeScale = 0
            loseAudio.PlayOneShot(loseAudio.clip);
        }

        losePanel.SetActive(true);
        Time.timeScale = 0;
    }

    IEnumerator ShowWinDelayed(float delay)
    {
        yield return new WaitForSeconds(0.5f);
        winPanel.SetActive(true);
        Time.timeScale = 0;
    }
    #endregion
}
