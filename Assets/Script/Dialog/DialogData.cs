// DialogLine.cs
using UnityEngine;
using UnityEngine.Events; // << เพิ่มเข้ามาเพื่อใช้ UnityEvent

[System.Serializable]
public struct DialogLine
{
    [Tooltip("NPC ที่จะพูดบรรทัดนี้")]
    public NPCDialog speaker;

    [Tooltip("ข้อความที่จะแสดง")]
    [TextArea(2, 5)]
    public string sentence;

    [Header("Optional Effects & Events")]
    [Tooltip("(Optional) ชื่อ Trigger ใน Animator ของ Speaker ที่จะให้เล่นเมื่อเริ่มบรรทัดนี้")]
    public string animationTrigger;

    [Tooltip("(Optional) เสียงที่จะเล่นเฉพาะเมื่อเริ่มบรรทัดนี้")]
    public AudioClip lineSpecificSound;

    [Tooltip("(Optional) Event ที่จะถูกเรียก *ทันที* เมื่อเริ่มแสดงบรรทัดนี้")] // << NEW
    public UnityEvent onLineStartEvent; // << เพิ่ม UnityEvent เข้ามา
}