using UnityEngine;

public class TrapManager : MonoBehaviour
{
    [Header("Danh sách bát")]
    public BambooBowl bowl1;
    public BambooBowl bowl2;
    public BambooBowl bowl3;

    [Header("Số lượng tre đúng yêu cầu")]
    public int targetBowl1 = 8;
    public int targetBowl2 = 12;
    public int targetBowl3 = 6;

    [Header("Vật thể sẽ bị phá hủy")]
    public GameObject objectToDestroy; // Đổi từ Rigidbody2D sang GameObject

    private bool trapTriggered = false;

    void Update()
    {
        if (trapTriggered) return;

        CheckResult();
    }

    void CheckResult()
    {
        // Sai số lượng (vượt quá) -> reset
        if (bowl1.currentBamboo > targetBowl1 ||
            bowl2.currentBamboo > targetBowl2 ||
            bowl3.currentBamboo > targetBowl3)
        {
            Debug.Log("Sai số lượng! Các bát đã bị reset.");
            bowl1.ResetBowl();
            bowl2.ResetBowl();
            bowl3.ResetBowl();
            return;
        }

        // Đúng hoàn toàn số lượng ở cả 3 bát
        if (bowl1.currentBamboo == targetBowl1 &&
            bowl2.currentBamboo == targetBowl2 &&
            bowl3.currentBamboo == targetBowl3)
        {
            TriggerTrap();
        }
    }

    void TriggerTrap()
    {
        trapTriggered = true;

        Debug.Log("Đúng số lượng! Vật thể đã bị phá hủy.");

        if (objectToDestroy != null)
        {
            // Phá hủy vật thể ngay lập tức
            Destroy(objectToDestroy);
        }

        // Tắt script này để tránh chạy Update thừa
        enabled = false;
    }
}