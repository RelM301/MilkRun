using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class StaminaUI : MonoBehaviour
{
    #region Variables
    [Header("Stamina References")]
    [SerializeField] private Image fillImage;
    [SerializeField] private PlayerStamina playerStamina;

    [Header("Stamina Settings")]
    [SerializeField] private float blinkSpeed = 25f;

    [Header("Timer References")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameManager gameManager;

    [Header("Timer Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float normalFontSize = 36f;
    [SerializeField] private float heartbeatPeakSize = 52f;

    private Color _originalStaminaColor;

    // Tracks which whole second we last beat on, to sync pulse to the second
    private int _lastBeatSecond = -1;

    // Controls the pulse animation over time
    private float _pulseTimer = 0f;
    private bool _isPulsing = false;

    // How long each pulse animation lasts
    private const float PulseDuration = 0.35f;
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        if (fillImage != null)
            _originalStaminaColor = fillImage.color;

        if (timerText != null)
        {
            timerText.color = normalColor;
            timerText.fontSize = normalFontSize;
        }
    }

    private void OnEnable()
    {
        if (playerStamina != null)
            playerStamina.OnStaminaChanged += UpdateFill;
    }

    private void OnDisable()
    {
        if (playerStamina != null)
            playerStamina.OnStaminaChanged -= UpdateFill;
    }

    private void Update()
    {
        if (playerStamina == null || fillImage == null) return;
        HandleExhaustionVisuals();
        HandleTimerUI();
    }
    #endregion

    #region Private Methods
    private void HandleExhaustionVisuals()
    {
        if (playerStamina.IsExhausted)
        {
            float alpha = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f;
            fillImage.color = new Color(_originalStaminaColor.r, _originalStaminaColor.g, _originalStaminaColor.b, alpha);
        }
        else
        {
            if (fillImage.color != _originalStaminaColor)
                fillImage.color = _originalStaminaColor;
        }
    }

    private void HandleTimerUI()
    {
        if (timerText == null || gameManager == null) return;

        float timeRemaining = gameManager.TimeRemaining;

        // Format as MM:SS
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // Check if we are in warning territory
        bool inWarning = timeRemaining <= gameManager.WarningThreshold
                         && !gameManager.GameEnded;

        if (inWarning)
        {
            timerText.color = warningColor;

            // Detect the moment each new second ticks over
            int currentSecond = Mathf.CeilToInt(timeRemaining);
            if (currentSecond != _lastBeatSecond)
            {
                _lastBeatSecond = currentSecond;
                _pulseTimer = 0f;   // restart the pulse animation
                _isPulsing = true;
            }

            // Run the pulse animation
            if (_isPulsing)
            {
                _pulseTimer += Time.deltaTime;
                float progress = _pulseTimer / PulseDuration;

                if (progress >= 1f)
                {
                    // Pulse finished — snap back to normal size and stop
                    timerText.fontSize = normalFontSize;
                    _isPulsing = false;
                }
                else
                {
                    // Sine curve: starts at 0, peaks in the middle, returns to 0
                    // Mathf.Sin(progress * Mathf.PI) gives a smooth 0→1→0 arc
                    float sizeBoost = Mathf.Sin(progress * Mathf.PI);
                    timerText.fontSize = normalFontSize + (heartbeatPeakSize - normalFontSize) * sizeBoost;
                }
            }
        }
        else
        {
            // Outside warning — make sure everything is reset cleanly
            timerText.color = normalColor;
            timerText.fontSize = normalFontSize;
            _isPulsing = false;
            _lastBeatSecond = -1;
        }
    }

    private void UpdateFill(float percent)
    {
        if (fillImage != null)
            fillImage.fillAmount = percent;
    }
    #endregion
}
