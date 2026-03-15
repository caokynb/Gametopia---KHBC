using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class HUDManager : MonoBehaviour
{
    public PlayerMovement player;
    public TextMeshProUGUI bambooText;
    public Transform healthContainer;
    public GameObject heartPrefab;

    private List<GameObject> heartList = new List<GameObject>();

    void Start()
    {
        Debug.Log("--- BẮT ĐẦU KIỂM TRA HUD ---");

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.GetComponent<PlayerMovement>();
        }

        if (player != null)
        {
            if (player.stats != null)
            {
                // In ra chính xác số máu mà nó đọc được!
                Debug.Log("Đã tìm thấy Stats! Máu tối đa đang là: " + player.stats.maxHealth + ", Máu hiện tại là: " + player.stats.healthPoint);

                // Dùng maxHealth (hoặc healthPoint tùy bạn đang setup) để sinh tim
                int heartsToSpawn = player.stats.maxHealth;

                if (heartsToSpawn == 0)
                {
                    Debug.LogWarning("⚠️ CẢNH BÁO: Máu bằng 0 nên vòng lặp đẻ tim bị hủy!");
                }

                for (int i = 0; i < heartsToSpawn; i++)
                {
                    GameObject newHeart = Instantiate(heartPrefab, healthContainer);
                    newHeart.transform.localScale = Vector3.one;
                    heartList.Add(newHeart);
                }

                Debug.Log("Đã đẻ xong " + heartList.Count + " trái tim!");
            }
            else
            {
                Debug.LogError("❌ LỖI: Tìm thấy Player nhưng dữ liệu Stats bị NULL!");
            }
        }
        else
        {
            Debug.LogError("❌ LỖI: Không tìm thấy Player!");
        }
    }

    void Update()
    {
        if (player == null || player.stats == null) return;

        bambooText.text = player.stats.currentBambooCount.ToString();

        int currentHp = player.stats.healthPoint;
        for (int i = 0; i < heartList.Count; i++)
        {
            if (i < currentHp) heartList[i].SetActive(true);
            else heartList[i].SetActive(false);
        }
    }
}