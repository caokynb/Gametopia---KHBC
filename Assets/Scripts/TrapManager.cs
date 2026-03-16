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

    [Header("Vật rơi sẵn trong scene")]
    public Rigidbody2D fallingObject;

    private bool trapTriggered = false;

    void Update()
    {
        if (trapTriggered) return;

        CheckResult();
    }

    void CheckResult()
    {
        // Sai số lượng -> reset
        if (bowl1.currentBamboo > targetBowl1 ||
            bowl2.currentBamboo > targetBowl2 ||
            bowl3.currentBamboo > targetBowl3)
        {
            Debug.Log("Sai số lượng! Reset.");

            bowl1.ResetBowl();
            bowl2.ResetBowl();
            bowl3.ResetBowl();
            return;
        }

        // Đúng hoàn toàn
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

        Debug.Log("Đúng! Vật rơi xuống.");

        if (fallingObject != null)
        {
            fallingObject.bodyType = RigidbodyType2D.Dynamic;
            fallingObject.gravityScale = 3f;
        }

        enabled = false;
    }
}