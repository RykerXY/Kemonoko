using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(SpriteRenderer))]
public class Firefly : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("ระยะทางสูงสุดที่จะเคลื่อนที่ไปทางซ้าย/ขวา จากจุดเริ่มต้น")]
    public float moveRangeX = 3.0f;
    [Tooltip("ระยะทางสูงสุดที่จะเคลื่อนที่ขึ้น/ลง จากจุดเริ่มต้น")]
    public float moveRangeY = 0.5f;
    [Tooltip("ความเร็วโดยรวมของการเคลื่อนที่")]
    public float moveSpeed = 1.0f;
    [Tooltip("ความนุ่มนวลของการเปลี่ยนทิศทาง")]
    public float smoothness = 5.0f;

    [Header("Visual Effects")]
    [Tooltip("ระยะเวลาที่ใช้ในการ Fade-in (วินาที)")]
    public float fadeInDuration = 1.5f;

    [Tooltip("ลาก Component Light 2D ของหิ่งห้อยมาใส่ที่นี่ (ถ้ามี)")]
    public Light2D fireflyLight; // Reference to the Light2D component

    [Tooltip("ความสว่างปกติของแสงไฟ")]
    public float baseIntensity = 1.0f;
    [Tooltip("ระดับการเปลี่ยนแปลงความสว่าง (ยิ่งมาก ยิ่งกะพริบแรง)")]
    public float intensityPulsation = 0.3f;
    [Tooltip("ความเร็วในการกะพริบของแสง")]
    public float pulsationSpeed = 3.0f;

    [Header("Lifetime Settings")] // <-- เพิ่ม Header ใหม่
    [Tooltip("ระยะเวลาที่หิ่งห้อยจะมีชีวิตอยู่ (วินาที) ก่อนถูกทำลาย")]
    public float lifespan = 15.0f; // <-- ตัวแปรใหม่สำหรับกำหนดเวลา

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float timeSinceLastTargetChange = 0f;
    private SpriteRenderer spriteRenderer;
    private Color originalSpriteColor;
    private float originalLightIntensity;
    private bool isFadingIn = true;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("Firefly Requires a SpriteRenderer component!", this);
            enabled = false;
            return;
        }

        originalSpriteColor = spriteRenderer.color;
        spriteRenderer.color = new Color(originalSpriteColor.r, originalSpriteColor.g, originalSpriteColor.b, 0f);

        if (fireflyLight != null)
        {
            // ตรวจสอบว่าค่าที่ตั้งใน Inspector ไม่ใช่ 0 ก่อนเก็บ
            // เผื่อเราต้องการให้ baseIntensity เป็น 0 จริงๆ
             if (baseIntensity <= 0) {
                 // ถ้า baseIntensity เป็น 0 หรือน้อยกว่า ให้ใช้ค่า intensity ปัจจุบันที่ตั้งไว้เป็น original แทน
                 originalLightIntensity = fireflyLight.intensity;
             } else {
                 // ถ้า baseIntensity > 0 ให้ใช้ค่า baseIntensity ที่ตั้งไว้ใน script เป็นหลัก
                 originalLightIntensity = baseIntensity; // หรือจะใช้ fireflyLight.intensity ก็ได้ถ้าต้องการเก็บค่าจาก Inspector
             }
            fireflyLight.intensity = 0f;
        } else {
            Debug.LogWarning("Firefly Light 2D component not assigned. Light effects will be skipped.", this);
        }
    }

    void Start()
    {
        startPosition = transform.position;
        SetNewRandomTarget();

        StartCoroutine(FadeInEffect());
    }

    void Update()
    {
        if (!isFadingIn)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed / smoothness);

            timeSinceLastTargetChange += Time.deltaTime;
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            if (distanceToTarget < 0.2f || timeSinceLastTargetChange > smoothness)
            {
                SetNewRandomTarget();
            }

            UpdateLightPulsation();
        }
    }

    IEnumerator FadeInEffect()
    {
        isFadingIn = true;
        float timer = 0f;
        float targetIntensity = (fireflyLight != null) ? baseIntensity : 0f; // กำหนดเป้าหมาย Intensity

        while (timer < fadeInDuration)
        {
            float progress = Mathf.Clamp01(timer / fadeInDuration);

            float currentAlpha = Mathf.Lerp(0f, originalSpriteColor.a, progress);
            spriteRenderer.color = new Color(originalSpriteColor.r, originalSpriteColor.g, originalSpriteColor.b, currentAlpha);

            if (fireflyLight != null)
            {
                 // ใช้ targetIntensity ที่คำนวณไว้ตอนเริ่ม Coroutine
                float currentIntensity = Mathf.Lerp(0f, targetIntensity, progress);
                fireflyLight.intensity = currentIntensity;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = originalSpriteColor;
        if (fireflyLight != null)
        {
            // ไม่จำเป็นต้องตั้งค่าตรงนี้ ปล่อยให้ UpdateLightPulsation จัดการ
        }
        isFadingIn = false;

        // --- เรียกใช้การทำลายตัวเองหลังจาก Fade in เสร็จ ---
        if (lifespan > 0) // ตรวจสอบก่อนว่าตั้งค่า lifespan ไว้หรือไม่
        {
             Invoke("DestroyFirefly", lifespan);
        }
        // -------------------------------------------------
    }

    void UpdateLightPulsation()
    {
        if (fireflyLight != null && !isFadingIn)
        {
            // ใช้ baseIntensity ที่เก็บไว้ตอน Awake หรือที่ตั้งค่าไว้
            float currentBaseIntensity = (originalLightIntensity > 0) ? originalLightIntensity : baseIntensity; // เลือกใช้ค่าที่เหมาะสม
            float intensityOffset = Mathf.Sin(Time.time * pulsationSpeed) * intensityPulsation;
            fireflyLight.intensity = Mathf.Max(0, currentBaseIntensity + intensityOffset); // ใช้ Max ป้องกันติดลบ
        }
    }

    void SetNewRandomTarget()
    {
        float randomX = Random.Range(-moveRangeX, moveRangeX);
        float randomY = Random.Range(-moveRangeY, moveRangeY);
        targetPosition = startPosition + new Vector3(randomX, randomY, 0);
        timeSinceLastTargetChange = 0f;
    }

    private void DestroyFirefly()
    {
        Destroy(gameObject);
    }
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GlobalPoint.instance.fireflyPoints += 1;

            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.color = Color.yellow;
        Vector3 size = new Vector3(moveRangeX * 2, moveRangeY * 2, 0.1f);
        Gizmos.DrawWireCube(center, size);

        if (Application.isPlaying && !isFadingIn)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetPosition);
            Gizmos.DrawWireSphere(targetPosition, 0.1f);
        }
    }
}