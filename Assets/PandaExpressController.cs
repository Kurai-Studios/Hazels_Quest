using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PandaExpressController : MonoBehaviour, IInteractable
{
    public NPCController dialogueData;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portaitImage;

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;
    void Awake()
    {
        Debug.Log($"{gameObject.name} Awake - dialogueData is: {dialogueData}");
    }

    void Start()
    {
        Debug.Log($"{gameObject.name} Start - dialogueData is: {dialogueData}");
    }
    public bool canInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        Debug.Log($"[{gameObject.name}] Interact called. DialogueData: {dialogueData}");

        if (dialogueData == null)
        {
            Debug.LogWarning($"[{gameObject.name}] No dialogue data assigned!");
            return;
        }

        if (isDialogueActive)
        {
            Debug.Log($"[{gameObject.name}] Continuing dialogue...");
            NextLine();
        }
        else
        {
            Debug.Log($"[{gameObject.name}] Starting dialogue...");
            StartDialogue();
        }
    } 

    void StartDialogue()
    {
        Debug.Log("Starting dialogue...");

        if (dialoguePanel == null || dialogueText == null || nameText == null || portaitImage == null)
        {
            Debug.LogError("UI references are not assigned!");
            return;
        }

        isDialogueActive = true;
        dialogueIndex = 0;

        nameText.SetText(dialogueData.npcName);
        portaitImage.sprite = dialogueData.npcPortrait;

        dialoguePanel.SetActive(true);

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");

        foreach(char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping = false;
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueText.SetText("");
        dialoguePanel.SetActive(false);
    }
}
