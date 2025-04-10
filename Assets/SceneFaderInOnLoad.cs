using UnityEngine;
using UnityEngine.UI; // สำหรับ Image
using System.Collections;

public class SceneFaderInOnLoad : MonoBehaviour
{
    [Header("Fade Settings")]
    [Tooltip("UI Image ที่ใช้สำหรับ Fade In (ต้องอยู่ใน Scene นี้)")]
    public Image fadeInImage; // Image สำหรับ Fade ใน Scene นี้

    [Tooltip("ระยะเวลาในการ Fade In (วินาที)")]
    public float fadeInDuration = 0.5f; // สามารถตั้งค่าแยกแต่ละ Scene ได้

    void Start()
    {
        // ตรวจสอบว่ามี Image หรือไม่
        if (fadeInImage == null)
        {
            Debug.LogError($"[{gameObject.name}] SceneFaderInOnLoad: ยังไม่ได้กำหนด Fade In Image!", this);
            return; // ไม่ทำอะไรถ้าไม่มี Image
        }

        // เริ่ม Coroutine Fade In ทันที
        StartCoroutine(FadeInCoroutine());
    }

    private IEnumerator FadeInCoroutine()
    {
        // 1. ตั้งค่าเริ่มต้น: ทำให้ Image ทึบและเปิดใช้งาน
        Color currentColor = fadeInImage.color;
        currentColor.a = 1f; // เริ่มต้นทึบ
        fadeInImage.color = currentColor;
        fadeInImage.gameObject.SetActive(true);

        // 2. ทำการ Fade In (จากทึบไปใส)
        float counter = 0f;
        while (counter < fadeInDuration)
        {
            counter += Time.deltaTime;
            // ลดค่า Alpha จาก 1 ไป 0
            currentColor.a = Mathf.Clamp01(1f - (counter / fadeInDuration));
            fadeInImage.color = currentColor;
            yield return null; // รอเฟรมถัดไป
        }

        // 3. เมื่อเสร็จ: ทำให้แน่ใจว่าใสสนิท และปิด Image
        currentColor.a = 0f;
        fadeInImage.color = currentColor;
        fadeInImage.gameObject.SetActive(false);

        // Debug.Log($"[{gameObject.name}] Fade In complete."); // Optional Log
    }
}