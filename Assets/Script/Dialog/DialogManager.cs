// DialogManager.cs modifications
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class DialogManager : MonoBehaviour
{
    public static DialogManager instance;

    // --- REMOVE THIS LINE ---
    // [Header("Dialog Flow")]
    // public KeyCode continueKey = KeyCode.E;

    [Header("Player Control")]
    public PlayerController PlayerController;

    // ... (rest of the variables: queues, state, references) ...
    private Queue<string> singleNpcSentences;
    private Queue<DialogLine> conversationLines;
    private bool isDialogActive = false;
    private enum DialogMode { None, SingleNPC, Conversation }
    private DialogMode currentMode = DialogMode.None;
    private NPCDialog currentSingleNpcScript;
    private NPCDialog lastSpeakerNpcScript;
    private UnityEvent currentDialogCompleteEvent;


    void Awake()
    {
        if (instance == null) { instance = this; }
        else if (instance != this) { Destroy(gameObject); return; }

        singleNpcSentences = new Queue<string>();
        conversationLines = new Queue<DialogLine>();
    }

    void Start()
    {
        // Find PlayerController
        if (PlayerController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) { PlayerController = player.GetComponent<PlayerController>(); }
            if (PlayerController == null) { Debug.LogError("DialogManager: PlayerController component not found on Player!"); }
        }
    }

    void Update()
    {
         // If Dialog is active and the LEFT MOUSE BUTTON is pressed down this frame
         if (isDialogActive && Input.GetMouseButtonDown(0)) // <<<< CHANGED HERE
         {
             // Call the appropriate function based on the current mode
             if (currentMode == DialogMode.SingleNPC)
             {
                 DisplayNextSentence();
             }
             else if (currentMode == DialogMode.Conversation)
             {
                 DisplayNextConversationLine();
             }
         }
     }

    // ... (StartDialog, StartConversation, DisplayNextSentence, DisplayNextConversationLine, EndDialog, SetPlayerMovement, IsDialogActive methods remain the same) ...

    // --- Keep these methods ---
    public void StartDialog(string[] npcSentences, NPCDialog npcScript, UnityEvent onCompleteEvent = null)
    {
        if (isDialogActive || npcScript == null) return;
        if (npcScript.dialogBubbleObject == null || npcScript.worldDialogText == null)
        {
            Debug.LogError($"NPC '{npcScript.gameObject.name}' is missing Dialog Bubble/Text reference!", npcScript.gameObject);
            return;
        }

        Debug.Log($"Starting single dialog with {npcScript.gameObject.name}");
        isDialogActive = true;
        currentMode = DialogMode.SingleNPC;
        currentSingleNpcScript = npcScript;
        currentDialogCompleteEvent = onCompleteEvent;

        SetPlayerMovement(false);

        singleNpcSentences.Clear();
        foreach (string sentence in npcSentences) { singleNpcSentences.Enqueue(sentence); }

        currentSingleNpcScript.SetBubbleActive(true);
        lastSpeakerNpcScript = currentSingleNpcScript;

        DisplayNextSentence();
    }

    public void StartConversation(DialogLine[] lines, UnityEvent onCompleteEvent = null)
    {
        if (isDialogActive || lines == null || lines.Length == 0) return;

        Debug.Log("Starting multi-NPC conversation.");
        isDialogActive = true;
        currentMode = DialogMode.Conversation;
        currentDialogCompleteEvent = onCompleteEvent;
        lastSpeakerNpcScript = null;
        currentSingleNpcScript = null;

        SetPlayerMovement(false);

        conversationLines.Clear();
        foreach (DialogLine line in lines)
        {
            if (line.speaker != null)
            {
                conversationLines.Enqueue(line);
            }
            else
            {
                 Debug.LogWarning("Skipping conversation line with null speaker.");
            }
        }

        if (conversationLines.Count == 0)
        {
            Debug.LogWarning("Conversation sequence has no valid lines. Ending immediately.");
            EndDialog();
            return;
        }

        DisplayNextConversationLine();
    }

    void DisplayNextSentence()
    {
        if (singleNpcSentences.Count == 0) {
            EndDialog();
            return;
        }
        string sentence = singleNpcSentences.Dequeue();
        if (currentSingleNpcScript != null && currentSingleNpcScript.worldDialogText != null)
        {
             currentSingleNpcScript.worldDialogText.text = sentence;
        }
        Debug.Log($"SingleNPC - {currentSingleNpcScript.gameObject.name}: {sentence}");
    }

    void DisplayNextConversationLine()
    {
         if (conversationLines.Count == 0) {
            EndDialog();
            return;
        }
        DialogLine currentLine = conversationLines.Dequeue();
        NPCDialog currentSpeaker = currentLine.speaker;
        if (currentSpeaker == null || currentSpeaker.dialogBubbleObject == null || currentSpeaker.worldDialogText == null)
        {
             Debug.LogError($"Conversation Error: Speaker '{currentSpeaker?.gameObject.name ?? "NULL"}' or its bubble/text is invalid. Skipping line.");
             DisplayNextConversationLine();
             return;
        }
        if (lastSpeakerNpcScript != null && lastSpeakerNpcScript != currentSpeaker)
        {
             lastSpeakerNpcScript.SetBubbleActive(false);
        }
        currentSpeaker.SetBubbleActive(true);
        currentSpeaker.worldDialogText.text = currentLine.sentence;
        lastSpeakerNpcScript = currentSpeaker;
        Debug.Log($"Conversation - {currentSpeaker.gameObject.name}: {currentLine.sentence}");
    }

    public void EndDialog()
    {
        if (!isDialogActive) return;
        Debug.Log("Ending dialog/conversation.");
        isDialogActive = false;
        if (lastSpeakerNpcScript != null)
        {
            lastSpeakerNpcScript.SetBubbleActive(false);
             Debug.Log($"Hiding bubble for last speaker: {lastSpeakerNpcScript.gameObject.name}");
        }
        SetPlayerMovement(true);
        currentDialogCompleteEvent?.Invoke();
        currentMode = DialogMode.None;
        currentSingleNpcScript = null;
        lastSpeakerNpcScript = null;
        currentDialogCompleteEvent = null;
        singleNpcSentences.Clear();
        conversationLines.Clear();
    }

    private void SetPlayerMovement(bool canMove)
    {
         if (PlayerController != null)
         {
             PlayerController.canMove = canMove;
         }
         else {
              Debug.LogWarning("DialogManager: PlayerController reference is null, cannot control movement.");
         }
    }

    public bool IsDialogActive() { return isDialogActive; }
    // ---------------------------
}