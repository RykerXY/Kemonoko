// DialogManager.cs
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class DialogManager : MonoBehaviour
{
    // ... (ส่วนอื่นๆ เหมือนเดิม: instance, Header Attributes, Internal State, Awake, Start, Update) ...
    public static DialogManager instance;

    [Header("Typewriter Effect & Sounds")]
    [SerializeField] private float typingSpeed = 0.04f;
    [SerializeField] private AudioClip defaultTypingSound;
    [SerializeField] private AudioClip sentenceCompleteSound;

    [Header("Player Control Reference")]
    public PlayerController playerController;

    private AudioSource audioSource;
    private Queue<string> currentSingleNpcSentences;
    private Queue<DialogLine> currentConversationLines;
    private bool isDialogActive = false;
    private enum DialogMode { None, SingleNPC, Conversation }
    private DialogMode currentMode = DialogMode.None;
    private NPCDialog currentNpcForSingleDialog;
    private NPCDialog currentConversationStarterNpc;
    private NPCDialog lastSpeakerNpc;
    private UnityEvent currentDialogCompleteEvent;
    private Coroutine currentTypingCoroutine = null;
    private bool isTyping = false;
    private string fullSentenceToDisplay = "";

    void Awake()
    {
        if (instance == null) { instance = this; }
        else if (instance != this) { Destroy(gameObject); return; }
        audioSource = GetComponent<AudioSource>();
        currentSingleNpcSentences = new Queue<string>();
        currentConversationLines = new Queue<DialogLine>();
    }

    void Start()
    {
        if (playerController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) { playerController = player.GetComponent<PlayerController>(); }
            if (playerController == null) { Debug.LogError("DialogManager: PlayerController not found on Player!"); }
        }
    }

    void Update()
    {
        if (isDialogActive && Input.GetMouseButtonDown(0))
        {
            HandleInteractionInput();
        }
    }

    private void HandleInteractionInput()
    {
        if (isTyping)
        {
            SkipTyping();
        }
        else
        {
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


    // --- Public Methods to Start Dialog ---
    public void StartDialog(string[] npcSentences, NPCDialog speakerNpc, UnityEvent onCompleteEvent = null)
    {
        if (isDialogActive || speakerNpc == null || npcSentences == null || npcSentences.Length == 0) return;
        if (!ValidateNpcReferences(speakerNpc)) return;

        SetupDialogStart(DialogMode.SingleNPC, onCompleteEvent);
        currentNpcForSingleDialog = speakerNpc;
        currentConversationStarterNpc = speakerNpc;

        currentSingleNpcSentences.Clear();
        foreach (string sentence in npcSentences) { currentSingleNpcSentences.Enqueue(sentence); }

        DisplayNextSentence();
    }

    public void StartConversation(DialogLine[] lines, NPCDialog starterNpc, UnityEvent onCompleteEvent = null)
    {
        if (isDialogActive || starterNpc == null || lines == null || lines.Length == 0) return;

        SetupDialogStart(DialogMode.Conversation, onCompleteEvent);
        currentConversationStarterNpc = starterNpc;
        currentNpcForSingleDialog = null;

        currentConversationLines.Clear();
        foreach (DialogLine line in lines)
        {
            if (ValidateDialogLine(line))
            {
                currentConversationLines.Enqueue(line);
            }
        }

        if (currentConversationLines.Count == 0)
        {
            Debug.LogWarning("Conversation sequence has no valid lines after validation. Ending immediately.");
            EndDialog();
            return;
        }

        DisplayNextConversationLine();
    }

    // --- Core Display Logic ---

    private void DisplayNextSentence()
    {
        StopCurrentTypingCoroutine();

        if (currentSingleNpcSentences.Count == 0)
        {
            EndDialog();
            return;
        }

        string sentence = currentSingleNpcSentences.Dequeue();
        // สำหรับ SingleNPC, ไม่มี Line-Specific Event, Anim, Sound
        ProcessAndDisplayLine(currentNpcForSingleDialog, sentence, null, null, null); // << ส่ง null สำหรับ event
    }

    private void DisplayNextConversationLine()
    {
        StopCurrentTypingCoroutine();

        if (currentConversationLines.Count == 0)
        {
            EndDialog();
            return;
        }

        DialogLine line = currentConversationLines.Dequeue();
        // ส่งข้อมูลทั้งหมดจาก DialogLine รวมถึง Event ไปด้วย
        ProcessAndDisplayLine(line.speaker, line.sentence, line.animationTrigger, line.lineSpecificSound, line.onLineStartEvent); // << ส่ง event จาก line
    }

    // ปรับปรุงให้รับ UnityEvent เข้ามาด้วย
    private void ProcessAndDisplayLine(NPCDialog speaker, string sentence, string animTrigger, AudioClip lineSound, UnityEvent lineEvent) // << เพิ่ม lineEvent parameter
    {
        if (speaker == null)
        {
            Debug.LogError("ProcessAndDisplayLine Error: Speaker is null!");
            // Attempt to advance
            if(currentMode == DialogMode.Conversation) DisplayNextConversationLine();
            else EndDialog();
            return;
        }

        // --- Invoke Line-Specific Event (ทำก่อน effect อื่นๆ) ---  << NEW LOGIC
        if (lineEvent != null)
        {
            try
            {
                lineEvent.Invoke();
                // Debug.Log($"Invoked line-specific event for speaker: {speaker.gameObject.name}"); // Optional log
            }
            catch (System.Exception e)
            {
                 Debug.LogError($"Error invoking line-specific event for speaker '{speaker.gameObject.name}': {e.Message}\n{e.StackTrace}", speaker.gameObject);
            }
        }

        // --- Switch Speaker Bubble ---
        if (lastSpeakerNpc != null && lastSpeakerNpc != speaker)
        {
            lastSpeakerNpc.SetBubbleActive(false);
        }
        speaker.SetBubbleActive(true);
        lastSpeakerNpc = speaker;

        // --- Trigger Animation ---
        if (!string.IsNullOrEmpty(animTrigger))
        {
            Animator speakerAnim = speaker.GetSpeakerAnimator();
            if (speakerAnim != null)
            {
                speakerAnim.SetTrigger(animTrigger);
            }
            else
            {
                Debug.LogWarning($"DialogManager: NPC '{speaker.gameObject.name}' has animation trigger '{animTrigger}' but no Animator component assigned!", speaker.gameObject);
            }
        }

        // --- Play Line-Specific Sound ---
        if (lineSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(lineSound);
        }

        // --- Start Typewriter Effect ---
        fullSentenceToDisplay = sentence;
        TextMeshProUGUI targetText = speaker.GetWorldDialogTextComponent();
        if (targetText != null)
        {
            currentTypingCoroutine = StartCoroutine(TypeSentenceCoroutine(fullSentenceToDisplay, targetText));
        } else {
            Debug.LogError($"DialogManager: NPC '{speaker.gameObject.name}' is missing World Dialog Text component!", speaker.gameObject);
            // Attempt to advance
             if(currentMode == DialogMode.Conversation) DisplayNextConversationLine();
             else EndDialog();
        }
    }


    // ... (Typewriter Coroutine & Helpers: TypeSentenceCoroutine, SkipTyping, TypingFinished, StopCurrentTypingCoroutine เหมือนเดิม) ...
     IEnumerator TypeSentenceCoroutine(string sentence, TextMeshProUGUI targetText)
    {
        isTyping = true;
        targetText.text = ""; // Clear previous text

        foreach (char letter in sentence.ToCharArray())
        {
            targetText.text += letter;

            // Play default typing sound if available
            if (defaultTypingSound != null && audioSource != null && letter != ' ') // Don't play sound for spaces
            {
                audioSource.PlayOneShot(defaultTypingSound);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        TypingFinished(); // Call when typing naturally completes
    }

    private void SkipTyping()
    {
        if (!isTyping) return;

        StopCurrentTypingCoroutine(); // Stop the coroutine

        // Immediately display the full sentence on the last speaker's text component
        if (lastSpeakerNpc != null)
        {
             TextMeshProUGUI targetText = lastSpeakerNpc.GetWorldDialogTextComponent();
             if(targetText != null) {
                 targetText.text = fullSentenceToDisplay;
             } else {
                  Debug.LogError("SkipTyping Error: Last speaker's text component is null!");
             }
        } else {
             Debug.LogError("SkipTyping Error: Last speaker reference is null!");
        }


        // Play completion sound (optional)
        if (sentenceCompleteSound != null && audioSource != null)
        {
            // Optional: Stop any lingering typing sounds first
            // audioSource.Stop();
            audioSource.PlayOneShot(sentenceCompleteSound);
        }

        TypingFinished(); // Ensure state is updated correctly
        // Debug.Log("Typing skipped.");
    }

     private void TypingFinished() {
        isTyping = false;
        // currentTypingCoroutine is already stopped or finished naturally
        currentTypingCoroutine = null; // Clear the reference
        // Debug.Log("Typing finished.");
        // Now waiting for player input to advance
    }


    private void StopCurrentTypingCoroutine()
    {
        if (currentTypingCoroutine != null)
        {
            StopCoroutine(currentTypingCoroutine);
            currentTypingCoroutine = null;
            isTyping = false; // Ensure typing state is reset
            // Debug.Log("Stopped previous typing coroutine."); // Optional log
        }
         // Make sure isTyping is false even if coroutine was null
         isTyping = false;
    }

    // ... (Ending Dialog: EndDialog เหมือนเดิม แต่ Logic การ MarkFirstInteractionComplete และ Invoke currentDialogCompleteEvent ยังอยู่) ...
    public void EndDialog()
    {
        if (!isDialogActive) return;
        // Debug.Log("Ending dialog.");

        StopCurrentTypingCoroutine(); // Ensure typing stops

        isDialogActive = false;

        // Hide the last speaker's bubble
        if (lastSpeakerNpc != null)
        {
            lastSpeakerNpc.SetBubbleActive(false);
        }

        SetPlayerMovement(true); // Give control back to player

        // Mark the FIRST interaction as complete for the NPC who STARTED the dialog
        if (currentConversationStarterNpc != null)
        {
             currentConversationStarterNpc.MarkFirstInteractionComplete();
        }


        currentDialogCompleteEvent?.Invoke(); // Trigger the completion event

        // Reset internal state
        currentMode = DialogMode.None;
        currentNpcForSingleDialog = null;
        currentConversationStarterNpc = null;
        lastSpeakerNpc = null;
        currentDialogCompleteEvent = null;
        currentSingleNpcSentences.Clear();
        currentConversationLines.Clear();
        fullSentenceToDisplay = "";
    }

    // ... (Utility and Setup: SetupDialogStart, SetPlayerMovement, IsDialogActive, Validation Helpers เหมือนเดิม) ...
      private void SetupDialogStart(DialogMode mode, UnityEvent onCompleteEvent)
    {
         // Debug.Log($"Setting up Dialog Start. Mode: {mode}");
        isDialogActive = true;
        currentMode = mode;
        currentDialogCompleteEvent = onCompleteEvent;
        lastSpeakerNpc = null; // Reset last speaker
        StopCurrentTypingCoroutine(); // Ensure no previous typing is running
        SetPlayerMovement(false); // Disable player movement
    }

    private void SetPlayerMovement(bool canMove)
    {
        if (playerController != null)
        {
            // Safety check if Player GameObject got destroyed somehow
            if (playerController.gameObject != null)
            {
                playerController.canMove = canMove;
            }
            else
            {
                 Debug.LogWarning("DialogManager: PlayerController GameObject seems to have been destroyed.");
                 playerController = null; // Clear invalid reference
            }
        }
        else {
             Debug.LogWarning("DialogManager: PlayerController reference is null. Cannot control movement.");
        }
    }

    public bool IsDialogActive()
    {
        return isDialogActive;
    }

     // --- Validation Helpers ---

     private bool ValidateNpcReferences(NPCDialog npc) {
         if (npc.dialogBubbleObject == null) {
             Debug.LogError($"Validation Error: NPC '{npc.gameObject.name}' is missing Dialog Bubble Object!", npc.gameObject);
             return false;
         }
         if (npc.worldDialogText == null) {
              Debug.LogError($"Validation Error: NPC '{npc.gameObject.name}' is missing World Dialog Text!", npc.gameObject);
             return false;
         }
         // Add more checks if needed (e.g., for Animator if required by default)
         return true;
     }

     private bool ValidateDialogLine(DialogLine line) {
         if (line.speaker == null) {
             Debug.LogError("Validation Error: Dialog line is missing a Speaker reference!");
             return false;
         }
          if (!ValidateNpcReferences(line.speaker)) { // Reuse NPC validation
             return false;
         }
         if (string.IsNullOrEmpty(line.sentence)) {
              Debug.LogWarning($"Validation Warning: Dialog line for speaker '{line.speaker.gameObject.name}' has an empty sentence.", line.speaker.gameObject);
             // Allow empty sentences, maybe for pauses? Return true here.
         }
         if (!string.IsNullOrEmpty(line.animationTrigger) && line.speaker.GetSpeakerAnimator() == null) {
              Debug.LogWarning($"Validation Warning: Dialog line for '{line.speaker.gameObject.name}' has animation trigger '{line.animationTrigger}' but speaker is missing Animator component!", line.speaker.gameObject);
             // Allow continuing, but warn the user.
         }
         // Add check for the event itself (optional, as null is allowed)
        // if (line.onLineStartEvent == null) {
        //      Debug.LogWarning($"Validation Warning: Dialog line for '{line.speaker.gameObject.name}' has no Line Start Event assigned.", line.speaker.gameObject);
        // }
         return true;
     }
}