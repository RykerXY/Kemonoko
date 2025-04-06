using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    // --- Public Constants for PlayerPrefs Keys ---
    // Made public so other scripts (like the player) can use the same keys.
    public const string CheckpointSavedKey = "CheckpointActive";
    public const string CheckpointXKey = "CheckpointX";
    public const string CheckpointYKey = "CheckpointY";
    public const string CheckpointZKey = "CheckpointZ";
    // -------------------------------------------

    [SerializeField] private bool showActivationIndicator = true;
    [SerializeField] private Color activeColor = Color.green;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    // --- No more static load method here ---

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true; // Ensure it's a trigger
        if (showActivationIndicator)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
            else if (showActivationIndicator)
            {
                // Optional: Warn if indicator is enabled but no renderer found
                Debug.LogWarning($"Checkpoint '{gameObject.name}' has Show Activation Indicator enabled, but no SpriteRenderer was found.", this);
                showActivationIndicator = false; // Disable it to prevent errors later
            }
        }
    }

    private void Start()
    {
        // Update visual state based on currently saved data when the scene starts
        UpdateVisualIndicator();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            // Check if this checkpoint is already the last saved one
            bool alreadyActive = false;
            if (PlayerPrefs.GetInt(CheckpointSavedKey, 0) == 1)
            {
                Vector3 currentSavedPos = new Vector3(
                    PlayerPrefs.GetFloat(CheckpointXKey),
                    PlayerPrefs.GetFloat(CheckpointYKey),
                    PlayerPrefs.GetFloat(CheckpointZKey)
                );
                // Use a small tolerance for floating-point comparisons
                if (Vector3.Distance(transform.position, currentSavedPos) < 0.01f)
                {
                    alreadyActive = true;
                }
            }

            if (!alreadyActive)
            {
                Debug.Log($"Checkpoint activated: {gameObject.name} at {transform.position}");

                // Save position to PlayerPrefs
                PlayerPrefs.SetFloat(CheckpointXKey, transform.position.x);
                PlayerPrefs.SetFloat(CheckpointYKey, transform.position.y);
                PlayerPrefs.SetFloat(CheckpointZKey, transform.position.z);
                PlayerPrefs.SetInt(CheckpointSavedKey, 1);
                PlayerPrefs.Save(); // Ensure data is written

                // Update visuals of all checkpoints
                UpdateAllCheckpointVisuals();
            }
            // Optional: Add an else block here if you want feedback even if it's already active
            // else { Debug.Log($"Checkpoint {gameObject.name} is already the active one."); }
        }
    }

    // Updates the visual indicator for *this* checkpoint instance
    private void UpdateVisualIndicator()
    {
        if (!showActivationIndicator || spriteRenderer == null) return;

        bool isActive = false;
        if (PlayerPrefs.GetInt(CheckpointSavedKey, 0) == 1)
        {
            Vector3 currentSavedPos = new Vector3(
                PlayerPrefs.GetFloat(CheckpointXKey),
                PlayerPrefs.GetFloat(CheckpointYKey),
                PlayerPrefs.GetFloat(CheckpointZKey)
            );
            if (Vector3.Distance(transform.position, currentSavedPos) < 0.01f)
            {
                isActive = true;
            }
        }

        spriteRenderer.color = isActive ? activeColor : originalColor;
    }

    // Finds all checkpoints and tells them to update their visuals
    // Made public static so it could potentially be called from elsewhere if needed,
    // but primarily used internally after activation.
    public static void UpdateAllCheckpointVisuals()
    {
        // FindObjectsByType is generally preferred over FindObjectsOfType
        Checkpoint[] allCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        foreach (Checkpoint cp in allCheckpoints)
        {
            cp.UpdateVisualIndicator(); // Call the instance method
        }
    }
}