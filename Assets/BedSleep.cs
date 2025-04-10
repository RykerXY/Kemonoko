using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // จำเป็นสำหรับ Coroutines

public class BedSleep : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("ชื่อ Scene ที่จะโหลดหลังจากนอน")]
    public string sceneToLoad = "Day 1 Night"; // ทำให้แก้ไขชื่อ Scene ใน Inspector ได้

    [Header("Interaction Settings")]
    [Tooltip("ข้อความที่จะแสดงเมื่อ Player อยู่ในระยะ (อาจใช้กับ UI อื่นๆ)")]
    public string interactionPrompt = "กด E เพื่อหลับ"; // เก็บข้อความไว้เผื่อใช้

    [Header("Optional: Player Control")]
    [Tooltip("(Optional) ลาก Player Controller มาใส่เพื่อหยุดการเคลื่อนไหวขณะ Fade")]
    public PlayerController playerController; // Reference ไปยัง Player Controller (ถ้ามี)

    // --- Internal State ---
    private bool playerInRange = false;   // สถานะว่า Player อยู่ในระยะหรือไม่
    private bool isInteracting = false; // สถานะว่ากำลังอยู่ในกระบวนการ Fade/Load หรือไม่

    void Start()
    {
        // (ทางเลือก) ลองค้นหา Player Controller อัตโนมัติถ้ายังไม่ได้ Assign
        if (playerController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                     // Debug.Log("Sleep script found PlayerController automatically.");
                }
            }
             // ไม่ต้องแสดง Error ถ้าหาไม่เจอ เพราะเป็น Optional
             // else { Debug.LogWarning("Sleep script could not find Player object or PlayerController component."); }
        }
    }

    void Update()
    {
        // ตรวจสอบเงื่อนไขการเริ่มปฏิสัมพันธ์ใน Update
        if (playerInRange && !isInteracting && Input.GetKeyDown(KeyCode.E))
        {
            StartSleepSequence();
        }
    }

    private void StartSleepSequence()
    {
        // ตั้งค่าสถานะว่ากำลังทำงาน เพื่อป้องกันการกด E ซ้ำ
        isInteracting = true;
        Debug.Log("Starting sleep sequence...");

        // (ทางเลือก) หยุดการเคลื่อนไหวของ Player
        if (playerController != null)
        {
            playerController.canMove = false; // สมมติว่า PlayerController มีตัวแปร canMove
        }

        // เริ่ม Coroutine สำหรับ Fade และ โหลด Scene
        StartCoroutine(FadeAndLoad());
    }

    IEnumerator FadeAndLoad()
    {
        // 1. ตรวจสอบว่ามี ScreenFader หรือไม่
        if (ScreenFader.instance == null)
        {
            Debug.LogError("ไม่พบ ScreenFader instance! กำลังโหลด Scene โดยตรง.");
            SceneManager.LoadScene(sceneToLoad);
            yield break; // ออกจาก Coroutine
        }

        // 2. เริ่ม Fade Out และรอจนกว่าจะเสร็จ
        Debug.Log("Fading out...");
        // ใช้ yield return เพื่อรอให้ FadeOutCoroutine ใน ScreenFader ทำงานจนจบ
        yield return ScreenFader.instance.FadeOut(); // สามารถใส่ duration ที่นี่ได้ เช่น FadeOut(1.0f)

        // 3. เมื่อ Fade Out เสร็จ (โค้ดมาถึงบรรทัดนี้) ให้โหลด Scene ใหม่
        Debug.Log("Fade complete. Loading scene: " + sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);

        // หมายเหตุ: isInteracting ไม่จำเป็นต้องตั้งกลับเป็น false ที่นี่
        // เพราะ Script นี้จะถูกทำลายไปพร้อมกับ Scene เดิมเมื่อโหลด Scene ใหม่
    }

    // --- Trigger Detection ---

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // แสดง Prompt (อาจจะส่ง Event ไปให้ UI Manager หรือแสดงผลโดยตรงถ้ามี Canvas ใกล้ๆ)
            Debug.Log(interactionPrompt); // แสดงใน Console เพื่อทดสอบ
            // ตัวอย่างการส่ง Event (ถ้ามี UI Manager ที่รับฟัง):
            // UIManager.instance?.ShowInteractionPrompt(interactionPrompt);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            // ซ่อน Prompt
            // ตัวอย่าง:
            // UIManager.instance?.HideInteractionPrompt();
        }
    }
}