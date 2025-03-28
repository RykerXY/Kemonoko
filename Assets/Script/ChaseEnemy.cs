using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] // บังคับว่าต้องมี Rigidbody2D อยู่ด้วย
public class ChaseEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2.5f; // ความเร็วในการเคลื่อนที่ของ AI (ปรับค่าได้ใน Inspector)

    [Header("Target")]
    public string playerTag = "Player"; // Tag ของ GameObject ผู้เล่น

    private Transform targetTransform; // ตำแหน่งของเป้าหมาย (ผู้เล่น)
    private Rigidbody2D rb;
    private bool canMove = false; // สถานะว่า AI สามารถเคลื่อนที่ได้หรือไม่ (เจอผู้เล่นแล้ว)

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // --- ตั้งค่า Rigidbody2D สำหรับ Top-down ---
        if (rb != null)
        {
            rb.gravityScale = 0; // ไม่มีแรงโน้มถ่วงในเกม Top-down
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // ไม่ต้องหมุนตัวแกน Z
            // rb.drag = 2f; // (Optional) เพิ่มแรงต้านทานเล็กน้อยเพื่อให้หยุดนิ่งขึ้นเมื่อไม่มี input (ถ้าต้องการ)
            // rb.angularDrag = 0.05f; // (Optional) ค่าเริ่มต้น
        }
        else
        {
            Debug.LogError("Rigidbody2D not found on this GameObject! AI cannot move.", gameObject);
            enabled = false; // ปิดการทำงานของ Script นี้ไปเลย
            return;
        }
        // --- จบการตั้งค่า Rigidbody2D ---


        // --- ค้นหาผู้เล่น ---
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null)
        {
            targetTransform = playerObject.transform;
            canMove = true; // เจอผู้เล่นแล้ว เริ่มเคลื่อนที่ได้
            Debug.Log("Player found! AI will start chasing.", gameObject);
        }
        else
        {
            Debug.LogWarning($"Player GameObject with tag '{playerTag}' not found! AI will not move.", gameObject);
            canMove = false; // หาผู้เล่นไม่เจอ ไม่ต้องเคลื่อนที่
        }
        // --- จบการค้นหาผู้เล่น ---
    }

    // ใช้ FixedUpdate เพราะเราทำงานกับระบบฟิสิกส์ (Rigidbody2D)
    // FixedUpdate จะทำงานเป็นช่วงเวลาคงที่ สอดคล้องกับการคำนวณฟิสิกส์
    void FixedUpdate()
    {
        // ตรวจสอบว่าสามารถเคลื่อนที่ได้หรือไม่ (เจอผู้เล่นแล้วหรือยัง)
        if (!canMove || targetTransform == null)
        {
            // ถ้าเคลื่อนที่ไม่ได้ หรือเป้าหมายหายไป (เช่น ผู้เล่นตาย) ให้หยุดนิ่ง
            if (rb.linearVelocity != Vector2.zero) // เช็คเพื่อลดการเรียก set velocity โดยไม่จำเป็น
            {
                 rb.linearVelocity = Vector2.zero;
            }
            return; // ออกจากการทำงานใน Frame นี้
        }

        // --- คำนวณทิศทางไปยังเป้าหมาย ---
        // 1. หาเวกเตอร์ชี้จากตำแหน่งปัจจุบัน (rb.position) ไปยังเป้าหมาย (targetTransform.position)
        //    Vector2 คือ (จุดหมาย - จุดเริ่มต้น)
        Vector2 directionToTarget = (Vector2)targetTransform.position - rb.position;

        // 2. ทำให้เวกเตอร์มีขนาดเท่ากับ 1 (Unit Vector) เพื่อเอาแค่ทิศทาง
        //    .normalized จะคืนค่าเวกเตอร์เดิมถ้ามันเป็น Vector2.zero อยู่แล้ว (ปลอดภัย)
        Vector2 moveDirection = directionToTarget.normalized;

        // --- เคลื่อนที่ AI ---
        // ตั้งค่าความเร็ว (velocity) ของ Rigidbody2D
        // velocity = ทิศทาง * ความเร็ว
        rb.linearVelocity = moveDirection * moveSpeed;
    }

     // (Optional) ถ้าต้องการให้ Sprite หันหน้าไปทางผู้เล่น (แบบง่ายๆ ไม่สมูท)
     
     void Update()
     {
         if (!canMove || targetTransform == null) return;

         Vector2 directionToTarget = (Vector2)targetTransform.position - (Vector2)transform.position;

         // คำนวณมุม (เป็นองศา) เทียบกับแกน x บวก
         // Atan2 จะให้ค่ามุมที่ถูกต้องในทุกควอดรันต์ (เป็นเรเดียน)
         // Mathf.Rad2Deg แปลงเรเดียนเป็นองศา
         float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

         // ตั้งค่าการหมุนของ Rigidbody (หรือ Transform ถ้าไม่ได้ใช้ rb.constraints)
         // ลบ 90 เพราะ Sprite ส่วนใหญ่มักจะหันหน้าขึ้น (แกน Y) ไม่ใช่ไปทางขวา (แกน X)
         // ถ้า Sprite ของคุณหันขวาอยู่แล้ว ไม่ต้องลบ 90
         rb.rotation = angle - 90f;
         // หรือใช้ transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
     }
     
}