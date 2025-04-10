using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using TMPro; // <<< ถ้ายังใช้ TextMeshPro ในนี้

public class PlayerHealth : MonoBehaviour
{
    // --- Singleton Pattern ---
    public static PlayerHealth instance { get; private set; }
    private bool isInitialized = false; // << Flag เพื่อเช็คว่าเคย Initialize ค่าเริ่มต้นหรือยัง
    // --- ---

    [Header("Health Settings")]
    public int maxHealth = 3;
    [SerializeField]
    private int currentHealth; // ค่านี้จะคงอยู่ถ้า Object ไม่ถูกทำลาย
    [SerializeField]
    private bool isDead = false;

    // ... (ส่วน Header อื่นๆ เหมือนเดิม: Damage Feedback, Events) ...
    [Header("Damage Feedback")]
    public SpriteRenderer playerSpriteRenderer;
    public Color flashColor = Color.red;
    public int numberOfFlashes = 3;
    public float flashCycleDuration = 0.2f;
    private Color originalColor;
    private Coroutine flashCoroutine;

    [Header("Events")]
    public UnityEvent<int, int> OnHealthChanged;
    public UnityEvent OnDeath;

    // ... (Properties เหมือนเดิม) ...
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    // --- เอา TextMeshPro ออกจาก Script นี้ (แนะนำ) ---
    // การอ้างอิง UI โดยตรงใน Script ที่เป็น DontDestroyOnLoad มักมีปัญหา
    // ควรมี Script แยกสำหรับ UI ที่อ่านค่าจาก Singleton นี้แทน
    // [Header("Display")]
    // public TextMeshProUGUI healthText;
    // --- ---

    void Awake()
    {
        // --- Singleton Setup ---
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // ทำให้ Player ไม่ถูกทำลาย
            Debug.Log("PlayerHealth Instance Created and set to DontDestroyOnLoad.");

            // --- Initial Setup (ทำครั้งแรกเท่านั้น) ---
            InitializePlayerState();
            isInitialized = true; // ตั้ง Flag ว่า Initialize แล้ว
            // --- ---
        }
        else if (instance != this)
        {
            // มี Player ตัวอื่นอยู่แล้ว (ตัวเก่าที่ไม่ถูกทำลาย) ทำลายตัวนี้ทิ้ง
            Debug.LogWarning("Another PlayerHealth instance detected. Destroying this duplicate.");
            Destroy(gameObject);
            return; // ออกจาก Awake() ไปเลย ไม่ต้องทำอะไรต่อ
        }
        // ถ้า instance == this (กลับมาโหลด Scene เดิมที่มีตัวเราอยู่แล้ว) ไม่ต้องทำอะไรใน Awake() อีก

        // --- Setup ที่ทำได้ทุกครั้ง (ถ้าจำเป็น) ---
        // เช่น หา SpriteRenderer ถ้ามันยังไม่ได้ตั้งค่า (แต่ควรตั้งใน InitializePlayerState ถ้าเป็นไปได้)
        if (playerSpriteRenderer == null)
        {
            playerSpriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (playerSpriteRenderer != null && !isInitialized) // ตั้ง originalColor ถ้ายังไม่เคยตั้ง
        {
            originalColor = playerSpriteRenderer.color;
        }
    }

    // ฟังก์ชันแยกสำหรับตั้งค่าเริ่มต้นครั้งแรก
    private void InitializePlayerState()
    {
        currentHealth = maxHealth; // << ตั้งค่า Health เริ่มต้นที่นี่!
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
            Debug.LogWarning("PlayerHealth: SpriteRenderer not found during initialization.", gameObject);
        }
        Debug.Log("Player Initialized with full health.");
    }


    void Start()
    {
        // เรียก Event เพื่ออัปเดต UI ครั้งแรก หรือเมื่อกลับเข้า Scene
        // ตรวจสอบ isInitialized เผื่อกรณีเป็น Duplicate ที่กำลังจะถูก Destroy
        if (isInitialized || instance == this)
        {
             OnHealthChanged?.Invoke(currentHealth, maxHealth);
             // updateHelathText(); // ถ้ายังใช้ TextMeshPro ในนี้ (ไม่แนะนำ)
        }
    }

    // --- เมธอด TakeDamage, Heal, Die, FlashEffect, updateHelathText เหมือนเดิม ---
    // ... (คัดลอกเมธอดเดิมมาใส่ที่นี่ได้เลย ไม่ต้องเปลี่ยนแปลง) ...
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
        // updateHelathText(); // ถ้ายังใช้ TextMeshPro ในนี้

        if (playerSpriteRenderer != null && !isDead)
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                 playerSpriteRenderer.color = originalColor;
            }
            flashCoroutine = StartCoroutine(FlashMultipleTimesEffect());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        if (isDead || healAmount <= 0 || currentHealth >= maxHealth) return;
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log($"{gameObject.name} healed {healAmount}. Current Health: {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        // updateHelathText(); // ถ้ายังใช้ TextMeshPro ในนี้
    }

     private void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log($"{gameObject.name} has died!");

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
             if(playerSpriteRenderer != null) playerSpriteRenderer.color = originalColor;
        }

        OnDeath?.Invoke();
         Destroy(gameObject, 1.5f); // อาจจะต้องพิจารณาการจัดการการตายใหม่ ถ้า Player ต้องอยู่ต่อ
         // หรืออาจจะแค่ disable component ต่างๆ แทนการ Destroy
    }

    private IEnumerator FlashMultipleTimesEffect()
    {
        float singleFlashHalfDuration = flashCycleDuration / 2f;
        if (playerSpriteRenderer == null) yield break;

        for (int i = 0; i < numberOfFlashes; i++)
        {
             if (isDead) yield break;
            playerSpriteRenderer.color = flashColor;
            yield return new WaitForSeconds(singleFlashHalfDuration);
             if (isDead || playerSpriteRenderer == null) yield break;
            playerSpriteRenderer.color = originalColor;
            yield return new WaitForSeconds(singleFlashHalfDuration);
        }
         if (playerSpriteRenderer != null) {
             playerSpriteRenderer.color = originalColor;
         }
        flashCoroutine = null;
    }

    /*public void updateHelathText()
    {
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
        else
        {
             Debug.LogWarning("HealthText is not assigned in PlayerHealth.");
        }
    }*/
}