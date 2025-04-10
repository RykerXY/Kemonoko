using UnityEngine;
using System.Collections.Generic; // ต้องใช้สำหรับ List

public class RandomPrefabSpawner : MonoBehaviour
{
    [Header("Prefabs & Spawn Settings")]
    [Tooltip("ลาก Prefabs ทั้งหมดที่ต้องการสุ่ม Spawn มาใส่ที่นี่")]
    public List<GameObject> prefabsToSpawn;

    [Tooltip("จำนวน Object ที่ต้องการ Spawn ทั้งหมด")]
    public int numberOfObjectsToSpawn = 10;

    [Header("Spawn Area")]
    [Tooltip("ความกว้างของพื้นที่สุ่ม Spawn (แกน X)")]
    public float spawnAreaWidth = 10f;
    [Tooltip("ความสูงของพื้นที่สุ่ม Spawn (แกน Y)")]
    public float spawnAreaHeight = 5f;
    [Tooltip("ตำแหน่งศูนย์กลางของพื้นที่ Spawn (ปกติคือตำแหน่งของ GameObject นี้เอง)")]
    public Vector2 spawnAreaCenterOffset = Vector2.zero; // Offset จากตำแหน่งของ Spawner

    [Header("Overlap Prevention")]
    [Tooltip("รัศมีขั้นต่ำที่ใช้ตรวจสอบการซ้อนทับ (ปรับค่านี้ให้เหมาะสมกับขนาด Prefab)")]
    public float minSeparationRadius = 0.5f;
    [Tooltip("จำนวนครั้งสูงสุดที่จะพยายามหาตำแหน่งใหม่ หากเกิดการซ้อนทับ")]
    public int maxSpawnAttempts = 20;
    [Tooltip("Layer ที่จะตรวจสอบการชน (ควรตั้งค่าให้ตรงกับ Layer ของ Object ที่ Spawn หรือสิ่งกีดขวางอื่นๆ)")]
    public LayerMask collisionCheckLayers; // สำคัญ: ตั้งค่า LayerMask ใน Inspector!

    [Header("Organization")]
    [Tooltip("ติ๊กถูกเพื่อจัดเก็บ Object ที่ Spawn ไว้ภายใต้ GameObject นี้")]
    public bool parentToSpawner = true;

    // เก็บรายการ Object ที่ Spawn สำเร็จ (เผื่อใช้อ้างอิงภายหลัง)
    private List<GameObject> spawnedObjects = new List<GameObject>();

    void Start()
    {
        // --- ตรวจสอบค่าเริ่มต้น ---
        if (prefabsToSpawn == null || prefabsToSpawn.Count == 0)
        {
            Debug.LogError("Prefabs to Spawn list is empty! Cannot spawn anything.", this);
            return;
        }
        // ลบ Prefab ที่เป็น null ออกจาก List (ถ้ามี)
        prefabsToSpawn.RemoveAll(item => item == null);
        if (prefabsToSpawn.Count == 0)
        {
             Debug.LogError("All prefabs in the list were null! Cannot spawn anything.", this);
             return;
        }

        if (numberOfObjectsToSpawn <= 0)
        {
            Debug.LogWarning("Number of objects to spawn is zero or negative. No objects will be spawned.", this);
            return;
        }

        // --- เริ่มกระบวนการ Spawn ---
        SpawnObjects();
    }

    void SpawnObjects()
    {
        Vector3 spawnerPosition = transform.position;
        int spawnedCount = 0;

        for (int i = 0; i < numberOfObjectsToSpawn; i++)
        {
            int currentAttempt = 0;
            bool positionFound = false;

            // เลือก Prefab แบบสุ่มจาก List
            GameObject prefabToInstantiate = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Count)];
            if (prefabToInstantiate == null) // เช็คอีกครั้งเผื่อกรณีแปลกๆ
            {
                 Debug.LogWarning($"Skipping spawn attempt {i+1} because selected prefab was null.");
                 continue; // ข้ามไป Spawn ตัวต่อไป
            }


            // --- พยายามหาตำแหน่งที่ไม่ซ้อนทับ ---
            while (!positionFound && currentAttempt < maxSpawnAttempts)
            {
                currentAttempt++;

                // คำนวณตำแหน่งสุ่มภายในพื้นที่
                float randomX = Random.Range(
                    spawnerPosition.x + spawnAreaCenterOffset.x - spawnAreaWidth / 2f,
                    spawnerPosition.x + spawnAreaCenterOffset.x + spawnAreaWidth / 2f
                );
                float randomY = Random.Range(
                    spawnerPosition.y + spawnAreaCenterOffset.y - spawnAreaHeight / 2f,
                    spawnerPosition.y + spawnAreaCenterOffset.y + spawnAreaHeight / 2f
                );
                Vector3 spawnPosition = new Vector3(randomX, randomY, spawnerPosition.z); // ใช้ Z ของ Spawner

                // --- ตรวจสอบการซ้อนทับ ---
                // ใช้ Physics2D.OverlapCircle ตรวจสอบว่ามี Collider อื่นในรัศมีที่กำหนดหรือไม่
                // ใช้ LayerMask ที่กำหนดเพื่อประสิทธิภาพและความแม่นยำ
                Collider2D overlap = Physics2D.OverlapCircle(spawnPosition, minSeparationRadius, collisionCheckLayers);

                if (overlap == null)
                {
                    // ไม่มีอะไรซ้อนทับ ณ ตำแหน่งนี้
                    positionFound = true;

                    // สร้าง Object จาก Prefab
                    GameObject newObject = Instantiate(prefabToInstantiate, spawnPosition, Quaternion.identity);
                    spawnedCount++;
                    spawnedObjects.Add(newObject); // เพิ่มใน List ที่เก็บไว้

                    // ตั้งค่า Parent (ถ้าต้องการ)
                    if (parentToSpawner)
                    {
                        newObject.transform.SetParent(this.transform);
                    }

                    // Debug.Log($"Successfully spawned '{prefabToInstantiate.name}' at {spawnPosition}");
                }
                // ถ้า overlap != null แสดงว่ามี Object อื่นอยู่แล้ว -> วน Loop ลองหาตำแหน่งใหม่
            }

            // --- หากพยายามครบตามจำนวนครั้งแล้วยังหาตำแหน่งไม่ได้ ---
            if (!positionFound)
            {
                Debug.LogWarning($"Failed to find a non-overlapping position for object {i + 1} ('{prefabToInstantiate.name}') after {maxSpawnAttempts} attempts. Check spawn area size, object density, minSeparationRadius, and collision layers.", this);
                // อาจจะหยุด Spawn ทั้งหมด หรือแค่ข้ามตัวนี้ไป (ปัจจุบันคือข้ามไป)
            }
        }

        Debug.Log($"Spawn process complete. Successfully spawned {spawnedCount}/{numberOfObjectsToSpawn} objects.", this);
    }

    // --- แสดงขอบเขตการ Spawn ใน Scene View (เพื่อความสะดวกในการตั้งค่า) ---
    void OnDrawGizmosSelected()
    {
        Vector3 center = (Vector3)spawnAreaCenterOffset + transform.position;
        Vector3 size = new Vector3(spawnAreaWidth, spawnAreaHeight, 0.1f); // ให้มีความหนาเล็กน้อยในแกน Z

        Gizmos.color = Color.cyan; // เลือกสีที่ต้องการ
        Gizmos.DrawWireCube(center, size);

        // (ตัวเลือกเสริม) วาดรัศมี Separation รอบๆ Spawner เพื่อให้เห็นภาพคร่าวๆ
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minSeparationRadius);
    }
}