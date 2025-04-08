using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// เปลี่ยนชื่อ Class เล็กน้อยเพื่อไม่ให้ซ้ำกับ Script เดิมของคุณ
public class NpcVeggieDestroyer : MonoBehaviour
{
    [Header("Target Settings")]
    public string targetTag = "Veggie";

    [Header("Movement")]
    public float moveSpeed = 3.0f;
    public float stoppingDistance = 0.5f;

    [Header("Searching")]
    [Tooltip("ความถี่ในการค้นหาเป้าหมายใหม่ (วินาที) เมื่อยังไม่มีเป้าหมาย")]
    public float searchInterval = 0.5f; // << อาจจะลดค่านี้ลงเพื่อให้ตอบสนองเร็วขึ้น

    private GameObject currentTarget = null;
    private float timeSinceLastSearch = 0f;
    // ไม่จำเป็นต้องมี searchingEnabled แล้ว เพราะจะค้นหาเรื่อยๆ จนกว่าจะเจอ

    void Start()
    {
        // ลองค้นหาครั้งแรก เผื่อมี Veggie อยู่แล้ว
        FindNewTarget();
        // ตั้งค่าเริ่มต้นให้ค้นหาได้เลยใน Update ถัดไปถ้ายังไม่เจอ
        timeSinceLastSearch = searchInterval;
    }

    void Update()
    {
        // --- ส่วนการค้นหา (ทำงานเมื่อไม่มีเป้าหมาย) ---
        if (currentTarget == null)
        {
            timeSinceLastSearch += Time.deltaTime;
            // เมื่อถึงเวลาค้นหา ให้ลองค้นหาใหม่
            if (timeSinceLastSearch >= searchInterval)
            {
                FindNewTarget();
                timeSinceLastSearch = 0f; // รีเซ็ตเวลา
            }

            // ถ้าหลังจากค้นหาแล้วยังไม่มีเป้าหมาย ก็ไม่ต้องทำอะไรต่อในเฟรมนี้
            if (currentTarget == null)
            {
                // อาจจะให้แสดง Animation Idle หรืออื่นๆ ที่นี่
                return;
            }
        }

        // --- ส่วนการเคลื่อนที่และทำลาย (ทำงานเมื่อ *มี* เป้าหมาย) ---
        // ตรวจสอบเผื่อเป้าหมายถูกทำลายไประหว่างเฟรมโดยสิ่งอื่น
        if (currentTarget == null) return;

        try // ใช้ try-catch ป้องกัน Error ถ้าเป้าหมายหายไปกระทันหัน
        {
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (distanceToTarget <= stoppingDistance)
            {
                // ถึงระยะ ให้ทำลาย
                DestroyTarget();
                // ค้นหาเป้าหมาย *ต่อไปทันที* หลังจากทำลาย ไม่ต้องรอ searchInterval
                FindNewTarget();
                timeSinceLastSearch = 0f; // รีเซ็ตเวลาเผื่อหาไม่เจอ
            }
            else
            {
                // ยังไม่ถึงระยะ ให้เคลื่อนที่เข้าหา
                MoveTowardsTarget();
            }
        }
        catch (MissingReferenceException)
        {
            // กรณีที่ currentTarget ถูกทำลายไปแล้วโดย Script อื่น
            // Debug.LogWarning($"NPC {gameObject.name}: Target was destroyed unexpectedly. Finding new target.");
            currentTarget = null; // เคลียร์เป้าหมายที่หายไป
            timeSinceLastSearch = searchInterval; // ตั้งเวลาให้ค้นหาใหม่ในเฟรมหน้า
        }
    }

    bool FindNewTarget()
    {
        // ค้นหา GameObject ทั้งหมด ณ เวลานั้น
        GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag(targetTag);

        if (potentialTargets.Length == 0)
        {
            currentTarget = null;
            return false; // ไม่เจอเป้าหมาย
        }

        // หาตัวที่ใกล้ที่สุด (ใช้ Linq เพื่อความกระชับ)
        currentTarget = potentialTargets
            .OrderBy(t => Vector3.Distance(transform.position, t.transform.position))
            .FirstOrDefault(); // เอาตัวแรก (ใกล้สุด) หรือ null ถ้า List ว่าง (ซึ่งไม่ควรเกิดถ้า Length > 0)

        // if (currentTarget != null) {
        //     Debug.Log($"NPC {gameObject.name}: Found new target: {currentTarget.name}");
        // }

        return (currentTarget != null); // คืนค่า true ถ้าเจอเป้าหมาย
    }

    void MoveTowardsTarget()
    {
        if (currentTarget == null || moveSpeed <= 0) return;

        Vector3 targetPosition = currentTarget.transform.position;
        targetPosition.z = transform.position.z; // ล็อคแกน Z สำหรับ 2D

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // การหันหน้า (เหมือนเดิม)
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (Mathf.Abs(direction.x) > 0.01f)
        {
            float currentScaleX = Mathf.Abs(transform.localScale.x);
            transform.localScale = new Vector3(direction.x > 0 ? currentScaleX : -currentScaleX, transform.localScale.y, transform.localScale.z);
        }
    }

    void DestroyTarget()
    {
        if (currentTarget == null) return;
        // Debug.Log($"NPC {gameObject.name}: Destroying target: {currentTarget.name}");
        GameObject targetToDestroy = currentTarget;
        currentTarget = null; // เคลียร์เป้าหมาย *ก่อน* ทำลาย
        Destroy(targetToDestroy);
    }

    void OnDrawGizmos()
    {
        if (currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, stoppingDistance);
        }
    }
}