using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Rigidbody2D))]
public class SetStartPositionWithCinemachine : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("ลาก GameObject ที่ต้องการใช้ตำแหน่ง Transform เป็นจุดเริ่มต้นมาใส่ที่นี่")]
    public Transform startPositionMarker;

    [Tooltip("ลาก Cinemachine Virtual Camera ที่ตาม Player ตัวนี้มาใส่ที่นี่")]
    public CinemachineCamera playerVirtualCamera;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError($"[{gameObject.name}] ไม่พบ Component Rigidbody2D!", this);
        }
    }

    void Start()
    {
        if (rb == null)
        {
            Debug.LogError($"[{gameObject.name}] Rigidbody2D is missing, cannot set start position.", this);
            return;
        }
        if (startPositionMarker == null)
        {
            Debug.LogError($"[{gameObject.name}] กรุณาลาก GameObject ที่เป็นจุดเริ่มต้นมาใส่ในช่อง 'Start Position Marker'!", this);
            return;
        }
        if (playerVirtualCamera == null)
        {
            Debug.LogError($"[{gameObject.name}] กรุณาลาก Cinemachine Virtual Camera มาใส่ในช่อง 'Player Virtual Camera'!", this);
            return;
        }

        Vector2 previousPosition = rb.position;
        Vector2 targetPosition = startPositionMarker.position;

        rb.position = targetPosition;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        Debug.Log($"[{gameObject.name}] ตั้งค่าตำแหน่ง Player เป็น {targetPosition}");

        Vector3 positionDelta = (Vector3)targetPosition - (Vector3)previousPosition;

         if (playerVirtualCamera.Follow == this.transform)
         {
            playerVirtualCamera.OnTargetObjectWarped(transform, positionDelta);
            Debug.Log($"[{gameObject.name}] แจ้ง VCam ({playerVirtualCamera.name}) ให้วาร์ปตาม Player เรียบร้อยแล้ว.");
        }
        else
        {
             Debug.LogWarning($"[{gameObject.name}] VCam ({playerVirtualCamera.name}) ไม่ได้ Follow Player ({this.gameObject.name}) โดยตรง หรือยังไม่ได้ตั้งค่า Follow target. ไม่ได้เรียก OnTargetObjectWarped.", playerVirtualCamera);
        }
    }
}