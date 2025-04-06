// NPCDialog.cs modifications
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class NPCDialog : MonoBehaviour
{
    // ... (Header attributes and variables remain the same) ...
    [Header("Dialog Settings")]
    [Tooltip("Sentences this NPC says if NOT starting a multi-speaker conversation.")]
    [TextArea(3, 10)]
    public string[] sentences;

    [Header("Multi-Speaker Conversation (Optional)")]
    [Tooltip("Check this if interacting with this NPC should trigger the sequence below.")]
    public bool isConversationStarter = false;
    [Tooltip("The sequence of lines involving multiple speakers.")]
    public DialogLine[] conversationSequence;

    [Header("World Space Dialog Bubble")]
    public GameObject dialogBubbleObject;
    public TextMeshProUGUI worldDialogText;

    [Header("Events")]
    [Tooltip("Called when this NPC finishes its SINGLE dialogue OR when a multi-speaker conversation started by this NPC finishes.")]
    public UnityEvent onDialogComplete;

    private bool playerInRange = false;

    // ... (Start method remains the same) ...
    void Start()
    {
        if (dialogBubbleObject != null)
            dialogBubbleObject.SetActive(false);
        if (isConversationStarter)
        {
            for(int i = 0; i < conversationSequence.Length; i++)
            {
                if (conversationSequence[i].speaker == null) { Debug.LogError($"NPC '{gameObject.name}': Conversation Sequence line {i+1} is missing a Speaker reference!", gameObject); }
                else if (conversationSequence[i].speaker.worldDialogText == null || conversationSequence[i].speaker.dialogBubbleObject == null) { Debug.LogError($"NPC '{gameObject.name}': Speaker '{conversationSequence[i].speaker.gameObject.name}' in Conversation Sequence line {i+1} is missing bubble/text references!", conversationSequence[i].speaker.gameObject); }
            }
        }
        if (worldDialogText == null || dialogBubbleObject == null) { Debug.LogError($"NPC '{gameObject.name}' is missing Dialog Bubble Object or World Dialog Text reference!", gameObject); }
    }


    void Update()
    {
        // If player is in range, presses E, AND dialog is NOT currently active
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !DialogManager.instance.IsDialogActive()) // <<<< CHANGED HERE
        {
            // Prioritize starting the multi-speaker conversation if configured
            if (isConversationStarter && conversationSequence != null && conversationSequence.Length > 0)
            {
                DialogManager.instance.StartConversation(conversationSequence, onDialogComplete);
            }
            // Fallback to the NPC's own simple dialog
            else if (sentences != null && sentences.Length > 0)
            {
                DialogManager.instance.StartDialog(sentences, this, onDialogComplete);
            }
        }
    }

    // ... (OnTriggerEnter2D, OnTriggerExit2D, SetBubbleActive, GetWorldDialogTextComponent, IsPlayerInRange methods remain the same) ...
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log($"Player entered range of {gameObject.name}. IsStarter: {isConversationStarter}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
             Debug.Log($"Player exited range of {gameObject.name}");
        }
    }

    public void SetBubbleActive(bool isActive)
    {
        if (dialogBubbleObject != null)
        {
            dialogBubbleObject.SetActive(isActive);
        }
    }

    public TextMeshProUGUI GetWorldDialogTextComponent()
    {
        return worldDialogText;
    }

     public bool IsPlayerInRange()
    {
        return playerInRange;
    }
    // --------------------------------
}

// Don't forget the DialogLine struct if it's not in its own file
// [System.Serializable]
// public struct DialogLine
// {
//     public NPCDialog speaker;
//     [TextArea(3, 5)]
//     public string sentence;
// }