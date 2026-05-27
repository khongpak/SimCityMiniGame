using UnityEngine;

public class RoadController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Building buildingComponent;

    [Header("Road Sprites (ลากรูปภาพทั้ง 16 รูปมาใส่ตรงนี้ได้เลยค่ะ)")]
    public Sprite road_Isolated;   // 0: โดดเดี่ยว
    public Sprite road_N;          // 1: ทางตันหันขึ้น (เชื่อมบนอย่างเดียว)
    public Sprite road_S;          // 2: ทางตันหันลง (เชื่อมล่างอย่างเดียว)
    public Sprite road_E;          // 3: ทางตันหันขวา (เชื่อมขวาอย่างเดียว)
    public Sprite road_W;          // 4: ทางตันหันซ้าย (เชื่อมซ้ายอย่างเดียว)
    public Sprite road_NS;         // 5: ทางตรงแนวตั้ง (บน-ล่าง)
    public Sprite road_EW;         // 6: ทางตรงแนวนอน (ซ้าย-ขวา)
    public Sprite road_NE;         // 7: ทางโค้ง ขวา-บน
    public Sprite road_NW;         // 8: ทางโค้ง ซ้าย-บน
    public Sprite road_SE;         // 9: ทางโค้ง ขวา-ล่าง
    public Sprite road_SW;         // 10: ทางโค้ง ซ้าย-ล่าง
    public Sprite road_NSE;        // 11: สามแยก หันขวา (บน-ล่าง-ขวา)
    public Sprite road_NSW;        // 12: สามแยก หันซ้าย (บน-ล่าง-ซ้าย)
    public Sprite road_NEW;        // 13: สามแยก หันขึ้น (บน-ซ้าย-ขวา)
    public Sprite road_SEW;        // 14: สามแยก หันลง (ล่าง-ซ้าย-ขวา)
    public Sprite road_NSEW;       // 15: สี่แยก (เชื่อมครบทุกทิศ)

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        buildingComponent = GetComponent<Building>();
    }

    // ฟังก์ชันหลักที่จะถูกระบบสั่งให้รันอัปเดตรูปร่างตัวเองอัตโนมัติ
    public void UpdateRoadSprite()
    {
        if (buildingComponent == null || buildingComponent.gridManager == null) return;

        Vector2Int myPos = buildingComponent.myGridPos;
        GridManager gm = buildingComponent.gridManager;

        // เช็คเพื่อนบ้าน 4 ทิศรอบตัว ว่าเป็นถนนเหมือนกันไหม
        bool north = IsRoadAt(gm, myPos + Vector2Int.up);
        bool south = IsRoadAt(gm, myPos + Vector2Int.down);
        bool east  = IsRoadAt(gm, myPos + Vector2Int.right);
        bool west  = IsRoadAt(gm, myPos + Vector2Int.left);

        // เลือกสไปรต์ตามเงื่อนไขทิศทางแบบ Bitmasking
        Sprite selectedSprite = road_Isolated;

        // 1. สี่แยก
        if (north && south && east && west) selectedSprite = road_NSEW;
        
        // 2. สามแยก
        else if (north && south && east) selectedSprite = road_NSE;
        else if (north && south && west) selectedSprite = road_NSW;
        else if (north && east && west)  selectedSprite = road_NEW;
        else if (south && east && west)  selectedSprite = road_SEW;
        
        // 3. ทางโค้ง
        else if (north && east) selectedSprite = road_NE;
        else if (north && west) selectedSprite = road_NW;
        else if (south && east) selectedSprite = road_SE;
        else if (south && west) selectedSprite = road_SW;
        
        // 4. ทางตรง
        else if (north && south) selectedSprite = road_NS;
        else if (east && west)   selectedSprite = road_EW;
        
        // 5. ทางตัน
        else if (north) selectedSprite = road_N;
        else if (south) selectedSprite = road_S;
        else if (east)  selectedSprite = road_E;
        else if (west)  selectedSprite = road_W;
        
        // 6. ถนนโดดเดี่ยว
        else selectedSprite = road_Isolated;

        // สั่งเปลี่ยนรูปภาพบนหน้าจอจริง
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = selectedSprite;
        }
    }

    // ฟังก์ชันเสริมสำหรับวิ่งไปเช็คว่าช่องที่ระบุเป็นถนนไหม
    private bool IsRoadAt(GridManager gm, Vector2Int targetPos)
    {
        GameObject obj = gm.GetBuildingAt(targetPos);
        if (obj != null)
        {
            // ตรวจสอบว่าวัตถุในช่องนั้นมีชื่อที่มีคำว่า "Road" อยู่ในตัวแปรข้อมูลตึกหรือไม่
            if (obj.name.Contains("Road"))
            {
                return true;
            }
        }
        return false;
    }
}