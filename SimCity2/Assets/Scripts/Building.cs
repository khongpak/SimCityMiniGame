using UnityEngine;

public class Building : MonoBehaviour
{
    [HideInInspector]public int incomePerTick;
    [HideInInspector]public int constructionCost;
    
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