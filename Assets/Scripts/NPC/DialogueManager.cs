using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class DialogueLine
{
    public string characterName;
    [TextArea(3, 10)]
    public string line;
    public Sprite portrait;
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    public bool isDialogueActive = false;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    [Header("UI Elements")]
    public GameObject dialogueBox;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;

    [Header("Cài đặt")]
    public float typingSpeed = 0.04f;

    private Queue<DialogueLine> lines;
    private bool isTyping = false;
    private string currentLine = "";

    // --- BÍ KÍP TA: Bộ đếm chống Input Bleed ---
    private float nextInputTime = 0f;

    void Start()
    {
        lines = new Queue<DialogueLine>();
        if (dialogueBox != null) dialogueBox.SetActive(false);
    }

    void Update()
    {
        if (dialogueBox != null && dialogueBox.activeInHierarchy)
        {
            // Chặn mọi nút bấm nếu chưa qua thời gian "nguội" (Cooldown)
            if (Time.time < nextInputTime) return;

            if (Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0))
            {
                if (isTyping)
                {
                    StopAllCoroutines();
                    dialogueText.text = currentLine;
                    isTyping = false;
                }
                else
                {
                    DisplayNextLine();
                }
            }

            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetMouseButtonDown(1))
            {
                StopAllCoroutines();
                lines.Clear();
                EndDialogue();
            }
        }
    }

    public void StartDialogue(DialogueLine[] dialogueLines, bool autoMode = false)
    {
        isDialogueActive = true;
        dialogueBox.SetActive(true);
        lines.Clear();

        // --- BÍ KÍP TA NẰM Ở ĐÂY ---
        // Ép hệ thống điếc trong 0.2 giây đầu tiên để không nhận nhầm nút F lúc kích hoạt
        nextInputTime = Time.time + 0.2f;

        foreach (DialogueLine line in dialogueLines)
        {
            lines.Enqueue(line);
        }

        DisplayNextLine();
    }

    public void DisplayNextLine()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine nextLine = lines.Dequeue();

        if (nameText != null) nameText.text = nextLine.characterName;

        if (portraitImage != null)
        {
            if (nextLine.portrait != null)
            {
                portraitImage.sprite = nextLine.portrait;
                portraitImage.color = Color.white;
            }
            else
            {
                portraitImage.color = Color.clear;
            }
        }

        StopAllCoroutines();
        StartCoroutine(TypeSentence(nextLine.line));
    }

    IEnumerator TypeSentence(string sentence)
    {
        currentLine = sentence;
        dialogueText.text = "";
        isTyping = true;

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void EndDialogue()
    {
        StartCoroutine(CloseDialogueRoutine());
    }

    IEnumerator CloseDialogueRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }
        isDialogueActive = false;
    }
}