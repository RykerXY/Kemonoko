using UnityEngine;
using UnityEngine.UI;

public class HealthIconUpdate : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Image[] healthIcon;
    private int health;
    private void Awake()
    {
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth == null)
            {
                Debug.LogError("PlayerHealth component not found in the scene. Please assign it in the inspector or ensure it exists in the scene.");
            }
        }
    }
    void Update()
    {
        health = playerHealth.CurrentHealth;
        UpdateIcon();
    }

    void UpdateIcon()
    {
        switch (health)
        {
            case 3:
                healthIcon[0].enabled = true;
                healthIcon[1].enabled = true;
                healthIcon[2].enabled = true;
                break;
            case 2:
                healthIcon[0].enabled = true;
                healthIcon[1].enabled = true;
                healthIcon[2].enabled = false;
                break;
            case 1:
                healthIcon[0].enabled = true;
                healthIcon[1].enabled = false;
                healthIcon[2].enabled = false;
                break;
            case 0:
                healthIcon[0].enabled = false;
                healthIcon[1].enabled = false;
                healthIcon[2].enabled = false;
                break;
        }
    }
}
