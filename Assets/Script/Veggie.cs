using UnityEngine;

public class Veggie : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GlobalPoint.instance.veggiePoints += 1;
            Destroy(gameObject);
        }
    }
}
