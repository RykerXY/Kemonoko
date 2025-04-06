// DialogData.cs (or inside another script)
using UnityEngine;

[System.Serializable] // Makes it show up in the Inspector
public struct DialogLine
{
    public NPCDialog speaker; // Reference to the NPCDialog script on the speaker
    [TextArea(3, 5)]
    public string sentence;   // The line of dialog
}