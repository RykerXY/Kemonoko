using UnityEngine;
using UnityEngine.UI; // ต้องใช้สำหรับ UI Elements

public class HealthUI : MonoBehaviour
{
    public Slider healthSlider;
    // public Text healthText; // (Optional) ถ้าต้องการแสดงตัวเลข

    // ฟังก์ชันนี้จะถูกเรียกจาก UnityEvent ของ PlayerHealth
    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (healthSlider != null)
        {
            // ตั้งค่าค่าสูงสุดของ Slider (เผื่อมีการเพิ่ม Max Health ในอนาคต)
            healthSlider.maxValue = maxHealth;
            // ตั้งค่าค่าปัจจุบันของ Slider
            healthSlider.value = currentHealth;
        }

        // if (healthText != null)
        // {
        //     healthText.text = $"{currentHealth} / {maxHealth}";
        // }
    }
}