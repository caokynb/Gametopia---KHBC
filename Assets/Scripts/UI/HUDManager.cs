using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class HUDManager : MonoBehaviour
{
    [Header("Liên kết Nhân vật")]
    public PlayerMovement player;

    [Header("Giao diện Tre")]
    public TextMeshProUGUI bambooText;

    [Header("Giao diện Máu")]
    public Transform healthContainer;
    public GameObject heartPrefab;

    [Header("Giao diện Đổi Mode")]
    public Image hudBackground; // Kéo thả tấm nền HUD vào đây

    [Header("Chế độ Xây Dựng")]
    public Sprite constructionSprite;
    public Color constructionColor = Color.white; // Màu mặc định nếu không có ảnh

    [Header("Chế độ Tấn Công")]
    public Sprite attackSprite;
    public Color attackColor = Color.white;       // Màu mặc định nếu không có ảnh

    private List<GameObject> heartList = new List<GameObject>();

    // --- BIẾN DÙNG CHO HIỆU ỨNG RUNG ---
    private int lastHealth;
    private RectTransform containerRect;
    private Vector2 originalContainerPos;
    private Coroutine shakeCoroutine;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.GetComponent<PlayerMovement>();
        }

        // Lưu lại vị trí ban đầu của thanh máu để rung xong còn biết đường quay về
        containerRect = healthContainer.GetComponent<RectTransform>();
        if (containerRect != null)
        {
            originalContainerPos = containerRect.anchoredPosition;
        }

        if (player != null && player.stats != null)
        {
            lastHealth = player.stats.healthPoint;

            for (int i = 0; i < player.stats.maxHealth; i++)
            {
                GameObject newHeart = Instantiate(heartPrefab, healthContainer);
                newHeart.transform.localScale = Vector3.one;
                heartList.Add(newHeart);
            }
        }
    }

    void Update()
    {
        if (player == null || player.stats == null) return;

        // 1. Cập nhật số tre
        bambooText.text = player.stats.currentBambooCount.ToString();

        // 2. Kiểm tra lượng máu hiện tại
        int currentHp = player.stats.healthPoint;

        // NẾU MÁU HIỆN TẠI ÍT HƠN MÁU LƯU TRƯỚC ĐÓ -> BỊ ĐÁNH TRÚNG!
        if (currentHp < lastHealth)
        {
            if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
            shakeCoroutine = StartCoroutine(ShakeHeartsRoutine());
        }

        lastHealth = currentHp;

        // 3. Bật/Tắt các trái tim
        for (int i = 0; i < heartList.Count; i++)
        {
            if (i < currentHp) heartList[i].SetActive(true);
            else heartList[i].SetActive(false);
        }
    }

    // --- COROUTINE RUNG LẮC ---
    IEnumerator ShakeHeartsRoutine()
    {
        float elapsed = 0f;
        float duration = 0.3f;
        float magnitude = 15f;

        while (elapsed < duration)
        {
            float x = originalContainerPos.x + Random.Range(-magnitude, magnitude);
            float y = originalContainerPos.y + Random.Range(-magnitude, magnitude);

            containerRect.anchoredPosition = new Vector2(x, y);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        containerRect.anchoredPosition = originalContainerPos;
    }

    // --- HÀM THAY ĐỔI GIAO DIỆN (Gọi khi bấm phím chuyển mode) ---
    public void ChangeModeAppearance(bool isAttackMode)
    {
        if (hudBackground == null) return;

        if (isAttackMode)
        {
            if (attackSprite != null) hudBackground.sprite = attackSprite;
            hudBackground.color = attackColor;
        }
        else
        {
            if (constructionSprite != null) hudBackground.sprite = constructionSprite;
            hudBackground.color = constructionColor;
        }
    }
}