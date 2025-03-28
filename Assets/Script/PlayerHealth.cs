using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    [SerializeField]
    private int currentHealth;
    [SerializeField]
    private bool isDead = false;

    [Header("Damage Feedback")]
    [Tooltip("SpriteRenderer ของตัวละครผู้เล่น")]
    public SpriteRenderer playerSpriteRenderer;
    [Tooltip("สีที่จะให้กระพริบเมื่อโดน Damage")]
    public Color flashColor = Color.red;
    // --- เปลี่ยนจาก flashDuration เป็นค่าเหล่านี้ ---
    [Tooltip("จำนวนครั้งที่จะกระพริบ")]
    public int numberOfFlashes = 3; // << ใหม่: จำนวนครั้ง
    [Tooltip("ระยะเวลาของแต่ละรอบการกระพริบ (เปิด-ปิด) (วินาที)")]
    public float flashCycleDuration = 0.2f; // << ใหม่: เวลา 1 รอบ (เช่น 0.1 วิ สีแดง, 0.1 วิ สีปกติ)
    // --- ---

    private Color originalColor;
    private Coroutine flashCoroutine;

    [Header("Events")]
    public UnityEvent<int, int> OnHealthChanged;
    public UnityEvent OnDeath;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    [Header("Display")]
    public TextMeshProUGUI healthText;

    void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;

        if (playerSpriteRenderer == null)
        {
            playerSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (playerSpriteRenderer != null)
        {
            originalColor = playerSpriteRenderer.color;
        }
        else
        {
            Debug.LogWarning("PlayerHealth: SpriteRenderer not found. Cannot apply flash effect.", gameObject);
        }
    }

    void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead || damageAmount <= 0)
        {
            return;
        }

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"{gameObject.name} took {damageAmount} damage. Current Health: {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // --- เริ่ม Flash Effect (ถ้ายังไม่ตาย) ---
        if (playerSpriteRenderer != null && !isDead)
        {
            // หยุด Coroutine เก่า (ถ้ามี) เพื่อเริ่มใหม่ทันที
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                // สำคัญ: ตั้งสีกลับเป็นปกติก่อนเริ่มกระพริบรอบใหม่ทันที
                // เพื่อหลีกเลี่ยงกรณีที่หยุดตอนกำลังเป็นสีแดงแล้วเริ่มใหม่เลย
                 playerSpriteRenderer.color = originalColor;
            }
            flashCoroutine = StartCoroutine(FlashMultipleTimesEffect());
        }
        // --- จบส่วน Flash Effect ---

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
         // (โค้ดเดิม ไม่เปลี่ยนแปลง)
        if (isDead || healAmount <= 0 || currentHealth >= maxHealth) return;
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log($"{gameObject.name} healed {healAmount}. Current Health: {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        // (โค้ดส่วนใหญ่เหมือนเดิม)
        if (isDead) return;
        isDead = true;
        Debug.Log($"{gameObject.name} has died!");

        // หยุด Flash Effect ถ้ากำลังทำงาน และตั้งสีกลับ
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
             if(playerSpriteRenderer != null) playerSpriteRenderer.color = originalColor;
        }

        OnDeath?.Invoke();
        // ... (ส่วนจัดการการตายอื่นๆ) ...
         Destroy(gameObject, 1.5f);
    }

    /// <summary>
    /// Coroutine สำหรับทำให้ Sprite กระพริบหลายครั้ง
    /// </summary>
    private IEnumerator FlashMultipleTimesEffect()
    {
        // คำนวณระยะเวลาสำหรับแต่ละครึ่งของรอบ (เปิด หรือ ปิด)
        float singleFlashHalfDuration = flashCycleDuration / 2f;

        // ตรวจสอบว่ามี SpriteRenderer ไหม ถ้าไม่ก็ออกจาก Coroutine เลย
        if (playerSpriteRenderer == null) yield break;

        // เริ่มการกระพริบตามจำนวนครั้งที่กำหนด
        for (int i = 0; i < numberOfFlashes; i++)
        {
             // ถ้าผู้เล่นตายระหว่างกระพริบ ให้หยุดทันที
             if (isDead) yield break;

            // 1. เปลี่ยนเป็นสี Flash
            playerSpriteRenderer.color = flashColor;
            // 2. รอครึ่งรอบ
            yield return new WaitForSeconds(singleFlashHalfDuration);

             // ถ้าผู้เล่นตายระหว่างรอ ให้หยุดทันที
             if (isDead || playerSpriteRenderer == null) yield break; // เช็ค renderer อีกครั้งเผื่อถูกทำลาย

            // 3. เปลี่ยนกลับเป็นสีเดิม
            playerSpriteRenderer.color = originalColor;
            // 4. รออีกครึ่งรอบ
            yield return new WaitForSeconds(singleFlashHalfDuration);
        }

        // เมื่อ Loop จบลง ตรวจสอบให้แน่ใจว่าสีกลับเป็นปกติ (เผื่อกรณี flashCycleDuration เป็น 0 หรืออื่นๆ)
         if (playerSpriteRenderer != null) {
             playerSpriteRenderer.color = originalColor;
         }

        // เคลียร์ reference ของ coroutine ที่ทำงานจบแล้ว
        flashCoroutine = null;
    }

    public void updateHelathText()
    {
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    // (Optional Debug)
    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.K)) TakeDamage(10);
    // }
}