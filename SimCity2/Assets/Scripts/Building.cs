using UnityEngine;

public class Building : MonoBehaviour
{
    [HideInInspector]public int incomePerTick;
    [HideInInspector]public int constructionCost;
    
    private ResourceManager resourceManager;
    private GridManager gridManager; // ต้องใช้อ้างอิงเพื่อเช็คตึกข้างๆ
    private Vector2Int myGridPos;   // ตำแหน่งของตึกนี้ใน Grid

    // ตัวแปรเช็คสถานะ
    [Header("Status")]
    public bool isConnectedToRoad = false;

    public void Setup(int income, int cost, Vector2Int pos, GridManager gm)
    {
        incomePerTick = income;
        constructionCost = cost;
        myGridPos = pos;
        gridManager = gm;
        resourceManager = FindFirstObjectByType<ResourceManager>();
        
        // เช็คถนนครั้งแรกเมื่อสร้างเสร็จ
        CheckRoadConnection();
    }

    public void CheckRoadConnection()
    {
        // ตรวจสอบช่องข้างๆ (บน ล่าง ซ้าย ขวา)
        Vector2Int[] neighbors = new Vector2Int[]
        {
            new Vector2Int(myGridPos.x + 1, myGridPos.y),
            new Vector2Int(myGridPos.x - 1, myGridPos.y),
            new Vector2Int(myGridPos.x, myGridPos.y + 1),
            new Vector2Int(myGridPos.x, myGridPos.y - 1)
        };

        isConnectedToRoad = false;

        foreach (var pos in neighbors)
        {
            // ดึงข้อมูลตึกที่อยู่ในช่องข้างๆ จาก GridManager
            GameObject neighborObj = gridManager.GetBuildingAt(pos);
            if (neighborObj != null)
            {
                // ถ้าตึกข้างๆ มีชื่อว่า "Road" (หรือเช็คผ่าน Tag/Component ก็ได้)
                if (neighborObj.name.Contains("Road")) 
                {
                    isConnectedToRoad = true;
                    break;
                }
            }
        }
    }

    // แก้ไขระบบผลิตเงิน
    private void ProduceResources()
    {
        // ถ้าไม่ติดถนน จะไม่ผลิตเงิน!
        if (!isConnectedToRoad) return; 

        if (resourceManager != null)
        {
            resourceManager.AddGold(incomePerTick);
        }
    }

    void Start()
    {
        // ค้นหา ResourceManager ในฉาก
        resourceManager = FindFirstObjectByType<ResourceManager>();
    }

    void OnEnable()
    {
        // เมื่อวางตึก ให้เริ่มฟังคำสั่ง Tick
        TimeManager.OnDayPassed += ProduceResources;
    }

    void OnDisable()
    {
        // เมื่อตึกถูกลบ ให้หยุดฟัง
        TimeManager.OnDayPassed -= ProduceResources;
    }

    
}