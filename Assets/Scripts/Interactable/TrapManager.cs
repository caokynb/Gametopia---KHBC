using UnityEngine;

public class TrapManager : MonoBehaviour
{
    [Header("Danh sách Lư Hương (Bát)")]
    public BambooBowl bowl1;
    public BambooBowl bowl2;
    public BambooBowl bowl3;

    [Header("Vật thể sẽ bị phá hủy khi giải xong")]
    public GameObject objectToDestroy;

    private bool trapTriggered = false;

    void Update()
    {
        if (trapTriggered) return;

        CheckResult();
    }

    void CheckResult()
    {
        // Chỉ cần 1 dòng kiểm tra cực gọn: Cả 3 Lư đều Xanh chưa?
        if (bowl1.isSolved && bowl2.isSolved && bowl3.isSolved)
        {
            TriggerTrap();
        }
    }

    void TriggerTrap()
    {
        trapTriggered = true;
        Debug.Log("Giải đố thành công cả 3 Lư Hương!");

        // Phá hủy cửa / chướng ngại vật
        if (objectToDestroy != null)
        {
            Destroy(objectToDestroy);
        }
    }
}