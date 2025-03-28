using UnityEngine;
using UnityEngine.EventSystems; // จำเป็นสำหรับ IPointerEnterHandler, IPointerExitHandler
using TMPro; // จำเป็นสำหรับ TextMeshProUGUI
using System.Collections; // จำเป็นสำหรับ Coroutine

public class ButtonTextScaler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("Text Component ที่ต้องการให้ขยาย (ถ้าไม่กำหนด จะพยายามหาใน Children)")]
    public TextMeshProUGUI targetText;

    [Tooltip("ขนาดที่จะขยายใหญ่ขึ้น (เช่น 1.2 คือใหญ่ขึ้น 20%)")]
    public float scaleFactor = 1.2f;

    [Tooltip("ระยะเวลาในการเปลี่ยนขนาด (วินาที)")]
    public float animationDuration = 0.1f;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    void Awake()
    {
        // ถ้าไม่ได้กำหนด targetText ใน Inspector ให้ลองหาใน Children
        if (targetText == null)
        {
            targetText = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (targetText != null)
        {
            // เก็บค่า Scale เริ่มต้น
            originalScale = targetText.rectTransform.localScale;
        }
        else
        {
            Debug.LogError("ไม่พบ TextMeshProUGUI component บนปุ่มนี้หรือใน Children!", this);
            this.enabled = false; // ปิดการทำงานของสคริปต์นี้ถ้าหา Text ไม่เจอ
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetText != null)
        {
            // หยุด Coroutine เก่า (ถ้ามี) ก่อนเริ่มอันใหม่
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
            }
            // เริ่ม Coroutine เพื่อขยายขนาด
            scaleCoroutine = StartCoroutine(ScaleText(originalScale * scaleFactor));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetText != null)
        {
            // หยุด Coroutine เก่า (ถ้ามี) ก่อนเริ่มอันใหม่
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
            }
            // เริ่ม Coroutine เพื่อกลับไปขนาดเดิม
            scaleCoroutine = StartCoroutine(ScaleText(originalScale));
        }
    }

    private IEnumerator ScaleText(Vector3 targetScale)
    {
        Vector3 currentScale = targetText.rectTransform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            // คำนวณ Scale ใหม่โดยใช้ Lerp (Linear Interpolation)
            targetText.rectTransform.localScale = Vector3.Lerp(currentScale, targetScale, elapsedTime / animationDuration);

            elapsedTime += Time.deltaTime; // เพิ่มเวลาที่ผ่านไป
            yield return null; // รอเฟรมถัดไป
        }

        // ตั้งค่า Scale ให้ตรงเป๊ะเมื่อ Animation จบ
        targetText.rectTransform.localScale = targetScale;
        scaleCoroutine = null; // เคลียร์ Coroutine reference
    }
}