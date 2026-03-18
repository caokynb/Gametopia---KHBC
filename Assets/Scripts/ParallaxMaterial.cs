using UnityEngine;

public class ParallaxMaterial : MonoBehaviour
{
    [Tooltip("Hệ số trôi. VD: Rừng xa = 0.01, Rừng gần = 0.05")]
    public float parallaxEffect = 0.05f;

    private Material mat;
    private Transform cam;
    private Vector3 lastCamPos;

    void Start()
    {
        // Lấy chất liệu từ "Màn chiếu" Quad
        mat = GetComponent<MeshRenderer>().material;
        cam = Camera.main.transform;
        lastCamPos = cam.position;
    }

    void LateUpdate()
    {
        // Tính xem Camera vừa nhích đi bao nhiêu mét
        float deltaX = cam.position.x - lastCamPos.x;

        // Trượt bề mặt ảnh đi một khoảng tương ứng (Thay đổi toạ độ UV)
        mat.mainTextureOffset += new Vector2(deltaX * parallaxEffect, 0);

        // Lưu lại vị trí Camera cho khung hình tiếp theo
        lastCamPos = cam.position;
    }
}