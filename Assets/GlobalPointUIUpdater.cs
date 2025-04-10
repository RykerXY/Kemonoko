using UnityEngine;
using TMPro;

public class GlobalPointUIUpdater : MonoBehaviour
{
    [Header("UI Element References (Assign in Inspector for THIS scene)")]
    public TextMeshProUGUI karmaPointsText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI flowerPointsText;
    public TextMeshProUGUI fireflyPointsText;
    public TextMeshProUGUI orePointsText;
    public TextMeshProUGUI veggiePointsText;


    void Start()
    {
        UpdateDisplay();
    }

    void Update()
    {
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (GlobalPoint.instance == null)
        {
            return;
        }

        if (karmaPointsText != null)
        {
            karmaPointsText.text = "Karma: " + GlobalPoint.instance.karmaPoints;
        }
        if (goldText != null)
        {
            goldText.text = "Gold: " + GlobalPoint.instance.gold;
        }
        if (flowerPointsText != null)
        {
            flowerPointsText.text = "Flower: " + GlobalPoint.instance.flowerPoints;
        }
        if (fireflyPointsText != null)
        {
            // <<< Bug Fix: ตรงนี้ควรใช้ GlobalPoint.instance.fireflyPoints
            fireflyPointsText.text = "Firefly: " + GlobalPoint.instance.fireflyPoints;
        }
        if (orePointsText != null)
        {
            orePointsText.text = "Ore: " + GlobalPoint.instance.orePoints;
        }
        if (veggiePointsText != null)
        {
            veggiePointsText.text = "Veggie: " + GlobalPoint.instance.veggiePoints;
        }
    }
}