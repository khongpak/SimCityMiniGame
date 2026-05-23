using UnityEngine;
// สำคัญมาก: ต้องเติมบรรทัดนี้เพื่อเรียกใช้งานระบบ Input ใหม่ค่ะ
using UnityEngine.InputSystem; 

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    
    [Header("Zoom Settings")]
    public float zoomSpeed = 2f; // ปรับลดลงเล็กน้อยเพื่อให้ซูมสมูทขึ้นในระบบใหม่
    public float minZoom = 2f;
    public float maxZoom = 15f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        HandleMovement();
        HandleZoom();
    }

    void HandleMovement()
    {
        float x = 0f;
        float y = 0f;

        // เช็คปุ่มระบบใหม่จาก Keyboard โดยตรง
        if (Keyboard.current != null)
        {
            // เช็คปุ่ม A, D หรือ ลูกศรซ้าย, ขวา
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x = 1f;

            // เช็คปุ่ม W, S หรือ ลูกศรขึ้น, ลง
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y = 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y = -1f;
        }

        Vector3 move = new Vector3(x, y, 0) * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);
    }

    void HandleZoom()
    {
        // เช็คการหมุนลูกกลิ้งเมาส์ระบบใหม่
        if (Mouse.current != null)
        {
            // ดึงค่าการ Scroll แกน Y (หมุนขึ้นจะเป็นค่าบวก หมุนลงเป็นค่าลบ)
            float scrollValue = Mouse.current.scroll.ReadValue().y;

            if (scrollValue != 0)
            {
                // ในระบบใหม่ค่าสกรอลจะค่อนข้างใหญ่ (เช่น 120 หรือ -120) เราจึงต้องหารหักลบลงมาเล็กน้อย
                cam.orthographicSize -= (scrollValue * 0.01f) * zoomSpeed;
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
            }
        }
    }
}