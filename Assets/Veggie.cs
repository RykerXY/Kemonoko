using UnityEngine;

public class Veggie : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GlobalPoint globalPoint = Object.FindFirstObjectByType<GlobalPoint>();
            if (globalPoint != null)
            {
                globalPoint.veggiePoints += 1;
                Debug.Log("Veggie points: " + globalPoint.veggiePoints);
            }

            Destroy(gameObject);
        }
    }
}
