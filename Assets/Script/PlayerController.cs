using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))] // ตรวจสอบว่ามี Rigidbody2D
[RequireComponent(typeof(Animator))]  // ตรวจสอบว่ามี Animator
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float baseSpeed = 5f;
    public float sprintSpeedMultiplier = 1.5f;
    public float buffSpeedMultiplier = 2f;
    public float buffDuration = 5f;
    public bool canMove = true;

    [Header("Visual Effects")] // เพิ่ม Header ใหม่สำหรับเอฟเฟกต์
    [Tooltip("Prefab ของ Particle Effect ที่จะสร้างเมื่อเก็บ Speed Buff")]
    public GameObject speedBuffParticlePrefab; // << เพิ่มตัวแปรสำหรับ Prefab

    // Animator reference (ควรลากใส่ใน Inspector หรือ Get ใน Awake/Start)
    public Animator animator;

    // --- Internal State ---
    private float currentSpeed;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Coroutine speedBuffCoroutine;
    private bool isSprinting = false;
    private bool isBuffActive = false;
    private bool isFacingRight = true; // เพิ่มตัวแปรสำหรับติดตามทิศทาง

    void Awake() // ใช้ Awake เพื่อ Get Components
    {
        rb = GetComponent<Rigidbody2D>();
        // ลองหา Animator ถ้ายังไม่ได้ลากใส่ใน Inspector
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // ตรวจสอบว่าหาเจอหรือไม่
        if (rb == null) Debug.LogError("PlayerController: Rigidbody2D not found!", gameObject);
        if (animator == null) Debug.LogError("PlayerController: Animator component not found or not assigned!", gameObject);
    }

    void Start()
    {
        currentSpeed = baseSpeed; // Initialize current speed
        UpdateSpeed(); // อัปเดตความเร็วเริ่มต้น (เผื่อมีสถานะอื่น ๆ ในอนาคต)
    }

    void Update() // ใช้ Update สำหรับ Input และ Logic ที่ไม่เกี่ยวกับ Physics โดยตรง
    {
        HandleSpriteFlip(); // จัดการการกลับด้าน Sprite
        HandleAnimationParameters(); // จัดการ Parameters ของ Animator (แยกออกมาเพื่อความชัดเจน)
    }

    void FixedUpdate() // Use FixedUpdate for physics manipulations
    {
        MoveCharacter(); // จัดการการเคลื่อนที่ใน FixedUpdate
    }

    // --- Input Handling ---
    public void Move(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed && canMove && animator != null) // ตรวจสอบ canMove ด้วย
        {
            Debug.Log("Attack performed!");
            animator.SetTrigger("Attack"); // สมมติว่ามี Trigger ชื่อ "Attack" ใน Animator
            // Add Attack logic here
        }
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        if (!canMove) // ถ้าเคลื่อนที่ไม่ได้ ก็ไม่ต้องทำอะไรเกี่ยวกับการ Sprint
        {
            if (isSprinting) // ถ้ากำลัง Sprint อยู่ตอนเคลื่อนที่ไม่ได้ ให้หยุด Sprint
            {
                isSprinting = false;
                UpdateSpeed(); // อัปเดตความเร็ว
                if (animator != null) animator.SetBool("isSprinting", false); // อัปเดต Animator ด้วย
            }
            return;
        }

        if (context.performed)
        {
            isSprinting = true;
        }
        else if (context.canceled)
        {
            isSprinting = false;
        }
        UpdateSpeed(); // Update speed immediately when sprint state changes
        // การตั้งค่า Animator isSprinting จะอยู่ใน HandleAnimationParameters()
    }

    // --- Movement & Physics ---
    private void MoveCharacter()
    {
        if (canMove && rb != null)
        {
            // ใช้ normalized เพื่อให้ความเร็วคงที่ทุกทิศทาง และคูณด้วย currentSpeed
            rb.linearVelocity = movement.normalized * currentSpeed;
        }
        else if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // หยุดสนิทเมื่อ canMove เป็น false
        }
    }


    // --- Animation & Visuals ---

    private void HandleAnimationParameters()
    {
        if (animator == null) return; // ออกถ้าไม่มี Animator

        float moveMagnitude = 0f;
        if (canMove && rb != null)
        {
            // ใช้ velocity magnitude เพื่อให้ animation สัมพันธ์กับการเคลื่อนที่จริง
            moveMagnitude = rb.linearVelocity.magnitude;
            // หรือใช้ movement input ถ้าอยากให้ animation เล่นตามการกด แม้จะชนกำแพง
            // moveMagnitude = movement.magnitude;
        }

        // ตั้งค่า isMoving
        animator.SetBool("isMoving", moveMagnitude > 0.1f); // ใช้ค่า threshold เล็กน้อย

        // ตั้งค่า moveX, moveY (เมื่อมีการเคลื่อนที่)
        // ควรใช้ last non-zero input หรือ velocity direction เพื่อให้ทิศทางถูกต้องตอนหยุด
        // แต่วิธีนี้จะใช้ Input ปัจจุบัน ซึ่งง่ายกว่า
        if (moveMagnitude > 0.1f)
        {
            // Normalization สำคัญหากใช้ Input โดยตรง เพื่อให้ Blend Tree ทำงานถูกต้อง
            Vector2 animationDirection = movement.normalized;
            animator.SetFloat("moveX", animationDirection.x);
            animator.SetFloat("moveY", animationDirection.y);
        }
        // ไม่ต้อง Reset เป็น 0 เพราะ isMoving จะเป็น false ทำให้ Animator กลับไป Idle State

        // ตั้งค่า isSprinting (ขึ้นอยู่กับสถานะ isSprinting และมีการเคลื่อนที่)
        animator.SetBool("isSprinting", isSprinting && moveMagnitude > 0.1f);
    }


    private void HandleSpriteFlip()
    {
        if (canMove && Mathf.Abs(movement.x) > 0.01f) // ตรวจสอบว่ามี input แนวนอน
        {
            bool shouldFaceRight = movement.x > 0;
            if (isFacingRight != shouldFaceRight)
            {
                isFacingRight = shouldFaceRight;
                Vector3 theScale = transform.localScale;
                theScale.x *= -1;
                transform.localScale = theScale;
            }
        }
    }

    // --- Buff Handling & Effects ---
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("SpeedBuff"))
        {
            // --- เก็บตำแหน่งก่อนทำลาย Object ---
            Vector3 spawnPosition = other.transform.position; // << เก็บตำแหน่ง

            if (speedBuffCoroutine != null)
            {
                StopCoroutine(speedBuffCoroutine);
            }
            speedBuffCoroutine = StartCoroutine(SpeedBuffRoutine(buffSpeedMultiplier, buffDuration));

            // --- ทำลาย Item ---
            Destroy(other.gameObject);

            // --- สร้าง Particle Effect (หลังจากทำลาย Item) ---
            if (speedBuffParticlePrefab != null) // << ตรวจสอบว่า Prefab ถูกกำหนดค่าไว้หรือไม่
            {
                Instantiate(speedBuffParticlePrefab, spawnPosition, Quaternion.identity); // << สร้าง Prefab ณ ตำแหน่งที่เก็บ
                 // Optional: ถ้าต้องการให้ Particle หายไปเองหลังจากเล่นจบ
                 // GameObject spawnedEffect = Instantiate(speedBuffParticlePrefab, spawnPosition, Quaternion.identity);
                 // Destroy(spawnedEffect, 3f); // ตั้งเวลาให้เหมาะสมกับ Particle ของคุณ
                 // หรือถ้า Particle System ใน Prefab ตั้งค่า Stop Action เป็น Destroy ไว้อยู่แล้ว ก็ไม่ต้องทำอะไรเพิ่ม
            }
            else
            {
                Debug.LogWarning("Speed Buff Particle Prefab is not assigned in the Inspector!", this.gameObject);
            }
        }
    }

    private void UpdateSpeed()
    {
        float targetSpeed = baseSpeed; // เริ่มจากความเร็วพื้นฐาน

        if (isBuffActive)
        {
            targetSpeed = baseSpeed * buffSpeedMultiplier;
            // Optional: ให้ Sprint ทับซ้อนกับ Buff
            // if (isSprinting) targetSpeed *= sprintSpeedMultiplier;
        }
        else if (isSprinting)
        {
            targetSpeed = baseSpeed * sprintSpeedMultiplier;
        }

        currentSpeed = targetSpeed;
        // Debug.Log($"Speed updated to: {currentSpeed}");
    }

    private IEnumerator SpeedBuffRoutine(float multiplier, float duration)
    {
        isBuffActive = true;
        UpdateSpeed(); // Apply buff speed immediately
        Debug.Log($"Speed buff activated! New speed: {currentSpeed}");

        yield return new WaitForSeconds(duration);

        isBuffActive = false;
        speedBuffCoroutine = null;
        UpdateSpeed(); // Recalculate speed based on current state (sprinting or not)
        Debug.Log($"Speed buff deactivated! Speed is now: {currentSpeed}");
    }
}