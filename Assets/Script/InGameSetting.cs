using UnityEngine;

public class InGameSetting : MonoBehaviour
{
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    public void RestartGame()
    {
        // โหลด Scene ปัจจุบันใหม่
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);
    }
    public void LoadMainMenu()
    {
        // โหลด Scene ที่ชื่อว่า "MainMenu"
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
