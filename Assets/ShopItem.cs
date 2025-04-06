using UnityEngine;

public class ShopItem : MonoBehaviour
{
    public int heal1 = 1;
    public int healMax = 10;
    public PlayerHealth playerHealth;
    public GameObject ShopPanel;

    void Start()
    {
        ShopPanel.SetActive(false);
    }
    public void BuyHeal1()
    {
        if (playerHealth != null)
        {
            playerHealth.Heal(heal1);
            Debug.Log("Bought Heal 1");
        }
    }
    public void BuyHealMax()
    {
        if (playerHealth != null)
        {
            playerHealth.Heal(healMax);
            Debug.Log("Bought Heal Max");
        }
    }

    public void OpenShop()
    {
        ShopPanel.SetActive(true);
        Debug.Log("Open Shop");
    }
    public void CloseShop()
    {
        ShopPanel.SetActive(false);
        Debug.Log("Close Shop");
    }
}
