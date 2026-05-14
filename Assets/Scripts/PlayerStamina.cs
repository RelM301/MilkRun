using System;
using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    #region Variables
    public event Action<float> OnStaminaChanged;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float regenRate = 7f;
    [SerializeField] private float burnRate = 35f;

    [Header("Exhaustion Settings")]
    [SerializeField] private float minExhaustionDuration = 1.5f;

    private float _currentStamina;
    private bool _isExhausted = false;
    private float _exhaustionTimer = 0f;
    private bool _isBurningThisStep = false; // Set by movement, consumed here
    private bool _forcedExhaustionPending = false;

    public bool CanSprint => !_isExhausted && _currentStamina > 1f;
    public bool IsExhausted => _isExhausted;
    #endregion

    #region Unity Callbacks
    private void Awake() => _currentStamina = maxStamina;
    private void Start() => NotifyChange();

    // Everything now runs in FixedUpdate — same clock as movement
    private void FixedUpdate()
    {
        if (_isBurningThisStep)
        {
            Burn();
        }
        else
        {
            Regen();
        }

        // TEMPORARY DEBUG - remove after diagnosis
        Debug.Log($"[Stamina] Stamina={_currentStamina:F2} | Exhausted={_isExhausted} | Timer={_exhaustionTimer:F2} | Burning={_isBurningThisStep} | BurnRate={burnRate} | RegenRate={regenRate} | MinExhaustion={minExhaustionDuration}");

        _isBurningThisStep = false;
    }
    #endregion

    #region Public Methods
    // Movement calls this to signal intent — actual burn happens in FixedUpdate
    public void RequestBurn()
    {
        _isBurningThisStep = true;
    }
    #endregion

    #region Private Methods
    private void Burn()
    {
        _currentStamina -= burnRate * Time.fixedDeltaTime;

        // If stamina is critically low, flag it — don't let a regen frame save it
        if (_currentStamina <= 1.5f)
        {
            _forcedExhaustionPending = true;
        }

        if (_currentStamina <= 0.1f)
        {
            _currentStamina = 0f;
            if (!_isExhausted)
            {
                _isExhausted = true;
                _exhaustionTimer = 0f;
            }
            _forcedExhaustionPending = false;
        }

        NotifyChange();
    }

    private void Regen()
    {
        // If we were burning hard and stamina is critically low, 
        // don't allow regen to rescue it — push to exhaustion instead
        if (_forcedExhaustionPending)
        {
            _currentStamina -= burnRate * Time.fixedDeltaTime;
            if (_currentStamina <= 0.1f)
            {
                _currentStamina = 0f;
                if (!_isExhausted)
                {
                    _isExhausted = true;
                    _exhaustionTimer = 0f;
                }
                _forcedExhaustionPending = false;
            }
            NotifyChange();
            return; // Skip normal regen entirely
        }

        if (_currentStamina >= maxStamina) return;

        if (_isExhausted)
            _exhaustionTimer += Time.fixedDeltaTime;

        _currentStamina += regenRate * Time.fixedDeltaTime;
        _currentStamina = Mathf.Min(_currentStamina, maxStamina);

        if (_isExhausted
            && _currentStamina >= maxStamina
            && _exhaustionTimer >= minExhaustionDuration)
        {
            _isExhausted = false;
            _exhaustionTimer = 0f;
        }

        NotifyChange();
    }

    private void NotifyChange() => OnStaminaChanged?.Invoke(_currentStamina / maxStamina);
    #endregion
}
