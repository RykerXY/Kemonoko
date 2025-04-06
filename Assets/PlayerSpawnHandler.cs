using UnityEngine;

public class PlayerSpawnHandler : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"{this.GetType().Name} Start: Player position is now {transform.position}");
    }
    private void Awake()
    {
        Debug.Log($"PlayerSpawnHandler Awake STARTING. Current position: {transform.position}"); // Log start pos

        if (PlayerPrefs.GetInt(Checkpoint.CheckpointSavedKey, 0) == 1)
        {
            float x = PlayerPrefs.GetFloat(Checkpoint.CheckpointXKey);
            float y = PlayerPrefs.GetFloat(Checkpoint.CheckpointYKey);
            float z = PlayerPrefs.GetFloat(Checkpoint.CheckpointZKey);
            Vector3 loadedPosition = new Vector3(x, y, z);

            Debug.Log($"PlayerSpawnHandler Awake: Found saved checkpoint at {loadedPosition}. Applying now."); // Log loaded pos

            // --- Crucial: Set the Player's position ---
            transform.position = loadedPosition;
            // -----------------------------------------

            Debug.Log($"PlayerSpawnHandler Awake FINISHED. Position AFTER setting: {transform.position}"); // Log final pos
        }
        else
        {
            Debug.Log("PlayerSpawnHandler Awake: No checkpoint data found. Position remains: " + transform.position);
        }
    }

    // Example: Add a way to clear the checkpoint for testing
    [ContextMenu("Clear Saved Checkpoint")]
    private void ClearCheckpointData()
    {
        PlayerPrefs.DeleteKey(Checkpoint.CheckpointSavedKey);
        PlayerPrefs.DeleteKey(Checkpoint.CheckpointXKey);
        PlayerPrefs.DeleteKey(Checkpoint.CheckpointYKey);
        PlayerPrefs.DeleteKey(Checkpoint.CheckpointZKey);
        PlayerPrefs.Save();
        Debug.Log("Cleared saved checkpoint data from PlayerPrefs.");
    }
}