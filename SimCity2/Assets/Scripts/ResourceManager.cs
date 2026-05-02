using UnityEngine;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    public int gold = 100;
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

    void Start()
    {
        UpdateUI();
    }

    // ฟังก์ชันที่จะทำงานเมื่อเกิด Event
    void HandleBuildingPlaced(int cost)
    {
        DeductGold(cost);
    }

    public void AddGold(int amount)
    {
        gold += amount;
        UpdateUI();
    }

    public void DeductGold(int amount)
    {
        gold -= amount;
        if (gold < 0) gold = 0;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (goldText != null)
        {
            goldText.text = "Gold: " + gold;
        }
    }
}