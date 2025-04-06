using UnityEngine;

public class Flower : MonoBehaviour
{
    public float TimeToDestroy = 2.5f;
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Add karma points to the player
            GlobalPoint globalPoint = Object.FindFirstObjectByType<GlobalPoint>();
            if (globalPoint != null)
            {
                globalPoint.flowerPoints += 1;
                Debug.Log("Flower points: " + globalPoint.flowerPoints);
            }

            // Destroy the flower after collecting it
            Destroy(gameObject);
        }
    }

    //Destroy this flower if x seconds have passed
    void Update()
    {
        TimeToDestroy -= Time.deltaTime;
        if (TimeToDestroy <= 0)
        {
            Destroy(gameObject);
        }
    }
}
