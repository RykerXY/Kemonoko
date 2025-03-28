using UnityEngine;
using System.Collections; // เพิ่มเข้ามาเพื่อใช้ Coroutine (ถ้าต้องการหน่วงเวลา)

[RequireComponent(typeof(Rigidbody2D))]
public class PatrolEnemy : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform pointA; // ลาก Empty GameObject จุด A มาใส่
    public Transform pointB; // ลาก Empty GameObject จุด B มาใส่
    public float moveSpeed = 2f;
    public float waitTimeAtPoint = 1f; // (Optional) เวลาหยุดรอที่แต่ละจุด (วินาที)
    public float stoppingDistance = 0.1f; // ระยะห่างจากจุดหมายที่จะถือว่าถึงแล้ว

    [Header("Visuals")]
    public bool faceMovementDirection = true; // ตั้งเป็น false ถ้าไม่ต้องการให้หัน Sprite

    private Rigidbody2D rb;
    private Transform currentTarget;
    private bool isWaiting = false; // สถานะว่ากำลังรออยู่ที่จุดหมายหรือไม่
    private bool isFacingRight = true; // สถานะการหันหน้า (True = ขวา, False = ซ้าย) - **ปรับค่านี้ถ้า Sprite เริ่มต้นหันซ้าย**

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // --- Basic Rigidbody Setup for Top-Down ---
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            Debug.LogError("Rigidbody2D not found!", gameObject);
            enabled = false;
            return;
        }
        // --- End Rigidbody Setup ---

        // --- Patrol Points Check ---
        if (pointA == null || pointB == null)
        {
            Debug.LogError("Patrol points (Point A or Point B) are not assigned!", gameObject);
            enabled = false;
            return;
        }
        // --- End Patrol Points Check ---

        // เริ่มต้นให้ไปที่ Point B ก่อน (หรือ Point A ก็ได้)
        currentTarget = pointB;
        transform.position = pointA.position; // เริ่มที่ Point A

        // ตั้งค่าการหันหน้าครั้งแรกให้ถูกต้อง
        if (faceMovementDirection)
        {
            // เช็คว่าเป้าหมายแรก (B) อยู่ทางขวาหรือซ้ายของจุดเริ่มต้น (A)
            if ((pointB.position.x < pointA.position.x && isFacingRight) || (pointB.position.x > pointA.position.x && !isFacingRight))
            {
                FlipSprite(); // พลิกถ้าทิศทางเริ่มต้นไม่ตรงกับเป้าหมายแรก
            }
        }
    }

    void FixedUpdate()
    {
        if (isWaiting || currentTarget == null)
        {
            // ถ้ากำลังรอ หรือไม่มีเป้าหมาย ให้หยุดนิ่ง
             if(rb.linearVelocity != Vector2.zero) rb.linearVelocity = Vector2.zero;
            return;
        }

        // คำนวณระยะห่างไปยังเป้าหมายปัจจุบัน
        float distanceToTarget = Vector2.Distance(rb.position, currentTarget.position);

        // ตรวจสอบว่าถึงเป้าหมายหรือยัง (อยู่ในระยะ stoppingDistance)
        if (distanceToTarget <= stoppingDistance)
        {
            // ถึงแล้ว: หยุดเคลื่อนที่และเริ่มรอ (ถ้าตั้งค่า waitTime > 0)
            rb.linearVelocity = Vector2.zero;
            if (waitTimeAtPoint > 0)
            {
                StartCoroutine(WaitAtPoint());
            }
            else
            {
                // ถ้าไม่ต้องการรอ ก็สลับเป้าหมายทันที
                SwitchTarget();
            }
        }
        else
        {
            // ยังไม่ถึง: เคลื่อนที่ไปยังเป้าหมาย
            MoveTowardsTarget();
        }
    }

    void MoveTowardsTarget()
    {
        if (currentTarget == null) return;

        // คำนวณทิศทาง
        Vector2 direction = ((Vector2)currentTarget.position - rb.position).normalized;

        // ตั้งค่า Velocity
        rb.linearVelocity = direction * moveSpeed;

        // หัน Sprite (ถ้าเปิดใช้งาน)
        if (faceMovementDirection)
        {
            // เช็คทิศทางการเคลื่อนที่แนวนอน (rb.velocity.x)
            if (rb.linearVelocity.x > 0.01f && !isFacingRight) // กำลังไปทางขวา แต่หน้าหันซ้ายอยู่
            {
                FlipSprite();
            }
            else if (rb.linearVelocity.x < -0.01f && isFacingRight) // กำลังไปทางซ้าย แต่หน้าหันขวาอยู่
            {
                FlipSprite();
            }
        }
    }

    void SwitchTarget()
    {
        // สลับเป้าหมาย: ถ้าปัจจุบันคือ A ให้ไป B, ถ้าปัจจุบันคือ B ให้ไป A
        currentTarget = (currentTarget == pointA) ? pointB : pointA;

        // เมื่อเปลี่ยนเป้าหมาย ให้เช็คและหันหน้าทันที (เผื่อกรณีรอแล้วต้องหันกลับ)
         if (faceMovementDirection && !isWaiting) // เช็ค isWaiting ด้วย ป้องกันการ flip ตอนยังรออยู่
        {
            // เช็คว่าเป้าหมายใหม่อยู่ทางขวาหรือซ้าย
            if ((currentTarget.position.x < transform.position.x && isFacingRight) || (currentTarget.position.x > transform.position.x && !isFacingRight))
            {
                FlipSprite();
            }
        }
    }

    void FlipSprite()
    {
        isFacingRight = !isFacingRight; // สลับสถานะการหันหน้า
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1; // พลิกแกน X
        transform.localScale = currentScale;
    }

    // Coroutine สำหรับการรอที่จุดหมาย (Optional)
    IEnumerator WaitAtPoint()
    {
        isWaiting = true; // ตั้งสถานะว่ากำลังรอ
        yield return new WaitForSeconds(waitTimeAtPoint); // รอตามเวลาที่กำหนด
        isWaiting = false; // เลิกรอ
        SwitchTarget(); // สลับเป้าหมายหลังจากรอเสร็จ
    }
}