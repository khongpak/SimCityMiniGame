using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps; // 🔥 [เพิ่มเข้ามาใหม่: จำเป็นต้องใช้เพื่อดักจับข้อมูล Tilemap ค่ะ]

public class GridManager : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float cellSize = 1.0f;

    [Header("Building Settings")]
    public BuildingData[] availableBuildings; 
    private int selectedBuildingIndex = 0;   

    [Header("References")]
    public GameObject highlightPrefab;
    private GameObject highlightInstance;
    // 🔥 [เพิ่มเข้ามาใหม่: กล่องสำหรับลากวัตถุ Tilemap ในฉากมาใส่เพื่อใช้เช็คพื้นผิว]
    public Tilemap targetTilemap; 

    public static event Action<int> OnBuildingPlaced;

    private GameObject[,] gridArray;
    private Vector2 gridOffset;

    void Start()
    {
        gridArray = new GameObject[width, height];
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

        // กดปุ่ม Esc เพื่อยกเลิกการวางตึก
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            selectedBuildingIndex = -1; 
            Debug.Log("ยกเลิกโหมดสร้างตึก เรียบร้อยค่ะ");
        }

        // คลิกซ้ายเพื่อวางตึก (เพิ่มเงื่อนไขตรวจสอบพื้นที่)
        if (selectedBuildingIndex != -1 && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

            if (float.IsNaN(mouseScreenPos.x) || float.IsNaN(mouseScreenPos.y)) return;

            if (Camera.main != null)
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Camera.main.nearClipPlane));
                Vector2Int gridPosition = GetGridPosition(new Vector3(mousePosition.x - gridOffset.x, mousePosition.y - gridOffset.y, 0));

                // 🔥 [แก้ไขจุดนี้]: ต้องผ่านทั้งเงื่อนไข IsValidPosition (ช่องว่าง) และเป็นพื้นดิน (IsWalkableTerrain) ถึงจะยอมให้สร้าง
                if (IsValidPosition(gridPosition) && IsWalkableTerrain(gridPosition))
                {
                    PlaceBuilding(gridPosition);
                }
            }
        }

        // คลิกขวาเพื่อทุบตึก
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

            if (float.IsNaN(mouseScreenPos.x) || float.IsNaN(mouseScreenPos.y)) return;

            if (Camera.main != null)
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Camera.main.nearClipPlane));
                Vector2Int gridPosition = GetGridPosition(new Vector3(mousePosition.x - gridOffset.x, mousePosition.y - gridOffset.y, 0));

                if (gridPosition.x >= 0 && gridPosition.x < width && gridPosition.y >= 0 && gridPosition.y < height)
                {
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
        if (selectedBuildingIndex == -1)
        {
            highlightInstance.SetActive(false);
            return;
        }

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        
        if (float.IsNaN(mouseScreenPos.x) || float.IsNaN(mouseScreenPos.y))
        {
            highlightInstance.SetActive(false);
            return;
        }

        if (mouseScreenPos.x < 0 || mouseScreenPos.x > Screen.width || 
            mouseScreenPos.y < 0 || mouseScreenPos.y > Screen.height)
        {
            highlightInstance.SetActive(false);
            return;
        }

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Camera.main.nearClipPlane));
        Vector2Int gridPosition = GetGridPosition(new Vector3(mousePosition.x - gridOffset.x, mousePosition.y - gridOffset.y, 0));

        if (gridPosition.x >= 0 && gridPosition.x < width && gridPosition.y >= 0 && gridPosition.y < height)
        {
            highlightInstance.SetActive(true);
            
            Vector3 cellCenter = new Vector3(
                gridPosition.x * cellSize + (cellSize / 2) + gridOffset.x, 
                gridPosition.y * cellSize + (cellSize / 2) + gridOffset.y, 
                0
            );
            highlightInstance.transform.position = cellCenter;

            if (availableBuildings != null && availableBuildings.Length > 0)
            {
                BuildingData currentData = availableBuildings[selectedBuildingIndex];
                GameObject currentPrefab = currentData.prefab;
                
                SpriteRenderer highlightSprite = highlightInstance.GetComponent<SpriteRenderer>();
                SpriteRenderer prefabSprite = currentPrefab.GetComponent<SpriteRenderer>();

                if (highlightSprite != null && prefabSprite != null)
                {
                    highlightSprite.sprite = prefabSprite.sprite; 
                    
                    ResourceManager rm = FindFirstObjectByType<ResourceManager>();

                    bool hasPower = true;
                    if (currentData.powerGeneratedOrConsumed < 0 && rm != null)
                    {
                        // ถ้าเป็นตึกที่กินไฟ ให้เช็คว่าไฟส่วนกลางเหลือพอไหม
                        hasPower = rm.HasEnoughPower(currentData.powerGeneratedOrConsumed);
                    }
                    
                    // 🔥 [แก้ไขจุดนี้]: เปลี่ยนสีพรีวิวเป็นเขียวเมื่อช่องว่าง เงินพอ และ "ไม่ใช่พื้นน้ำ" เท่านั้น
                    bool canPlace = IsValidPosition(gridPosition) && 
                                    IsWalkableTerrain(gridPosition) && 
                                    (rm != null && rm.gold >= currentData.cost)&&
                                    hasPower;
                    
                    if (canPlace)
                    {
                        highlightSprite.color = new Color(0.5f, 1f, 0.5f, 0.5f); // สีเขียวโปร่งแสงผ่านฉลุย
                    }
                    else
                    {
                        highlightSprite.color = new Color(1f, 0.5f, 0.5f, 0.5f); // สีแดงเตือนภัย (ห้ามวาง)
                    }
                }
            }
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

    // 🔥 [ฟังก์ชันเพิ่มเข้ามาใหม่สำหรับระบบสแกนตรวจสอบประเภทพื้นผิวบน Tilemap]
    bool IsWalkableTerrain(Vector2Int gridPos)
    {
        // ถ้าคุณเพชรยังไม่ได้ลากวัตถุ Tilemap มาใส่ในช่องดักจับ ให้ระบบยอมให้วางของไปก่อนเพื่อป้องกันค้าง
        if (targetTilemap == null) return true;

        // แปลงพิกัด Grid สู่ระบบพิกัดตําแหน่ง Cell ของตัว Tilemap ตรง ๆ
        Vector3Int tilemapCellPos = new Vector3Int(gridPos.x - (width / 2), gridPos.y - (height / 2), 0);

        // วิ่งไปเช็คแผ่นกระเบื้อง ณ พิกัดนั้น ๆ
        TileBase currentTile = targetTilemap.GetTile(tilemapCellPos);

        // ถ้าไม่มีแผ่นกระเบื้องวางอยู่เลย (เช่น นอกขอบแมพ) ห้ามวางตึกเด็ดขาดค่ะ
        if (currentTile == null) return false;

        // สแกนตรวจสอบชื่อของสไปรต์กระเบื้องชิ้นนั้น
        string tileName = currentTile.name.ToLower();

        // 🌊 [กฎเหล็ก]: ถ้าชื่อไฟล์มีคำว่าน้ำ (water) หรือ ทะเล (sea) ให้ส่งค่ากลับเป็น False (สร้างตึกไม่ได้)
        if (tileName.Contains("water") || tileName.Contains("sea") || tileName.Contains("river"))
        {
            return false; 
        }

        return true; // ถ้าไม่ใช่พื้นน้ำ ยอมให้ผ่านค่ะ
    }

    
    void PlaceBuilding(Vector2Int pos)
    {
        BuildingData currentData = availableBuildings[selectedBuildingIndex];
        ResourceManager rm = FindFirstObjectByType<ResourceManager>();
        
        // ตรวจสอบทั้งเงิน และตรวจสอบไฟฟ้า (ถ้าเป็นตึกกินไฟ)
        bool canPlace = rm != null && rm.gold >= currentData.cost;
        if (currentData.powerGeneratedOrConsumed < 0 && rm != null)
        {
            canPlace = canPlace && rm.HasEnoughPower(currentData.powerGeneratedOrConsumed);
        }

        if (canPlace)
        {
            Vector3 worldPosition = new Vector3(
                pos.x * cellSize + (cellSize / 2) + gridOffset.x, 
                pos.y * cellSize + (cellSize / 2) + gridOffset.y, 
                0
            );

            GameObject newBuilding = Instantiate(currentData.prefab, worldPosition, Quaternion.identity);
            
            // 🔥 [ส่งค่า powerGeneratedOrConsumed เข้าไปในตัวตึกด้วยค่ะ]
            if (newBuilding.TryGetComponent(out Building b))
            {
                b.Setup(currentData.incomePerTick, currentData.cost, currentData.powerGeneratedOrConsumed, pos, this);
            }

            // 🔥 [สั่งอัปเดตระบบไฟฟ้าเข้าสู่ ResourceManager ส่วนกลาง]
            if (rm != null)
            {
                rm.UpdatePowerGrid(currentData.powerGeneratedOrConsumed);
            }

            OnBuildingPlaced?.Invoke(currentData.cost);
            gridArray[pos.x, pos.y] = newBuilding;

            NotifyNeighbors(pos);
        }
        else
        {
            Debug.Log("เงิน หรือ ระบบไฟฟ้าไม่เพียงพอสำหรับสร้าง " + currentData.name);
        }
    }

    public void SelectBuilding(int index)
    {
        selectedBuildingIndex = index;
    }

    // --- เพิ่มเติมในฟังก์ชัน DemolishBuilding() ---
    void DemolishBuilding(Vector2Int pos)
    {
        GameObject buildingToDestroy = gridArray[pos.x, pos.y];
        int refundAmount = 0; 
        int powerValue = 0; // 🔥 เพิ่มตัวแปรจำค่าไฟของตึกที่จะทุบ

        if (buildingToDestroy.TryGetComponent(out Building b))
        {
            refundAmount = b.constructionCost / 2; 
            powerValue = b.powerGeneratedOrConsumed; // 🔥 ดึงค่าไฟออกมา
        }

        ResourceManager rm = FindFirstObjectByType<ResourceManager>();
        if (rm != null)
        {
            rm.RefundGold(refundAmount);
            rm.ReleasePower(powerValue); // 🔥 [คืนค่าไฟฟ้าเข้าสู่ระบบส่วนกลาง]
        }

        Destroy(buildingToDestroy);
        gridArray[pos.x, pos.y] = null;
        
        Debug.Log($"ทุบตึกที่พิกัด {pos} เรียบร้อย ได้คืน {refundAmount} Gold และอัปเดตระบบไฟ");

        NotifyNeighbors(pos);
    }

    public GameObject GetBuildingAt(Vector2Int pos)
    {
        if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
        {
            return gridArray[pos.x, pos.y];
        }
        return null;
    }

    private void NotifyNeighbors(Vector2Int centerPos)
    {
        Vector2Int[] neighbors = new Vector2Int[]
        {
            new Vector2Int(centerPos.x + 1, centerPos.y), 
            new Vector2Int(centerPos.x - 1, centerPos.y), 
            new Vector2Int(centerPos.x, centerPos.y + 1), 
            new Vector2Int(centerPos.x, centerPos.y - 1)  
        };

        foreach (var p in neighbors)
        {
            GameObject obj = GetBuildingAt(p);
            if (obj != null && obj.TryGetComponent(out Building b))
            {
                b.CheckRoadConnection(); 
            }
        }
    }
}