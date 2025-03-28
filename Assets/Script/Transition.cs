using UnityEngine;
using UnityEngine.SceneManagement;

public class Transition : MonoBehaviour
{
    public void OnAnimationEnd()
    {
        SceneManager.LoadScene("Game");
    }
}
