using UnityEngine;
using UnityEngine.SceneManagement;

public class Transition : MonoBehaviour
{
    public string sceneToLoad = "MainMenu";
    public void OnAnimationEnd()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
