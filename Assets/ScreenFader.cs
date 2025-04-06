using UnityEngine;
using UnityEngine.UI; // จำเป็นสำหรับ UI elements
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader instance; // Singleton pattern สำหรับการเข้าถึงง่ายๆ

    [Tooltip("UI Image ที่ใช้สำหรับ Fade (ลาก FadeImage จาก Hierarchy มาใส่)")]
    public Image fadeImage;
    [Tooltip("ความเร็วในการ Fade (วินาที)")]
    public float defaultFadeDuration = 0.5f;

    void Awake()
    {
        // ตั้งค่า Singleton
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject); // เอาออกถ้าไม่ต้องการให้ Fader คงอยู่ข้าม Scene
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // ตรวจสอบว่ามีการกำหนด fadeImage หรือยัง
        if (fadeImage == null)
        {
            Debug.LogError("ScreenFader: ยังไม่ได้กำหนด Fade Image!", gameObject);
        }
        else
        {
            // ตั้งค่าเริ่มต้นให้โปร่งใสและปิดไว้
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0f);
            fadeImage.gameObject.SetActive(false);
        }
    }

    // --- ฟังก์ชันสำหรับเรียกใช้ Fade ---

    // Fade จากใส -> ดำ
    public Coroutine FadeOut(float duration = -1f)
    {
        if (duration < 0) duration = defaultFadeDuration; // ใช้ค่า default ถ้าไม่ระบุ
        // หยุด Coroutine ที่ทำงานอยู่ก่อนหน้า (ถ้ามี)
        StopAllCoroutines();
        // เริ่ม Coroutine ใหม่และ return ค่าเพื่อให้รอได้
        return StartCoroutine(FadeOutCoroutine(duration));
    }

    // Fade จากดำ -> ใส
    public Coroutine FadeIn(float duration = -1f)
    {
        if (duration < 0) duration = defaultFadeDuration;
        StopAllCoroutines();
        return StartCoroutine(FadeInCoroutine(duration));
    }

    // --- Coroutines สำหรับทำงาน Fade ---

    private IEnumerator FadeOutCoroutine(float duration)
    {
        if (fadeImage == null) yield break; // ออกถ้าไม่มี Image

        fadeImage.gameObject.SetActive(true); // เปิด Image ขึ้นมา
        float counter = 0f;
        Color currentColor = fadeImage.color;
        currentColor.a = 0f; // เริ่มต้นใส
        fadeImage.color = currentColor;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            currentColor.a = Mathf.Clamp01(counter / duration); // คำนวณ Alpha
            fadeImage.color = currentColor;
            yield return null; // รอเฟรมถัดไป
        }

        currentColor.a = 1f; // ทำให้แน่ใจว่าทึบสนิท
        fadeImage.color = currentColor;
    }

    private IEnumerator FadeInCoroutine(float duration)
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true); // ต้องเปิดอยู่เพื่อ Fade
        float counter = 0f;
        Color currentColor = fadeImage.color;
        currentColor.a = 1f; // เริ่มต้นทึบ
        fadeImage.color = currentColor;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            currentColor.a = Mathf.Clamp01(1f - (counter / duration)); // คำนวณ Alpha (ลดลง)
            fadeImage.color = currentColor;
            yield return null; // รอเฟรมถัดไป
        }

        currentColor.a = 0f; // ทำให้แน่ใจว่าใสสนิท
        fadeImage.color = currentColor;
        fadeImage.gameObject.SetActive(false); // ปิด Image เมื่อ Fade เสร็จ
    }
}