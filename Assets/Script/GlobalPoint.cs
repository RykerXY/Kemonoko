using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GlobalPoint : MonoBehaviour
{
    public int karmaPoints = 0;
    public int flowerPoints = 0;
    public TextMeshProUGUI flowerPointsText;

    void Update()
    {
        if(flowerPoints >= 60)
        {
            karmaPoints += 1;
            flowerPoints = 0; // Reset flower points after adding to karma
            Debug.Log("Karma points: " + karmaPoints);
        }
        if (flowerPointsText != null)
        {
            flowerPointsText.text = "Flower: " + flowerPoints.ToString();
        }
    }
}
