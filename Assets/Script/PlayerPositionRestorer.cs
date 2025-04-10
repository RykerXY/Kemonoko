using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerPositionRestorer : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool needsPositionRestore = false;
    private Vector2 targetPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("ไม่พบ Rigidbody2D component บน Player!");
        }
    }

    void Start()
    {
        // ตรวจสอบและตั้งค่าธงใน Start
        if (rb != null && PlayerDataManager.Instance != null && PlayerDataManager.Instance.hasStoredPosition)
        {
            needsPositionRestore = true;
            targetPosition = PlayerDataManager.Instance.lastPlayerPosition;
            Debug.Log($"Position restore needed. Target: {targetPosition}");

            // (ทางเลือก) อาจจะลองปิดการเคลื่อนไหวของผู้เล่นชั่วคราวที่นี่
            var playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }
        }
         else if (PlayerDataManager.Instance != null && !PlayerDataManager.Instance.hasStoredPosition)
        {
             Debug.Log("PlayerDataManager found, but no position stored.");
        }
        else if (PlayerDataManager.Instance == null)
        {
             Debug.LogError("PlayerDataManager Instance not found!");
        }
    }

    void FixedUpdate()
    {
        // ทำงานเฉพาะเมื่อตั้งธงไว้ และทำแค่ครั้งเดียว
        if (needsPositionRestore)
        {
            Debug.Log($"Attempting to restore position in FixedUpdate. Current: {rb.position}");

            // 1. ตั้งค่าตำแหน่ง
            rb.position = targetPosition;

            // 2. รีเซ็ตความเร็ว
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            // 3. เคลียร์ธงและข้อมูลที่เก็บไว้
            needsPositionRestore = false;
            PlayerDataManager.Instance.ClearStoredPosition();

            Debug.Log($"Position restored in FixedUpdate. New rb.position: {rb.position}");

            var playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }
    }
}