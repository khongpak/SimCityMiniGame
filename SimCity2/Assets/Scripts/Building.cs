using UnityEngine;

public class Building : MonoBehaviour
{
    [Header("Building Income Settings")]
    public int incomePerTick = 10;
    public int constructionCost = 50;

    [Header("Status")]
    public bool isConnectedToRoad = false;

    // 🔥 [เพิ่มเข้ามาใหม่]: กล่องสำหรับจำพิกัดของตัวเอง และจำว่าใครเป็นคนสร้าง (GridManager)
    [HideInInspector] public Vector2Int myGridPos;
    [HideInInspector] public GridManager gridManager;

    void OnEnable()
    {
        TimeManager.OnDayPassed += HandleDayPassed;
    }

    void OnDisable()
    {
        TimeManager.OnDayPassed -= HandleDayPassed;
    }

    // 🔥 [แก้ไขและอัปเดตใหม่]: ฟังก์ชันที่ GridManager จะเรียกใช้ตอนเสกตึกนี้ขึ้นมาในโลก
    public void Setup(int income, int cost, Vector2Int pos, GridManager manager)
    {
        incomePerTick = income;
        constructionCost = cost;
        
        // 🔥 จำพิกัดและจำตัว GridManager ไว้ใช้สแกนหาเพื่อนบ้าน
        myGridPos = pos;
        gridManager = manager;

        // วางตึกเสร็จปุ๊บ ให้รันระบบเช็คสถานะถนนของตัวเองทันที 1 รอบค่ะ
        CheckRoadConnection();
    }

    void HandleDayPassed()
    {
        // เงื่อนไขในการผลิตเงิน: ถ้าเป็น "ถนน" เอง หรือถ้าเป็น "บ้านที่เชื่อมต่อกับถนนสำเร็จ" ถึงจะยอมให้ผลิตเงินค่ะ
        if (gameObject.name.Contains("Road") || isConnectedToRoad)
        {
            ResourceManager rm = FindFirstObjectByType<ResourceManager>();
            if (rm != null)
            {
                rm.AddGold(incomePerTick);
                Debug.Log(gameObject.name + " ผลิตรายได้เรียบร้อยแล้วค่ะ: +" + incomePerTick + " Gold");
            }
        }
    }

    // ฟังก์ชันตรวจสอบสิ่งปลูกสร้างรอบตัว 4 ทิศ เพื่ออัปเดตสถานะการเชื่อมต่อถนน
    public void CheckRoadConnection()
    {
        // ถ้าเป็นวัตถุประเภทถนนอยู่แล้ว ไม่จำเป็นต้องเช็คหาถนนอีกรอบค่ะ
        if (gameObject.name.Contains("Road"))
        {
            isConnectedToRoad = true;
            
            // 🔥 [พิเศษสำหรับระบบถนน]: ถ้าตัวฉันเองเป็นถนน และพิกัดพร้อมแล้ว ให้สั่งตัวคุมสไปรต์เปลี่ยนรูปร่างทันที!
            if (TryGetComponent(out RoadController roadCtrl))
            {
                roadCtrl.UpdateRoadSprite();
            }
            return;
        }

        // --- ส่วนของบ้านธรรมดา เช็คหาถนนรอบตัว 4 ทิศเหมือนเดิมค่ะ ---
        if (gridManager == null) return;

        Vector2Int[] neighbors = new Vector2Int[]
        {
            new Vector2Int(myGridPos.x + 1, myGridPos.y), // ขวา
            new Vector2Int(myGridPos.x - 1, myGridPos.y), // ซ้าย
            new Vector2Int(myGridPos.x, myGridPos.y + 1), // บน
            new Vector2Int(myGridPos.x, myGridPos.y - 1)  // ล่าง
        };

        isConnectedToRoad = false;

        foreach (var p in neighbors)
        {
            GameObject neighborObj = gridManager.GetBuildingAt(p);
            if (neighborObj != null)
            {
                // ถ้าสแกนเจอว่ามีบล็อกใดบล็อกหนึ่งรอบตัวมีชื่อขึ้นต้นหรือประกอบด้วยคำว่า "Road"
                if (neighborObj.name.Contains("Road"))
                {
                    isConnectedToRoad = true;
                    break; 
                }
            }
        }
    }
}