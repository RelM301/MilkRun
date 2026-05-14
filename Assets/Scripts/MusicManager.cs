using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    #region Variables
    public static MusicManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource backgroundSource;
    [SerializeField] private AudioSource chaseSource;

    [Header("Settings")]
    [SerializeField] private float fadeSpeed = 1.5f;

    [Header("Proximity Settings")]
    [SerializeField] private float dangerDistance = 5f;   // Full intensity at this distance
    [SerializeField] private float safeDistance = 20f;    // Base intensity at this distance
    [SerializeField] private float basePitch = 1f;        // Normal chase music pitch
    [SerializeField] private float maxPitch = 1.4f;       // Pitch when bear is very close
    [SerializeField] private float baseVolume = 0.6f;     // Normal chase music volume
    [SerializeField] private float maxVolume = 1f;        // Volume when bear is very close
    [SerializeField] private float proximityFadeSpeed = 2f; // How fast pitch/volume shifts

    private HashSet<GameObject> _activeChasers = new HashSet<GameObject>();

    // Stores the last reported distance per chaser
    private Dictionary<GameObject, float> _chaserDistances = new Dictionary<GameObject, float>();

    private Coroutine _fadeCoroutine;
    private bool _isQuitting = false;
    private bool _isStopped = false;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
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
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            _isStopped = false;

            if (backgroundSource != null)
            {
                backgroundSource.volume = 1f;
                backgroundSource.Play();
            }
            if (chaseSource != null)
            {
                chaseSource.volume = 0f;
                chaseSource.Stop();
            }
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        // Only adjust proximity effects while chase music is active
        if (_activeChasers.Count == 0 || _isStopped || chaseSource == null) return;

        // Find the closest bear among all active chasers
        float closestDistance = float.MaxValue;
        foreach (var kvp in _chaserDistances)
        {
            if (kvp.Value < closestDistance)
                closestDistance = kvp.Value;
        }

        // Mathf.InverseLerp converts the distance into a 0-1 value
        // When distance = safeDistance → t = 0 (base intensity)
        // When distance = dangerDistance → t = 1 (max intensity)
        // Mathf.Clamp01 ensures it never goes below 0 or above 1
        float t = Mathf.Clamp01(Mathf.InverseLerp(safeDistance, dangerDistance, closestDistance));

        // Smoothly shift pitch and volume toward the target values
        float targetPitch = Mathf.Lerp(basePitch, maxPitch, t);
        float targetVolume = Mathf.Lerp(baseVolume, maxVolume, t);

        chaseSource.pitch = Mathf.Lerp(chaseSource.pitch, targetPitch, Time.deltaTime * proximityFadeSpeed);
        chaseSource.volume = Mathf.Lerp(chaseSource.volume, targetVolume, Time.deltaTime * proximityFadeSpeed);
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
        StopAllCoroutines();
    }
    #endregion

    #region Public Methods
    public void RegisterChaser(GameObject enemy)
    {
        if (_isQuitting || _isStopped) return;
        if (_activeChasers.Count == 0)
        {
            TransitionTo(chaseSource, backgroundSource);
        }
        _activeChasers.Add(enemy);
        _chaserDistances[enemy] = safeDistance; // Start at safe distance until first report
    }

    public void UnregisterChaser(GameObject enemy)
    {
        if (_isQuitting || _isStopped) return;
        _activeChasers.Remove(enemy);
        _chaserDistances.Remove(enemy);

        if (_activeChasers.Count == 0)
        {
            // Reset pitch back to normal when no chasers remain
            chaseSource.pitch = basePitch;
            TransitionTo(backgroundSource, chaseSource);
        }
    }

    public void ReportChaserDistance(GameObject enemy, float distance)
    {
        if (_isStopped) return;
        _chaserDistances[enemy] = distance;
    }

    public void StopAll()
    {
        _isStopped = true;

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _activeChasers.Clear();
        _chaserDistances.Clear();

        if (backgroundSource != null)
        {
            backgroundSource.volume = 0f;
            backgroundSource.Stop();
        }
        if (chaseSource != null)
        {
            chaseSource.pitch = basePitch; // Reset pitch before stopping
            chaseSource.volume = 0f;
            chaseSource.Stop();
        }
    }
    #endregion

    #region Private Methods
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0) return;

        _isStopped = false;
        _activeChasers.Clear();
        _chaserDistances.Clear();

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

        if (chaseSource != null)
        {
            chaseSource.pitch = basePitch; // Always reset pitch on new scene
            chaseSource.volume = 0f;
            chaseSource.Stop();
        }
        if (backgroundSource != null)
        {
            backgroundSource.volume = 1f;
            backgroundSource.Play();
        }
    }

    private void TransitionTo(AudioSource fadeIn, AudioSource fadeOut)
    {
        if (fadeIn == null || fadeOut == null) return;
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeTrack(fadeOut, fadeIn));
    }
    #endregion

    #region Coroutines
    private IEnumerator FadeTrack(AudioSource fadeOut, AudioSource fadeIn)
    {
        if (fadeOut == null || fadeIn == null) yield break;
        if (!fadeIn.isPlaying) fadeIn.Play();

        float t = 0;
        float startVolumeOut = fadeOut.volume;
        float startVolumeIn = fadeIn.volume;

        while (t < 1f)
        {
            if (_isQuitting || fadeOut == null || fadeIn == null) yield break;
            t += Time.deltaTime * fadeSpeed;
            fadeOut.volume = Mathf.Lerp(startVolumeOut, 0, t);
            fadeIn.volume = Mathf.Lerp(startVolumeIn, 1, t);
            yield return null;
        }

        if (fadeOut != null)
        {
            fadeOut.volume = 0f;
            fadeOut.Stop();
        }
        if (fadeIn != null) fadeIn.volume = 1f;
    }
    #endregion
}
