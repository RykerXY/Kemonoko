using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float timeLimit = 60f; // Time limit in seconds
    private float timeRemaining;
    public bool isTimerRunning = false;
    public TextMeshProUGUI timerText; // Reference to the UI Text component
    public GameObject[] objectsToDestroy; // List of objects to destroy after time limit
    public Animator anim; // Reference to the Animator component

    void Start()
    {
        timeRemaining = timeLimit;
        isTimerRunning = true;
    }

    void Update()
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
