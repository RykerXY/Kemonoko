using UnityEngine;
using UnityEngine.UI;

public class HealthIconUpdate : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Image[] healthIcon;
    private int health;

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
