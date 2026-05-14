using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Variables
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 7f;
    [SerializeField] private float sprintSpeed = 14f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float jumpForce = 5f;

    [Header("Animation Settings")]
    [SerializeField] private float sprintAnimMultiplier = 2.0f;

    private Rigidbody rb;
    private Animator animator;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool _isSprinting = false; // The Sprint toggle
    private bool _sprintButtonHeld = false; // Tracks if you're actually holding B
    private PlayerStamina _stamina;
    #endregion

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        _stamina = GetComponent<PlayerStamina>();
    }

    #region Unity Callbacks
    void FixedUpdate()
    {
        // 1. Sprint Logic: Must be holding button AND have stamina AND not be exhausted
        if (_sprintButtonHeld && _stamina != null && _stamina.CanSprint)
        {
            _isSprinting = true;
            _stamina.RequestBurn();
        }
        else
        {
            _isSprinting = false;
        }

        // 2. Movement
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0f; camRight.y = 0f;
        camForward.Normalize(); camRight.Normalize();

        Vector3 movement = (camForward * moveInput.y) + (camRight * moveInput.x);
        float movementValue = moveInput.magnitude;

        // Use speeds
        float currentSpeed = (_isSprinting && movementValue > 0.1f) ? sprintSpeed : walkSpeed;
        rb.linearVelocity = new Vector3(movement.x * currentSpeed, rb.linearVelocity.y, movement.z * currentSpeed);

        // 3. Animation speed matches current movement speed
        if (animator)
        {
            animator.SetFloat("Movement", movementValue);
            animator.speed = (_isSprinting && movementValue > 0.1f) ? sprintAnimMultiplier : 1.0f;
        }

        // 4. Rotation
        if (movementValue > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) isGrounded = true;
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) isGrounded = false;
    }
    #endregion

    #region Input Methods
    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();

    public void OnJump(InputValue value)
    {
        if (isGrounded) rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public void OnSprint(InputValue value)
    {
        // With Action Type "Value", this will be true on hold and false on release
        _sprintButtonHeld = value.isPressed;
    }
    #endregion

}
