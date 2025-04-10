using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoad : MonoBehaviour
{
    [Header("Fade Settings")]
    [Tooltip("ระยะเวลาที่ต้องการให้ Fade Out สำหรับ Script นี้ (วินาที)")]
    public float customFadeDuration = 0.3f; // << เพิ่มตัวแปร หรือใช้ค่าคงที่

    public void LoadScene(string sceneName)
    {
        StartCoroutine(FadeAndLoadSceneCoroutine(sceneName));
    }

    private IEnumerator FadeAndLoadSceneCoroutine(string sceneToLoad)
    {
        Debug.Log($"เริ่มต้นกระบวนการโหลด Scene: {sceneToLoad}");

        if (ScreenFader.instance != null)
        {
            Debug.Log($"กำลัง Fade Out (Duration: {customFadeDuration}s)...");
            // --- ส่วนที่เปลี่ยน ---
            // ส่งค่า customFadeDuration ไปให้ FadeOut()
            yield return ScreenFader.instance.FadeOut(customFadeDuration);
            // --------------------
            Debug.Log("Fade Out เสร็จสิ้น.");
        }
        else
        {
            Debug.LogError("ไม่พบ ScreenFader instance! กำลังโหลด Scene โดยตรง (ไม่มี Fade).");
        }

        Debug.Log($"กำลังโหลด Scene: {sceneToLoad}");
        SceneManager.LoadScene(sceneToLoad);
    }
}