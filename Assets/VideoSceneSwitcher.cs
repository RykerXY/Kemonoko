using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(VideoPlayer))]
public class VideoSceneSwitcher : MonoBehaviour
{
    [Tooltip("ชื่อ Scene ที่จะโหลดเมื่อวิดีโอเล่นจบ (ต้องตรงกับชื่อใน Build Settings)")]
    public string nextSceneName; 

    private VideoPlayer videoPlayer;

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>(); 
    }

    void Start()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("VideoSceneSwitcher: โปรดกำหนดค่า Next Scene Name ใน Inspector!");
            enabled = false; 
            return;
        }

        if (videoPlayer == null)
        {
            Debug.LogError("VideoSceneSwitcher: ไม่พบ VideoPlayer component!", gameObject);
            enabled = false;
            return;
        }

        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("วิดีโอเล่นจบแล้ว กำลังเปลี่ยน Scene ไปที่: " + nextSceneName);
        videoPlayer.loopPointReached -= OnVideoFinished;

        SceneManager.LoadScene(nextSceneName);

        StartCoroutine(LoadNextSceneAfterDelay(3f));
    }
    private IEnumerator LoadNextSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(nextSceneName);
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}