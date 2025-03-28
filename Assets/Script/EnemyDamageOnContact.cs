using UnityEngine;

[RequireComponent(typeof(Collider2D))] // ควรมี Collider เพื่อตรวจจับการชน
public class EnemyDamageOnContact : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("จำนวน Damage ที่จะทำเมื่อชน Player")]
    public int damageAmount = 10;

    [Tooltip("ระยะเวลาหน่วง (วินาที) ก่อนที่จะทำ Damage ได้อีกครั้งหลังชน")]
    public float damageCooldown = 0.5f; // ป้องกันการทำ Damage รัวๆ ถ้าชนค้าง

    private float lastDamageTime = -Mathf.Infinity; // เวลาล่าสุดที่ทำ Damage (ตั้งเป็นค่าต่ำๆ เพื่อให้ชนครั้งแรกทำ Damage ได้เลย)

    /// <summary>
    /// ฟังก์ชันนี้จะถูกเรียกโดยอัตโนมัติเมื่อ Collider ของ GameObject นี้
    /// เริ่มสัมผัสกับ Collider อื่น (ที่ไม่ได้เป็น Trigger)
    /// </summary>
    /// <param name="collision">ข้อมูลเกี่ยวกับการชนที่เกิดขึ้น</param>
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. ตรวจสอบว่า GameObject ที่ชนมี Tag "Player" หรือไม่
        if (collision.gameObject.CompareTag("Player"))
        {
            // 2. ตรวจสอบว่าผ่านช่วง Cooldown หรือยัง
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                // 3. พยายามดึง Component PlayerHealth จาก GameObject ที่ชน
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

                // 4. ตรวจสอบว่าเจอ PlayerHealth และ Player ยังไม่ตาย
                if (playerHealth != null && !playerHealth.IsDead)
                {
                    // 5. ทำ Damage ให้กับ Player
                    Debug.Log($"{gameObject.name} hit {collision.gameObject.name} for {damageAmount} damage.");
                    playerHealth.TakeDamage(damageAmount);

                    // 6. อัปเดตเวลาล่าสุดที่ทำ Damage
                    lastDamageTime = Time.time;

                    // (Optional) เพิ่ม Effect อื่นๆ ตรงนี้ได้ เช่น
                    // - เล่นเสียง Enemy โจมตี
                    // - อาจจะให้ Enemy กระเด็นถอยหลังเล็กน้อย (ถ้ามี Rigidbody)
                    // Rigidbody2D enemyRb = GetComponent<Rigidbody2D>();
                    // if (enemyRb != null)
                    // {
                    //     Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
                    //     enemyRb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse); // ปรับค่าแรงกระแทกตามต้องการ
                    // }
                }
            }
            // else { Debug.Log("Damage on cooldown."); } // สำหรับ Debug
        }
    }

    // --- (Optional) พิจารณาใช้ OnTriggerEnter2D แทน ---
    
    // ถ้าคุณต้องการให้การชนไม่ส่งผลทางฟิสิกส์ (เช่น ไม่มีการดันกัน)
    // คุณสามารถตั้งค่า Collider ทั้งของ Player และ Enemy ให้ Is Trigger = true
    // แล้วใช้ OnTriggerEnter2D แทน OnCollisionEnter2D

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
    
    // -----------------------------------------------------
}