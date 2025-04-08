using TMPro;
using UnityEngine;

public class GlobalPoint : MonoBehaviour
{
    public static GlobalPoint instance { get; private set; }

    [Header("Points")]
    public int karmaPoints = 0;
    public int flowerPoints = 0;
    public int fireflyPoints = 0;
    public int orePoints = 0;
    public int veggiePoints = 0;

    [Header("UI Elements")]
    public TextMeshProUGUI flowerPointsText;
    public TextMeshProUGUI fireflyPointsText;
    public TextMeshProUGUI orePointsText;
    public TextMeshProUGUI veggiePointsText;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GlobalPoint Instance Created and set to DontDestroyOnLoad.");
        }
        else if (instance != this)
        {
            Debug.LogWarning("Another GlobalPoint instance detected. Destroying this duplicate.");
            Destroy(gameObject);
        }
    }
    void Update()
    {
        UpdateKarma();
        UpdateDisplay();
    }
    
    void UpdateKarma()
    {
        if (flowerPoints >= 60)
        {
            karmaPoints += 1;
            flowerPoints = 0;
            Debug.Log("Karma points: " + karmaPoints);
        }
        if (flowerPoints >= 60)
        {
            karmaPoints += 1;
            flowerPoints = 0;
            Debug.Log("Firefly points: " + flowerPoints);
        }
        if (orePoints >= 4)
        {
            karmaPoints += 1;
            orePoints = 0;
            Debug.Log("Ore points: " + karmaPoints);
        }
        if (veggiePoints > 20)
        {
            karmaPoints += 1;
            veggiePoints = 0;
            Debug.Log("Veggie points: " + veggiePoints);
        }
    }
    void UpdateDisplay()
    {
        if (flowerPointsText != null)
        {
            flowerPointsText.text = "Flower Points: " + flowerPoints;
        }
        if (fireflyPointsText != null)
        {
            fireflyPointsText.text = "Firefly: " + flowerPoints;
        }
        if (orePointsText != null)
        {
            orePointsText.text = "Ore Collected: " + orePoints;
        }
        if (veggiePointsText != null)
        {
            veggiePointsText.text = "Veggie Points: " + veggiePoints;
        }
    }
}
