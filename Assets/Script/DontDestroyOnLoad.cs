using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{
    void OnEnable()
    {
        DontDestroyOnLoad(gameObject);
    }

    //if object is already in the scene, destroy it
    void Awake()
    {
        if (FindObjectsByType<DontDestroyOnLoad>(FindObjectsSortMode.InstanceID).Length > 1)
        {
            Destroy(gameObject);
        }
    }
}
