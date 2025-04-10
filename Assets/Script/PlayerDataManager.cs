using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    public Vector3 lastPlayerPosition;
    public bool hasStoredPosition = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void StorePlayerPosition(Vector3 position)
    {
        lastPlayerPosition = position;
        hasStoredPosition = true;
        Debug.Log("Player position stored: " + lastPlayerPosition);
    }

    public void ClearStoredPosition()
    {
        hasStoredPosition = false;
        Debug.Log("Stored player position cleared.");
    }
}