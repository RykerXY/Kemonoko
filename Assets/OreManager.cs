using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal; // สำหรับ Light2D

public class GlowingOreManager : MonoBehaviour
{
    [Header("Ore Setup")]
    [Tooltip("ลาก GameObject ของแร่เริ่มต้นทั้งหมดมาใส่ (ตามลำดับการวน)")]
    public List<GameObject> initialOres = new List<GameObject>(); // เปลี่ยนชื่อเป็น initialOres เพื่อความชัดเจน

    [Header("Timing & Effects")]
    [Tooltip("ระยะเวลาที่ใช้ในการ Fade In แสง (วินาที)")]
    public float fadeInDuration = 0.5f;
    [Tooltip("ระยะเวลาที่แร่จะเรืองแสงเต็มที่ (วินาที) ก่อนเริ่ม Fade Out")]
    public float glowDuration = 1.5f;
    [Tooltip("ระยะเวลาที่ใช้ในการ Fade Out แสง (วินาที)")]
    public float fadeOutDuration = 0.5f;
    [Tooltip("ความสว่างสูงสุดของแสงเมื่อเรืองแสงเต็มที่")]
    public float targetGlowIntensity = 1.5f;

    // ตัวแปรภายใน
    private List<GameObject> currentOres = new List<GameObject>(); // List ของแร่ที่ยังเหลืออยู่
    private int currentOreIndex = -1;
    private Coroutine mainCycleCoroutine;
    // Dictionary เพื่อเก็บ Coroutine การเฟดของแต่ละแสง (ป้องกันการทับซ้อน/จัดการตอนเก็บ)
    private Dictionary<Light2D, Coroutine> activeFadeCoroutines = new Dictionary<Light2D, Coroutine>();

    void Start()
    {
        // --- ตรวจสอบและเตรียม List ---
        if (initialOres == null || initialOres.Count == 0)
        {
            Debug.LogError("Initial Ores list is not assigned or empty!", this);
            enabled = false;
            return;
        }

        currentOres = new List<GameObject>(initialOres); // คัดลอก List เริ่มต้นมาใช้

        // ตรวจสอบ Component และปิดสถานะเริ่มต้น
        for (int i = currentOres.Count - 1; i >= 0; i--) // วนถอยหลังเผื่อต้องลบตัวที่ไม่มี Component
        {
            GameObject ore = currentOres[i];
            if (ore == null)
            {
                Debug.LogWarning($"Removed null entry from ore list at index {i}");
                currentOres.RemoveAt(i);
                continue;
            }

            Light2D light = ore.GetComponent<Light2D>();
            Collider2D collider = ore.GetComponent<Collider2D>();
            ClickableOre clickable = ore.GetComponent<ClickableOre>();

            bool hasError = false;
            if (light == null) { Debug.LogError($"Ore '{ore.name}' is missing Light2D component.", ore); hasError = true; }
            if (collider == null) { Debug.LogError($"Ore '{ore.name}' is missing Collider2D component.", ore); hasError = true; }
            if (clickable == null) { Debug.LogError($"Ore '{ore.name}' is missing ClickableOre script.", ore); hasError = true; }

            if (hasError)
            {
                currentOres.RemoveAt(i); // เอาแร่ที่มีปัญหาออกไปเลย
                continue;
            }

            // ปิดสถานะเริ่มต้น
            light.intensity = 0f; // เริ่มที่ไม่มีแสง
            light.enabled = true; // เปิด component ไว้ แต่ intensity เป็น 0
            collider.enabled = false; // คลิกไม่ได้
        }

        // --- เริ่มการทำงานถ้ามีแร่เหลือ ---
        if (currentOres.Count > 0)
        {
            currentOreIndex = 0; // เริ่มที่ index 0
            mainCycleCoroutine = StartCoroutine(GlowCycle());
        }
        else
        {
            Debug.LogWarning("No valid ores found to start the cycle.");
            enabled = false;
        }
    }

    IEnumerator GlowCycle()
    {
        while (currentOres.Count > 0) // วน solange มีแร่อยู่
        {
            // --- ตรวจสอบ Index ---
             // ถ้า index เกินขนาด list (อาจเกิดหลังการลบ) ให้วนกลับไป 0
            if (currentOreIndex >= currentOres.Count)
            {
                currentOreIndex = 0;
                if(currentOres.Count == 0) break; // ออกถ้าไม่มีแร่เหลือแล้วจริงๆ
            }

            GameObject oreToGlow = currentOres[currentOreIndex];
            if (oreToGlow == null) // เช็คเผื่อกรณีแปลกๆ
            {
                 Debug.LogWarning($"Ore at index {currentOreIndex} became null unexpectedly. Removing and skipping.");
                 currentOres.RemoveAt(currentOreIndex);
                 continue; // ไปรอบต่อไป
            }

            Light2D light = oreToGlow.GetComponent<Light2D>();
            Collider2D collider = oreToGlow.GetComponent<Collider2D>();

            // --- 1. Fade In ---
            if (light != null)
            {
                collider.enabled = true; // เปิดให้คลิกได้ตอนเริ่ม Fade In
                yield return StartCoroutine(FadeLight(light, 0f, targetGlowIntensity, fadeInDuration));
            }

            // --- 2. Glow Period ---
            yield return new WaitForSeconds(glowDuration);

            // --- 3. Fade Out (ถ้ายังไม่ถูกเก็บไปก่อน) ---
            // ตรวจสอบว่า ore ยังอยู่และ collider ยังเปิดอยู่ไหม (แสดงว่ายังไม่ถูกเก็บ)
            if (oreToGlow != null && collider != null && collider.enabled)
            {
                 collider.enabled = false; // ปิดการคลิกตอนเริ่ม Fade Out
                if (light != null)
                {
                    yield return StartCoroutine(FadeLight(light, light.intensity, 0f, fadeOutDuration));
                }
            }
             // ถ้าถูกเก็บไปแล้ว oreToGlow จะเป็น null หรือ collider.enabled เป็น false, ไม่ต้องทำ Fade out

            // --- เลื่อนไปยังแร่ก้อนถัดไป (ถ้ายังไม่ถูกเก็บ) ---
            // เช็คอีกครั้งว่า object ปัจจุบันยังอยู่ไหม ถ้าอยู่ค่อยเลื่อน index
            // การเลื่อน index จะทำโดยอัตโนมัติเมื่อวนกลับไปต้น while loop และเช็ค currentOres.Count ใหม่
            // แต่เราต้อง +1 ให้ index ถ้าแร่ปัจจุบัน *ไม่* ถูกทำลายไป
            if (oreToGlow != null) // ถ้า ore ปัจจุบันยังไม่ถูกทำลาย (แค่หมดเวลา)
            {
                currentOreIndex = (currentOreIndex + 1) % currentOres.Count;
            }
            // ถ้า ore ถูกทำลายไปแล้ว index จะไม่เปลี่ยนในรอบนี้ แต่รอบหน้า while loop จะจัดการเอง

             // ใส่ yield เล็กน้อยเพื่อให้โอกาส Process อื่นๆ ทำงาน
             yield return null;
        }

        Debug.Log("All ores collected or removed. Stopping cycle.");
        enabled = false; // ไม่มีแร่เหลือแล้ว หยุดทำงาน
    }

    /// <summary>
    /// Coroutine สำหรับ Fade แสงระหว่างค่าหนึ่งไปอีกค่าหนึ่ง
    /// </summary>
    IEnumerator FadeLight(Light2D light, float startIntensity, float endIntensity, float duration)
    {
        if (light == null) yield break; // ออกถ้าไม่มีแสง

        // หยุด Coroutine การเฟดอันเก่าของแสงนี้ (ถ้ามี)
        if (activeFadeCoroutines.ContainsKey(light) && activeFadeCoroutines[light] != null)
        {
            StopCoroutine(activeFadeCoroutines[light]);
        }

        // เริ่ม Coroutine ใหม่และเก็บ Reference ไว้
        Coroutine fadeCoroutine = StartCoroutine(PerformFade(light, startIntensity, endIntensity, duration));
        activeFadeCoroutines[light] = fadeCoroutine;

        yield return fadeCoroutine; // รอให้ Coroutine นี้ทำงานจนจบ

        // ลบออกจาก Dictionary เมื่อทำงานเสร็จ
        activeFadeCoroutines.Remove(light);
    }

    // Coroutine ย่อยที่ทำการ Fade จริงๆ
    IEnumerator PerformFade(Light2D light, float startIntensity, float endIntensity, float duration)
    {
        if (duration <= 0) // ถ้าไม่มีระยะเวลา ให้ตั้งค่าทันที
        {
            if(light != null) light.intensity = endIntensity;
            yield break;
        }

        float timer = 0f;
        while (timer < duration)
        {
            if (light == null) yield break; // หยุดถ้าแสงหายไประหว่างทาง

            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            light.intensity = Mathf.Lerp(startIntensity, endIntensity, progress);
            yield return null; // รอเฟรมถัดไป
        }

        // ทำให้แน่ใจว่าค่าสุดท้ายถูกต้อง
        if (light != null) light.intensity = endIntensity;
    }


    /// <summary>
    /// ฟังก์ชันที่ ClickableOre เรียกเมื่อมีการคลิกแร่
    /// </summary>
    public void TryCollectOre(GameObject clickedOre)
    {
        // ตรวจสอบว่าคลิกถูกก้อนและก้อนนั้นยังอยู่ใน List หรือไม่
        int clickedIndex = currentOres.IndexOf(clickedOre);

        // คลิกถูกก้อนที่เรืองแสงอยู่หรือไม่ (index ตรง และ index ต้องเป็นอันที่กำลังวนอยู่)
        if (clickedIndex != -1 && clickedIndex == currentOreIndex)
        {
            Debug.Log($"Collecting ore: {clickedOre.name}");

            // --- เพิ่มคะแนน ---
            if (GlobalPoint.instance != null)
            {
                GlobalPoint.instance.orePoints += 1; // ใช้ orePoints
                Debug.Log($"Added 1 point. Total Ore Points: {GlobalPoint.instance.orePoints}");
            }
            else
            {
                Debug.LogWarning("GlobalPoint instance not found. Cannot add points.");
            }

            // --- หยุดการเฟดที่อาจกำลังทำงานอยู่กับแสงนี้ ---
            Light2D light = clickedOre.GetComponent<Light2D>();
            if (light != null && activeFadeCoroutines.ContainsKey(light) && activeFadeCoroutines[light] != null)
            {
                StopCoroutine(activeFadeCoroutines[light]);
                activeFadeCoroutines.Remove(light);
                // Debug.Log($"Stopped fade coroutine for collected ore: {clickedOre.name}");
            }

            // --- ลบออกจาก List และทำลาย GameObject ---
            currentOres.RemoveAt(clickedIndex); // ลบออกจาก List
            Destroy(clickedOre);                // ทำลาย GameObject

            // --- หยุด Coroutine การวนลูปปัจจุบัน แล้วเริ่มใหม่ ---
            // การเริ่มใหม่จะช่วยให้มันคำนวณ index และเริ่มรอบใหม่กับ list ที่อัปเดตแล้วได้ถูกต้อง
            if (mainCycleCoroutine != null)
            {
                StopCoroutine(mainCycleCoroutine);
            }

            if (currentOres.Count > 0) // ถ้ายังมีแร่เหลือ
            {
                // Index ไม่ต้อง +1 เพราะการลบทำให้ตัวถัดไปเลื่อนมาแทนที่
                // แต่ต้องแน่ใจว่า index ไม่เกินขนาด list ใหม่
                 if (currentOreIndex >= currentOres.Count) {
                      currentOreIndex = 0; // วนกลับไปถ้า index เดิมชี้ไปนอก list แล้ว
                 }
                mainCycleCoroutine = StartCoroutine(GlowCycle()); // เริ่ม cycle ใหม่
            }
            else
            {
                Debug.Log("Last ore collected!");
                enabled = false; // ไม่มีแร่เหลือแล้ว หยุดทำงาน
            }

            // เล่นเสียงหรือเอฟเฟกต์
            // AudioManager.instance.PlaySound("OreCollect");
        }
        // ไม่ต้องทำอะไรถ้าคลิกผิดก้อน (เพราะ collider ควรจะปิดอยู่)
    }

    void OnDestroy()
    {
        // หยุด Coroutine ทั้งหมดเมื่อ Manager ถูกทำลาย
        StopAllCoroutines();
    }
}