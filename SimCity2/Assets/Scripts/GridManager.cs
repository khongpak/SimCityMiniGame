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

        // --- เพิ่มเข้ามาใหม่: คลิกขวาเพื่อทุบตึก ---
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

            if (float.IsNaN(mouseScreenPos.x) || float.IsNaN(mouseScreenPos.y)) return;

            if (Camera.main != null)
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Camera.main.nearClipPlane));
                Vector2Int gridPosition = GetGridPosition(new Vector3(mousePosition.x - gridOffset.x, mousePosition.y - gridOffset.y, 0));

                // ตรวจสอบว่าพิกัดอยู่ในตารางหรือไม่
                if (gridPosition.x >= 0 && gridPosition.x < width && gridPosition.y >= 0 && gridPosition.y < height)
                {
                    // ตรวจสอบว่ามีตึกให้ทุบไหม (ในอาเรย์ต้องไม่ว่าง)
                    if (gridArray[gridPosition.x, gridPosition.y] != null)
                    {
                        DemolishBuilding(gridPosition);
                    }
                }
            }
        }
    }

    void UpdateHighlight()
{
    if (Camera.main == null || highlightInstance == null) return;

    Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
    
    // --- เพิ่มตรงนี้เพื่อดักจับค่า NaN ป้องกันกล้อง Error ค่ะ ---
    if (float.IsNaN(mouseScreenPos.x) || float.IsNaN(mouseScreenPos.y))
    {
        highlightInstance.SetActive(false);
        return;
    }
    // ----------------------------------------------------

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
        
        // คำนวณตำแหน่งแสดงผลของ Highlight ให้ตรงล็อกตาราง
        Vector3 cellCenter = new Vector3(
            gridPosition.x * cellSize + (cellSize / 2) + gridOffset.x, 
            gridPosition.y * cellSize + (cellSize / 2) + gridOffset.y, 
            0
        );
        highlightInstance.transform.position = cellCenter;

        // --- เพิ่มระบบเปลี่ยนรูปร่างหน้าตาของเงาตึกตามตึกที่เลือกอยู่ ---
        // --- ระบบเงาตึกอัจฉริยะ (Smart Ghost Preview) ---
        if (availableBuildings != null && availableBuildings.Length > 0)
        {
            BuildingData currentData = availableBuildings[selectedBuildingIndex];
            GameObject currentPrefab = currentData.prefab;
            
            SpriteRenderer highlightSprite = highlightInstance.GetComponent<SpriteRenderer>();
            SpriteRenderer prefabSprite = currentPrefab.GetComponent<SpriteRenderer>();

            if (highlightSprite != null && prefabSprite != null)
            {
                highlightSprite.sprite = prefabSprite.sprite; // เปลี่ยนรูปตามตึกที่เลือก
                
                // ดึง ResourceManager มาเช็คเงิน ณ เฟรมนั้นๆ
                ResourceManager rm = FindFirstObjectByType<ResourceManager>();
                
                // ตรวจสอบเงื่อนไข: ช่องต้องว่าง และ เงินต้องพอ
                bool canPlace = IsValidPosition(gridPosition) && (rm != null && rm.gold >= currentData.cost);
                
                if (canPlace)
                {
                    // สร้างได้ -> ให้เงาเป็นสีเขียวตองอ่อนโปร่งแสง
                    highlightSprite.color = new Color(0.5f, 1f, 0.5f, 0.5f); 
                }
                else
                {
                    // สร้างไม่ได้ (เงินไม่พอ หรือช่องเต็ม) -> ให้เงาเป็นสีแดงโปร่งแสง
                    highlightSprite.color = new Color(1f, 0.5f, 0.5f, 0.5f); 
                }
            }
        }
        // --------------------------------------------------------
        // --------------------------------------------------------
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
                b.constructionCost = currentData.cost;
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

    void DemolishBuilding(Vector2Int pos)
    {
        GameObject buildingToDestroy = gridArray[pos.x, pos.y];

        // ในเกมแนวสร้างเมือง ตึกแต่ละแบบราคาไม่เท่ากัน เราต้องเช็คว่าตึกที่จะทุบราคาเท่าไหร่
        // แต่ตอนนี้เราคืนเงินแบบเหมาจ่ายครอย่างง่าย หรือถ้าจะให้ดี เราคำนวณคืนเงิน 50% ได้ค่ะ
        // เพื่อความง่ายในขั้นนี้ เราจะดึงข้อมูลราคา หรือคืนเงินให้ผู้เล่นเป็นค่าคงที่ไปก่อน เช่น คืนให้ 10 ทอง
        int refundAmount = 0; 

        if (buildingToDestroy.TryGetComponent(out Building b))
        {
            // คำนวณคืนเงิน 50% (ใช้การหาร 2)
            refundAmount = b.constructionCost / 2; 
        }

        ResourceManager rm = FindFirstObjectByType<ResourceManager>();
        if (rm != null)
        {
            rm.RefundGold(refundAmount);
        }

        // ลบวัตถุออกจากฉากเกม
        Destroy(buildingToDestroy);
        
        // เคลียร์ค่าใน Array ให้กลับมาว่าง (null) เพื่อให้สร้างตึกใหม่ทับได้
        gridArray[pos.x, pos.y] = null;
        
        Debug.Log($"ทุบตึกที่พิกัด {pos} เรียบร้อย ได้คืน {refundAmount} Gold");
    }
}