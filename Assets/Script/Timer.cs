using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public bool OreMiniGames = false;
    public bool VeggieMiniGames = false; // Flag for veggie mini-game
    public float timeLimit = 60f; // Time limit in seconds
    private float timeRemaining;
    public bool isTimerRunning = false;
    public TextMeshProUGUI timerText; // Reference to the UI Text component
    public GameObject[] objectsToDestroy; // List of objects to destroy after time limit
    public Animator anim; // Reference to the Animator component
    public GlobalPoint globalPoint; // Reference to the GlobalPoint script

    void Start()
    {
        timeRemaining = timeLimit;
        isTimerRunning = true;
    }

    void Update()
    {
        TimerUpdate();
        OreMiniGame();
        VeggieMiniGame();
    }

    void OreMiniGame()
    {
        if (OreMiniGames == true)
        {
            if (globalPoint.orePoints >= 4)
            {
                timeRemaining = 0;
            }
        }
    }
    void VeggieMiniGame()
    {
        if (VeggieMiniGames == true)
        {
            timerText.enabled = false;
            isTimerRunning = false;
            GameObject[] veggies = GameObject.FindGameObjectsWithTag("Veggie");
            if (veggies.Length == 0)
            {
                timeRemaining = 0;
                isTimerRunning = true;
            }
        }
    }

    void TimerUpdate()
    {
        timerText.text = "Time: " + Mathf.Ceil(timeRemaining).ToString();
        if (isTimerRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                isTimerRunning = false;
                Debug.Log("Time's up!");
                if(objectsToDestroy != null)
                {
                    foreach (GameObject obj in objectsToDestroy)
                    {
                        Destroy(obj);
                    }
                }
                anim.SetBool("End", true);
            }
        }
    }
}
