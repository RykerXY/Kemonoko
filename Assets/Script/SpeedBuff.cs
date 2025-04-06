using UnityEngine;

public class SpeedBuff : MonoBehaviour
{
    // Apply speed for a short duration
    public float speedDuration = 1f; // Duration of the speed buff
    public float speedMultiplier = 1.5f; // Speed multiplier for the buff
    private float originalSpeed; // Original speed of the player
    private PlayerController playerController; // Reference to the PlayerController script
    private bool isBuffActive = false; // Flag to check if the buff is active
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // Get the SpriteRenderer component
        spriteRenderer.enabled = true; // Enable the sprite renderer to make the object visible
    }

    void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>(); // Find the PlayerController in the scene
        originalSpeed = playerController.baseSpeed; // Store the original speed
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isBuffActive)
        {
            isBuffActive = true; // Set the buff as active
            playerController.baseSpeed *= speedMultiplier; // Increase the player's speed
            spriteRenderer.enabled = false; // Disable the sprite renderer to hide the object

            Debug.Log("Speed buff activated!");

            // Start a coroutine to reset the speed after the duration
            StartCoroutine(ResetSpeedAfterDuration());
        }
    }
    private System.Collections.IEnumerator ResetSpeedAfterDuration()
    {
        yield return new WaitForSeconds(speedDuration); // Wait for the duration of the buff
        playerController.baseSpeed = originalSpeed; // Reset the player's speed to original
        isBuffActive = false; // Set the buff as inactive
        Debug.Log("Speed buff deactivated!");
    }
}
