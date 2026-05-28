using UnityEngine;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    public int gold { get; private set; } = 100;
    
    [Header("Power System Settings")]
    public int currentPowerUsed = 0;
    public int maxPowerAvailable = 0;
      

    void OnEnable()
    {
        GridManager.OnBuildingPlaced += HandleBuildingPlaced;
    }

    void OnDisable()
    {
        GridManager.OnBuildingPlaced -= HandleBuildingPlaced;
    }

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

    public void RefundGold(int amount)
    {
        gold += amount;
    }

    // 🔥 [ฟังก์ชันเพิ่มเข้ามาใหม่]: สำหรับอัปเดตระบบไฟฟ้าเมื่อมีการสร้างหรือทุบตึก
    public void UpdatePowerGrid(int amount)
    {
        if (amount > 0)
        {
            // ถ้าค่าเป็นบวก แปลว่าเป็นสิ่งปลูกสร้างที่ผลิตไฟฟ้า (เช่น โรงไฟฟ้า)
            maxPowerAvailable += amount;
        }
        else
        {
            // ถ้าค่าเป็นลบ แปลว่าเป็นตึกที่ดึงไฟฟ้าไปใช้ (เราจะแปลงเป็นค่าบวกเพื่อสะสมในค่าที่ถูกใช้ไป)
            currentPowerUsed += Mathf.Abs(amount);
        }
    }

    // 🔥 [ฟังก์ชันเพิ่มเข้ามาใหม่]: สำหรับคืนค่าไฟฟ้ากลับเข้าระบบตอนทุบตึกทิ้ง
    public void ReleasePower(int amount)
    {
        if (amount > 0)
        {
            // ทุบโรงไฟฟ้า -> พลังงานที่มีให้ใช้ลดลง
            maxPowerAvailable -= amount;
        }
        else
        {
            // ทุบตึกธรรมดา -> คืนพลังงานที่เคยดึงไปใช้กลับสู่ส่วนกลาง
            currentPowerUsed -= Mathf.Abs(amount);
        }
    }

    // 🔥 [ฟังก์ชันเพิ่มเข้ามาใหม่]: เช็คว่าพลังงานไฟฟ้าส่วนกลางเหลือพอให้ตึกใหม่เปิดใช้งานไหม
    public bool HasEnoughPower(int amountRequired)
    {
        // ตึกที่ต้องการไฟฟ้า ค่าใน BuildingData จะเป็นลบ (เช่น -5) 
        // เราจึงใช้ Mathf.Abs เพื่อดูจำนวนที่ต้องการจริง ๆ
        int required = Mathf.Abs(amountRequired);
        return (currentPowerUsed + required) <= maxPowerAvailable;
    }
}