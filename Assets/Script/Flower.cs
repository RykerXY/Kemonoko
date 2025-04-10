using UnityEngine;

public class Flower : MonoBehaviour
{
    public float TimeToDestroy = 2.5f;
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GlobalPoint.instance.flowerPoints += 1;

            Destroy(gameObject);
        }
    }

    void Update()
    {
        TimeToDestroy -= Time.deltaTime;
        if (TimeToDestroy <= 0)
        {
            Destroy(gameObject);
        }
    }
}
