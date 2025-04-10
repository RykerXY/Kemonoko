// GlobalPoint.cs (แก้ไข)
using TMPro; // << เอาออกได้ถ้าไม่ใช้ที่นี่แล้ว
using UnityEngine;

public class GlobalPoint : MonoBehaviour
{
    public static GlobalPoint instance { get; private set; }

    [Header("Points")]
    public int karmaPoints = 0;
    public int flowerPoints = 0;
    public int fireflyPoints = 0;
    public int orePoints = 0;
    public int veggiePoints = 0;

    [Header("Gold")]
    public int gold = 0;

    private bool firefly = false;
    private bool flower = false;
    private bool ore = false;
    private bool veggie = false;

    private bool isGoodEnding = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return; 
        }
    }

    void Update()
    {
        UpdateKarma();
        if(karmaPoints >= 2 && !isGoodEnding)
        {
            isGoodEnding = true;
            Debug.Log("Good Ending Achieved!");
        }
    }

    void UpdateKarma() 
    {
        if (flowerPoints >= 60)
        {
            if(!flower) // เช็คว่าเพิ่มแล้วหรือยัง
            {
                flower = true; // ตั้งค่าให้เพิ่มแล้ว
                karmaPoints += 1;
            }
            flowerPoints = 0; // รีเซ็ต Flower
            Debug.Log("Karma increased from Flowers. New Karma: " + karmaPoints);
        }
        if (fireflyPoints >= 60)
        {
            if(!firefly) // เช็คว่าเพิ่มแล้วหรือยัง
            {
                firefly = true; // ตั้งค่าให้เพิ่มแล้ว
                karmaPoints += 1;
            }
            fireflyPoints = 0; // รีเซ็ต Firefly
            Debug.Log("Karma increased from Fireflies. New Karma: " + karmaPoints);
        }
        if (orePoints >= 32)
        {
            if(!ore) // เช็คว่าเพิ่มแล้วหรือยัง
            {
                ore = true; // ตั้งค่าให้เพิ่มแล้ว
                karmaPoints += 1;
            }
            orePoints = 0; // รีเซ็ต Ore
            Debug.Log("Karma increased from Ores. New Karma: " + karmaPoints);
        }
        if (veggiePoints >= 20)
        {
            if(!veggie) // เช็คว่าเพิ่มแล้วหรือยัง
            {
                veggie = true; // ตั้งค่าให้เพิ่มแล้ว
                karmaPoints += 1;
            }
            veggiePoints = 0; // รีเซ็ต Veggie
            Debug.Log("Karma increased from Veggies. New Karma: " + karmaPoints);
        }
    }

    public void AddPoints(string type, int amount)
    {
        switch (type.ToLower())
        {
            case "flower": flowerPoints += amount; break;
            case "firefly": fireflyPoints += amount; break;
            case "ore": orePoints += amount; break;
            case "veggie": veggiePoints += amount; break;
            default: Debug.LogWarning($"Unknown point type: {type}"); break;
        }
    }
    public void AddGold(int amount)
    {
        gold += amount;
    }
}