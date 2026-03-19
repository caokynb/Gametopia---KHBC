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
    // [MỚI 1] Biến toàn cục để các script khác dễ dàng gọi đến
    public static DialogueManager instance;

    // [MỚI 2] Biến đánh dấu xem hộp thoại có đang bật không
    public bool isDialogueActive = false;

    // [MỚI 3] Thêm hàm Awake này vào ngay dưới khai báo biến
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
    [Tooltip("Thời gian chờ trước khi tự động chuyển câu (Dành cho Auto Mode)")]
    public float autoDelay = 1.5f;

    private Queue<DialogueLine> lines;
    private bool isTyping = false;
    private string currentLine = "";

    // [MỚI] Cờ đánh dấu xem có đang ở chế độ Tự động không
    private bool isAutoMode = false;

    void Start()
    {
        lines = new Queue<DialogueLine>();
        if (dialogueBox != null) dialogueBox.SetActive(false);
    }

    void Update()
    {
        // [ĐÃ SỬA] Chỉ cho phép bấm F để tua nhanh/chuyển câu nếu KHÔNG phải Auto Mode
        if (!isAutoMode && dialogueBox != null && dialogueBox.activeInHierarchy && Input.GetKeyDown(KeyCode.F))
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
    }

    // [ĐÃ SỬA] Nhận thêm biến truyền vào để biết đây là Auto hay Manual
    public void StartDialogue(DialogueLine[] dialogueLines, bool autoMode = false)
    {
        isDialogueActive = true;
        isAutoMode = autoMode;
        dialogueBox.SetActive(true);
        lines.Clear();

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

        // [MỚI] Nếu là Auto Mode, đợi 1 khoảng thời gian rồi tự động chạy câu tiếp theo
        if (isAutoMode)
        {
            yield return new WaitForSeconds(autoDelay);
            DisplayNextLine();
        }
    }

    void EndDialogue()
    {
        // Gọi Coroutine để tạo độ trễ nhỏ trước khi tắt
        StartCoroutine(CloseDialogueRoutine());
    }

    IEnumerator CloseDialogueRoutine()
    {
        // Chờ 0.1 giây để phím F "nguội" đi, tránh bị nhận diện 2 lần
        yield return new WaitForSeconds(0.2f);

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }
        isDialogueActive = false;
    }
}