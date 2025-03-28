using UnityEngine;
public class MainMenu : MonoBehaviour
{
    public Animator anim;
    public void QuteGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }   
    
    public void StartGame()
    {
        anim.SetBool("isStart", true);
        Debug.Log("Start Game");
    }
}
