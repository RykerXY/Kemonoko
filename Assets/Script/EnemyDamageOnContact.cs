using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyDamageOnContact : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("จำนวน Damage ที่จะทำเมื่อชน Player")]
    public int damageAmount = 10;

    [Tooltip("ระยะเวลาหน่วง (วินาที) ก่อนที่จะทำ Damage ได้อีกครั้งหลังชน")]
    public float damageCooldown = 0.5f;

    private float lastDamageTime = -Mathf.Infinity;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

                if (playerHealth != null && !playerHealth.IsDead)
                {
                    Debug.Log($"{gameObject.name} hit {collision.gameObject.name} for {damageAmount} damage.");
                    playerHealth.TakeDamage(damageAmount);

                    lastDamageTime = Time.time;
                }
            }
            else { Debug.Log("Damage on cooldown."); }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
             if (Time.time >= lastDamageTime + damageCooldown)
             {
                 PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                 if (playerHealth != null && !playerHealth.IsDead)
                 {
                     playerHealth.TakeDamage(damageAmount);
                     lastDamageTime = Time.time;
                     Debug.Log($"{gameObject.name} triggered damage on {other.name}");
                 }
             }
        }
    }
}