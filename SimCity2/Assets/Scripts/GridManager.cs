using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridManager : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float cellSize = 1.0f;

    // --- ส่วนที่แก้ไข/เพิ่มเข้ามาใหม่ ---
    [Header("Building Settings")]
    public BuildingData[] availableBuildings; // รายการข้อมูลตึกทั้งหมด (ลบ public GameObject buildingPrefab อันเก่าออกได้เลยค่ะ)
    private int selectedBuildingIndex = 0;   // เก็บสถิติว่าตอนนี้เลือกตึกตัวไหนอยู่
    // ----------------------------------

    public GameObject highlightPrefab;
    private GameObject highlightInstance;

    public static event Action<int> OnBuildingPlaced;

    private GameObject[,] gridArray;
    private Vector2 gridOffset;

    void Start()
    {
        gridArray = new GameObject[width, height];
        
        // คำนวณ Offset ให้ตารางอยู่กึ่งกลางโลกพอดี
        gridOffset = new Vector2(-(width / 2f) * cellSize, -(height / 2f) * cellSize);

        if (highlightPrefab != null)
        {
            highlightInstance = Instantiate(highlightPrefab);
            highlightInstance.SetActive(false);
        }
    }

    void Update()
    {
        UpdateHighlight();

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

            if (float.IsNaN(mouseScreenPos.x) || float.IsNaN(mouseScreenPos.y))
            {
                return;
            }

            if (Camera.main != null)
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Camera.main.nearClipPlane));
                
                // ชดเชยพิกัดด้วย Offset ก่อนคำนวณตำแหน่งตาราง
                Vector2Int gridPosition = GetGridPosition(new Vector3(mousePosition.x - gridOffset.x, mousePosition.y - gridOffset.y, 0));

                if (IsValidPosition(gridPosition))
                {
                    PlaceBuilding(gridPosition);
                }
            }
        }
    }

    void UpdateHighlight()
    {
        if (Camera.main == null || highlightInstance == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        
        // ตรวจสอบว่าเมาส์อยู่ในขอบเขตหน้าจอหรือไม่
        if (mouseScreenPos.x < 0 || mouseScreenPos.x > Screen.width || 
            mouseScreenPos.y < 0 || mouseScreenPos.y > Screen.height)
        {
            highlightInstance.SetActive(false);
            return;
        }

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Camera.main.nearClipPlane));
        
        // ชดเชยพิกัดด้วย Offset
        Vector2Int gridPosition = GetGridPosition(new Vector3(mousePosition.x - gridOffset.x, mousePosition.y - gridOffset.y, 0));

        if (gridPosition.x >= 0 && gridPosition.x < width && gridPosition.y >= 0 && gridPosition.y < height)
        {
            highlightInstance.SetActive(true);
            
            // นำ Offset กลับมาคำนวณตำแหน่งแสดงผลของ Highlight
            Vector3 cellCenter = new Vector3(
                gridPosition.x * cellSize + (cellSize / 2) + gridOffset.x, 
                gridPosition.y * cellSize + (cellSize / 2) + gridOffset.y, 
                0
            );
            highlightInstance.transform.position = cellCenter;
        }
        else
        {
            highlightInstance.SetActive(false);
        }
    }

    Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / cellSize);
        int y = Mathf.FloorToInt(worldPosition.y / cellSize);
        return new Vector2Int(x, y);
    }

    bool IsValidPosition(Vector2Int pos)
    {
        if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
        {
            return gridArray[pos.x, pos.y] == null;
        }
        return false;
    }

    // --- ฟังก์ชันนี้ถูกแก้ไขข้างในทั้งหมด ---
    void PlaceBuilding(Vector2Int pos)
    {
        BuildingData currentData = availableBuildings[selectedBuildingIndex];
        
        // ค้นหา ResourceManager เพื่อเช็คเงินก่อนสร้าง
        ResourceManager rm = FindFirstObjectByType<ResourceManager>();
        
        if (rm != null && rm.gold >= currentData.cost)
        {
            Vector3 worldPosition = new Vector3(
                pos.x * cellSize + (cellSize / 2) + gridOffset.x, 
                pos.y * cellSize + (cellSize / 2) + gridOffset.y, 
                0
            );

            // สร้างตึกจาก Prefab ที่ระบุไว้ในข้อมูลตึกชนิดนั้นๆ
            GameObject newBuilding = Instantiate(currentData.prefab, worldPosition, Quaternion.identity);
            
            // ส่งค่ารายได้ (Income) ไปให้กับสคริปต์ Building ที่อยู่ในตึกนั้น
            if (newBuilding.TryGetComponent(out Building b))
            {
                b.incomePerTick = currentData.incomePerTick;
            }

            // สั่งทำลายทอง/หักเงิน ตามราคากลางจริงของตึกนั้น
            OnBuildingPlaced?.Invoke(currentData.cost);
            gridArray[pos.x, pos.y] = newBuilding;
        }
        else
        {
            Debug.Log("เงินไม่พอสร้าง " + currentData.name);
        }
    }

    // --- ฟังก์ชันเพิ่มเข้ามาใหม่สำหรับให้ปุ่ม UI เรียกใช้ ---
    public void SelectBuilding(int index)
    {
        selectedBuildingIndex = index;
    }
}