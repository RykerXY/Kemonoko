using UnityEngine;
using System.Collections;
using Unity.Cinemachine; // <<< เพิ่ม Namespace นี้

[RequireComponent(typeof(Collider2D))]
public class SimpleTeleporter : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform targetLocation;
    public KeyCode activationKey = KeyCode.E;

    [Tooltip("ระยะเวลาในการ Fade เข้า/ออก (วินาที)")]
    public float fadeDuration = 0.5f;

    [Header("Cinemachine Settings")]
    [Tooltip("Virtual Camera ที่กำลังติดตาม Player (ลาก VCam จาก Hierarchy มาใส่)")]
    public CinemachineCamera activePlayerVCam; // <<< อ้างอิงถึง VCam

    [Header("Visual Feedback (Optional)")]
    public GameObject interactionPrompt;

    private bool playerInRange = false;
    private GameObject playerObject = null;
    private bool isTeleporting = false;

    void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"Collider บน {gameObject.name} ไม่ได้ตั้งค่าเป็น 'Is Trigger'", gameObject);
        }
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        if (targetLocation == null)
        {
            Debug.LogError($"Teleporter '{gameObject.name}' ยังไม่ได้กำหนด Target Location!", gameObject);
        }

        // ตรวจสอบว่ากำหนด VCam หรือยัง
        if (activePlayerVCam == null)
        {
            // อาจจะลองค้นหาอัตโนมัติ แต่การกำหนดใน Inspector จะชัวร์กว่า
            Debug.LogError($"SimpleTeleporter บน {gameObject.name} ยังไม่ได้กำหนด Active Player VCam!", gameObject);
            // FindObjectOfType<CinemachineVirtualCamera>(); // << ไม่แนะนำถ้ามี VCam หลายตัว
        }
        // ตรวจสอบว่า VCam มี Follow Target เป็น Player หรือไม่ (ทำใน Inspector จะง่ายกว่า)
        // else if (activePlayerVCam.Follow == null) {
        //    Debug.LogWarning($"VCam '{activePlayerVCam.name}' ไม่มี Follow target!", activePlayerVCam.gameObject);
        // }
    }

    void Update()
    {
        // ตรวจสอบ input และเงื่อนไขต่างๆ รวมถึง VCam
        if (playerInRange && Input.GetKeyDown(activationKey) && !isTeleporting)
        {
            if (playerObject != null && targetLocation != null && ScreenFader.instance != null && activePlayerVCam != null)
            {
                StartCoroutine(TeleportSequenceCoroutine(playerObject));
            }
            else
            {
                // แสดงข้อผิดพลาดที่เจาะจงมากขึ้น
                if(playerObject == null) Debug.LogWarning("Teleport cancelled: Player object lost.", gameObject);
                if(targetLocation == null) Debug.LogError("Teleport cancelled: Target Location is null!", gameObject);
                if(ScreenFader.instance == null) Debug.LogError("Teleport cancelled: ScreenFader instance not found!", gameObject);
                if(activePlayerVCam == null) Debug.LogError("Teleport cancelled: Active Player VCam not assigned!", gameObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerObject = other.gameObject;
            // ตรวจสอบให้แน่ใจว่า VCam ตัวนี้ติดตาม Player ที่เข้ามาจริงๆ
            if (activePlayerVCam != null && activePlayerVCam.Follow != other.transform)
            {
                Debug.LogWarning($"VCam '{activePlayerVCam.name}' อาจจะไม่ได้ติดตาม Player '{other.name}' ที่เข้ามาในระยะ!", gameObject);
            }

            if (interactionPrompt != null && !isTeleporting)
            {
                interactionPrompt.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.gameObject == playerObject)
        {
            playerInRange = false;
            playerObject = null;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    private IEnumerator TeleportSequenceCoroutine(GameObject playerToTeleport)
    {
        isTeleporting = true;
        if (interactionPrompt != null) interactionPrompt.SetActive(false);

        Debug.Log("เริ่มต้น Fade Out...");
        // 1. Fade Out
        yield return ScreenFader.instance.FadeOut(fadeDuration);

        // --- ตอนนี้หน้าจอดำสนิท ---
        Debug.Log("หน้าจอดำ: กำลังวาร์ป Player และแจ้ง Cinemachine...");

        // เก็บตำแหน่งเดิมไว้ก่อนย้าย
        Vector3 originalPlayerPos = playerToTeleport.transform.position;

        // 2. ทำการ Teleport Player
        playerToTeleport.transform.position = targetLocation.position;

        // รีเซ็ตความเร็ว Player (ถ้ามี)
        Rigidbody2D rb = playerToTeleport.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // 3. แจ้ง Cinemachine VCam ว่า Target ได้วาร์ปแล้ว <<< สำคัญมาก!
        if (activePlayerVCam != null)
        {
            // คำนวณระยะทางที่ Player เคลื่อนที่ไป
            Vector3 positionDelta = playerToTeleport.transform.position - originalPlayerPos;
            // เรียกฟังก์ชันของ Cinemachine เพื่อให้ VCam Snap ตามทันทีโดยไม่ใช้ Damping
            activePlayerVCam.OnTargetObjectWarped(playerToTeleport.transform, positionDelta);
            Debug.Log($"แจ้ง VCam '{activePlayerVCam.name}' ว่า Player วาร์ปไป {positionDelta.magnitude} หน่วย.");
        }

        // --- หน้าจอยังคงดำอยู่ ---

        // (Optional) รอ Frame หนึ่งเพื่อให้ Cinemachine ประมวลผลการ Warp
        // yield return null; // ปกติไม่จำเป็น แต่ใส่ไว้กันเหนียวได้

        Debug.Log("เริ่มต้น Fade In...");
        // 4. Fade In
        yield return ScreenFader.instance.FadeIn(fadeDuration);

        Debug.Log("Teleport Sequence เสร็จสิ้น");
        isTeleporting = false;

    }
}