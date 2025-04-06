using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float baseSpeed = 5f;
    public float sprintSpeedMultiplier = 1.5f; // Renamed for clarity, multiplier for sprint
    public float buffSpeedMultiplier = 2f; // Multiplier for the buff item
    public float buffDuration = 5f; // Duration of the buff item
    public bool canMove = true;

    private float currentSpeed; // Use this for movement calculation
    private Rigidbody2D rb;
    private Vector2 movement;
    private Coroutine speedBuffCoroutine;
    private bool isSprinting = false;
    private bool isBuffActive = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = baseSpeed; // Initialize current speed
    }

    void FixedUpdate() // Use FixedUpdate for physics manipulations
    {
        if (canMove)
        {
            rb.linearVelocity = movement * currentSpeed; // Use velocity for smoother physics movement
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Attack performed!");
            // Add Attack logic here
        }
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isSprinting = true;
        }
        else if (context.canceled)
        {
            isSprinting = false;
        }
        UpdateSpeed(); // Update speed immediately when sprint state changes
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("SpeedBuff"))
        {
            if (speedBuffCoroutine != null)
            {
                StopCoroutine(speedBuffCoroutine);
            }
            speedBuffCoroutine = StartCoroutine(SpeedBuffRoutine(buffSpeedMultiplier, buffDuration));
            Destroy(other.gameObject);
        }
    }

    private void UpdateSpeed()
    {
        if (isBuffActive)
        {
            // Buff takes priority and uses its own multiplier
            currentSpeed = baseSpeed * buffSpeedMultiplier;
            // Optional: Decide if sprint should stack with buff
            if (isSprinting) currentSpeed *= sprintSpeedMultiplier; // Uncomment to stack
        }
        else if (isSprinting)
        {
            currentSpeed = baseSpeed * sprintSpeedMultiplier;
        }
        else
        {
            currentSpeed = baseSpeed;
        }
        // Debug.Log($"Speed updated to: {currentSpeed}"); // Optional debug
    }

    private IEnumerator SpeedBuffRoutine(float multiplier, float duration)
    {
        isBuffActive = true;
        // buffSpeedMultiplier = multiplier; // Update if multiplier can change per buff item
        UpdateSpeed(); // Apply buff speed immediately
        Debug.Log($"Speed buff activated! New speed: {currentSpeed}");

        yield return new WaitForSeconds(duration);

        isBuffActive = false;
        speedBuffCoroutine = null;
        UpdateSpeed(); // Recalculate speed based on current state (sprinting or not)
        Debug.Log($"Speed buff deactivated! Speed is now: {currentSpeed}");
    }
}