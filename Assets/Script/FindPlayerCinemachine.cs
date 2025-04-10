using UnityEngine;
using Unity.Cinemachine;

public class FindPlayerCinemachine : MonoBehaviour
{
    private CinemachineCamera cinemachineCamera;
    void Start()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();
        if (cinemachineCamera == null)
        {
            Debug.LogError("CinemachineCamera component not found on this GameObject.");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player GameObject not found. Make sure it has the 'Player' tag.");
            return;
        }
        else
        {
            cinemachineCamera.Follow = player.transform;
            Debug.Log("Player GameObject found: " + player.name);
        }
    }
}
