using UnityEngine;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    public int gold {get; private set;}= 100;
    public TMP_Text goldText;

    void OnEnable()
    {
        // ลงทะเบียนรับฟัง Event
        GridManager.OnBuildingPlaced += HandleBuildingPlaced;
    }

    void OnDisable()
    {
        // ยกเลิกการรับฟังเพื่อป้องกัน Memory Leak
        GridManager.OnBuildingPlaced -= HandleBuildingPlaced;
    }


    // ฟังก์ชันที่จะทำงานเมื่อเกิด Event
    void HandleBuildingPlaced(int cost)
    {
        DeductGold(cost);
    }

    public void AddGold(int amount)
    {
        gold += amount;

    }

    public void DeductGold(int amount)
    {
        gold -= amount;
        if (gold < 0) gold = 0;
    }

}