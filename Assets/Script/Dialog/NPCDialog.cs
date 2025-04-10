// NPCDialog.cs
using UnityEngine;
using UnityEngine.Events;
using TMPro; // ยังคงจำเป็นสำหรับ worldDialogText

public class NPCDialog : MonoBehaviour
{
    [Header("Speaker Visuals")]
    [Tooltip("Animator ของตัวละคร NPC นี้ (ถ้าต้องการให้มี Animation ตอนพูด)")]
    public Animator speakerAnimator; // << NEW: เพิ่ม Animator
    [Tooltip("GameObject ที่เป็นกรอบคำพูด (จะถูก เปิด/ปิด)")]
    public GameObject dialogBubbleObject;
    [Tooltip("TextMeshProUGUI ที่ใช้แสดงข้อความในกรอบคำพูด")]
    public TextMeshProUGUI worldDialogText;

    [Header("First Interaction Dialog")]
    [Tooltip("ติ๊กถ้าต้องการให้เริ่มบทสนทนาแบบหลายคนเมื่อคุยครั้งแรก")]
    public bool isFirstInteractionConversation = false;
    [Tooltip("บทสนทนาแบบหลายคน (ถ้า isFirstInteractionConversation เป็น true)")]
    public DialogLine[] firstInteractionConversation;
    [Tooltip("ประโยคที่ NPC พูดคนเดียว (ถ้า isFirstInteractionConversation เป็น false)")]
    [TextArea(3, 10)]
    public string[] firstInteractionSentences;

    [Header("Subsequent Interaction Dialog (Optional)")]
    [Tooltip("บทสนทนา/ประโยคที่จะใช้หลังจากคุยครั้งแรกไปแล้ว (ถ้าเว้นว่าง จะใช้ First Interaction ซ้ำ)")]
    public bool hasSubsequentDialog = false; // << NEW: ติ๊กเพื่อเปิดใช้งาน Subsequent Dialog
    [Tooltip("ติ๊กถ้าต้องการให้เริ่มบทสนทนาแบบหลายคนเมื่อคุยครั้งถัดไป")]
    public bool isSubsequentInteractionConversation = false; // << NEW
    [Tooltip("บทสนทนาแบบหลายคนสำหรับครั้งถัดไป (ถ้า hasSubsequentDialog และ isSubsequentInteractionConversation เป็น true)")]
    public DialogLine[] subsequentInteractionConversation; // << NEW
    [Tooltip("ประโยคที่ NPC พูดคนเดียวสำหรับครั้งถัดไป (ถ้า hasSubsequentDialog และ isSubsequentInteractionConversation เป็น false)")]
    [TextArea(3, 10)]
    public string[] subsequentInteractionSentences; // << NEW


    [Header("Events")]
    [Tooltip("Event ที่จะถูกเรียก *หลังจาก* บทสนทนา (ทั้งครั้งแรกและครั้งถัดไป) จบลง")]
    public UnityEvent onDialogComplete; // Event เดิม ใช้สำหรับทั้งสองแบบ

    // --- Internal State ---
    private bool playerInRange = false;
    private bool hasInteractedBefore = false; // << NEW: สถานะว่าเคยคุยจบครั้งแรกไปหรือยัง

    void Start()
    {
        // Validate required components
        if (dialogBubbleObject == null) Debug.LogError($"NPC '{gameObject.name}': Missing Dialog Bubble Object reference!", gameObject);
        if (worldDialogText == null) Debug.LogError($"NPC '{gameObject.name}': Missing World Dialog Text reference!", gameObject);

        // Validate optional components if used
        if (!string.IsNullOrEmpty(GetFirstInteractionAnimationTrigger()) && speakerAnimator == null)
            Debug.LogWarning($"NPC '{gameObject.name}': Has animation trigger but missing Speaker Animator reference!", gameObject);

        // Validate dialog content (basic checks)
        if (!isFirstInteractionConversation && (firstInteractionSentences == null || firstInteractionSentences.Length == 0))
            Debug.LogWarning($"NPC '{gameObject.name}': Configured for single dialog (first) but has no sentences.", gameObject);
        if (isFirstInteractionConversation && (firstInteractionConversation == null || firstInteractionConversation.Length == 0))
            Debug.LogWarning($"NPC '{gameObject.name}': Configured for conversation (first) but sequence is empty.", gameObject);

        // Validate subsequent dialog if enabled
        if(hasSubsequentDialog)
        {
            if (!isSubsequentInteractionConversation && (subsequentInteractionSentences == null || subsequentInteractionSentences.Length == 0))
                Debug.LogWarning($"NPC '{gameObject.name}': Configured for single subsequent dialog but has no sentences.", gameObject);
             if (isSubsequentInteractionConversation && (subsequentInteractionConversation == null || subsequentInteractionConversation.Length == 0))
                 Debug.LogWarning($"NPC '{gameObject.name}': Configured for subsequent conversation but sequence is empty.", gameObject);
        }


        SetBubbleActive(false); // ซ่อน Bubble ตอนเริ่ม
    }

    void Update()
    {
        // Check for interaction input only if player is in range and no dialog is currently active globally
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !DialogManager.instance.IsDialogActive())
        {
            TriggerInteraction();
        }
    }

    private void TriggerInteraction()
    {
        bool useSubsequent = hasSubsequentDialog && hasInteractedBefore;

        // Determine which dialog sequence to use
        bool useConversation = useSubsequent ? isSubsequentInteractionConversation : isFirstInteractionConversation;
        DialogLine[] conversation = useSubsequent ? subsequentInteractionConversation : firstInteractionConversation;
        string[] sentences = useSubsequent ? subsequentInteractionSentences : firstInteractionSentences;

        if (useConversation)
        {
            if (conversation != null && conversation.Length > 0)
            {
                // Pass 'this' as the starter NPC, and the correct onComplete event
                DialogManager.instance.StartConversation(conversation, this, onDialogComplete);
            }
            else
            {
                Debug.LogWarning($"NPC '{gameObject.name}': Tried to start {(useSubsequent ? "subsequent" : "first")} conversation, but sequence is empty/null.", gameObject);
            }
        }
        else // Use single NPC sentences
        {
            if (sentences != null && sentences.Length > 0)
            {
                // Pass 'this' as the speaker NPC, and the correct onComplete event
                DialogManager.instance.StartDialog(sentences, this, onDialogComplete);
            }
            else
            {
                Debug.LogWarning($"NPC '{gameObject.name}': Tried to start {(useSubsequent ? "subsequent" : "first")} single dialog, but sentences are empty/null.", gameObject);
            }
        }
    }


    // --- Public Methods Called by DialogManager or Triggers ---

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

    public Animator GetSpeakerAnimator() // << NEW: Getter for Animator
    {
        return speakerAnimator;
    }

    // Called by DialogManager after the FIRST interaction finishes
    public void MarkFirstInteractionComplete() // << NEW
    {
        if (!hasInteractedBefore)
        {
            hasInteractedBefore = true;
            Debug.Log($"NPC '{gameObject.name}' marked as interacted.");
            // Add any logic here if needed when the first interaction is marked (e.g., save state)
        }
    }


    // --- Collision Detection ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    // --- Helper for Start() validation ---
     private string GetFirstInteractionAnimationTrigger()
     {
         if (isFirstInteractionConversation && firstInteractionConversation != null && firstInteractionConversation.Length > 0)
             return firstInteractionConversation[0].animationTrigger; // Check first line of conversation
         // For single sentences, we can't easily check here without more complex logic
         return null;
     }
}