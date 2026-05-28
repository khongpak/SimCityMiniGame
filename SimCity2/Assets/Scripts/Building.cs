using UnityEngine;

public class Building : MonoBehaviour
{
    [Header("Building Income Settings")]
    public int incomePerTick = 10;
    public int constructionCost = 50;
    
    // 🔥 [เพิ่มเข้ามาใหม่]: ตัวแปรจำค่าพลังงานไฟฟ้าของตึกนี้ (บวก = ผลิต, ลบ = ใช้ไฟ)
    [Header("Power Settings")]
    public int powerGeneratedOrConsumed = 0; 

    [Header("Status")]
    public bool isConnectedToRoad = false;

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

    // 🔥 [อัปเดตฟังก์ชัน Setup ให้รับค่าพลังงานไฟฟ้าเข้ามาด้วยค่ะ]
    public void Setup(int income, int cost, int power, Vector2Int pos, GridManager manager)
    {
        incomePerTick = income;
        constructionCost = cost;
        powerGeneratedOrConsumed = power; // จำค่าพลังงานไฟฟ้า
        myGridPos = pos;
        gridManager = manager;

        CheckRoadConnection();
    }

    void HandleDayPassed()
    {
        ResourceManager rm = FindFirstObjectByType<ResourceManager>();
        
        // เช็คสถานะไฟฟ้าส่วนกลางก่อนเบื้องต้น (ถ้าไฟฟ้ารวมที่ใช้ ดันมากกว่า ไฟฟ้าที่มี แสดงว่าไฟตก/ไฟดับทั้งเมือง)
        bool isPowerOvertaxed = false;
        if (rm != null)
        {
            isPowerOvertaxed = rm.currentPowerUsed > rm.maxPowerAvailable;
        }

        // เงื่อนไขในการผลิตเงิน:
        // 1. ตัวเองเป็นถนน (ถนนผลิตเงินได้เสมอถ้าตั้งค่าไว้ ไม่ใช้ไฟฟ้า)
        // 2. เป็นบ้านที่เชื่อมถนนสำเร็จ AND (เป็นตึกที่ไม่ได้ใช้ไฟ หรือ ระบบไฟส่วนกลางใช้งานได้ปกติ)
        bool canProduceIncome = false;

        if (gameObject.name.Contains("Road"))
        {
            canProduceIncome = true;
        }
        else if (isConnectedToRoad)
        {
            // ถ้าตึกนี้ต้องการไฟ (ค่าพลังงานเป็นลบ) แต่ตอนนี้น้ำไฟเข้าไม่ถึง/ไฟตก เมืองโหลดเกินพิกัด จะไม่ผลิตเงินค่ะ
            if (powerGeneratedOrConsumed < 0 && isPowerOvertaxed)
            {
                Debug.LogWarning(gameObject.name + " ไฟดับ! ไม่สามารถผลิตรายได้ได้ในรอบนี้ค่ะ");
                canProduceIncome = false;
            }
            else
            {
                canProduceIncome = true;
            }
        }

        if (canProduceIncome && rm != null)
        {
            rm.AddGold(incomePerTick);
            Debug.Log(gameObject.name + " ผลิตรายได้เรียบร้อยแล้วค่ะ: +" + incomePerTick + " Gold");
        }
    }

    public void CheckRoadConnection()
    {
        if (gameObject.name.Contains("Road"))
        {
            isConnectedToRoad = true;
            if (TryGetComponent(out RoadController roadCtrl))
            {
                roadCtrl.UpdateRoadSprite();
            }
            return;
        }

        if (gridManager == null) return;

        Vector2Int[] neighbors = new Vector2Int[]
        {
            new Vector2Int(myGridPos.x + 1, myGridPos.y),
            new Vector2Int(myGridPos.x - 1, myGridPos.y),
            new Vector2Int(myGridPos.x, myGridPos.y + 1),
            new Vector2Int(myGridPos.x, myGridPos.y - 1)
        };

        isConnectedToRoad = false;

        foreach (var p in neighbors)
        {
            GameObject neighborObj = gridManager.GetBuildingAt(p);
            if (neighborObj != null)
            {
                if (neighborObj.name.Contains("Road"))
                {
                    isConnectedToRoad = true;
                    break; 
                }
            }
        }
    }
}