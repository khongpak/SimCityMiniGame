using UnityEngine;

public class Building : MonoBehaviour
{
    public int incomePerTick = 5; // ตึกนี้ผลิตเงินเท่าไหร่ต่อ 1 Tick
    private ResourceManager resourceManager;

    void Start()
    {
        // ค้นหา ResourceManager ในฉาก
        resourceManager = FindFirstObjectByType<ResourceManager>();
    }

    void OnEnable()
    {
        // เมื่อวางตึก ให้เริ่มฟังคำสั่ง Tick
        TimeManager.OnMonthPassed += ProduceResources;
    }

    void OnDisable()
    {
        // เมื่อตึกถูกลบ ให้หยุดฟัง
        TimeManager.OnMonthPassed -= ProduceResources;
    }

    void ProduceResources()
    {
        if (resourceManager != null)
        {
            resourceManager.AddGold(incomePerTick);
        }
    }
}