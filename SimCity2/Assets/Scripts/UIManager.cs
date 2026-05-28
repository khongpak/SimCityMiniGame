using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI dateText; // ลาก Text อันใหม่มาใส่ที่นี่
    public TextMeshProUGUI powerText;
    public ResourceManager resourceManager;
    public TimeManager timeManager; // ลาก TimeManager มาใส่

    void Update()
    {
        if (resourceManager != null && goldText != null)
            goldText.text = "Gold: " + resourceManager.gold.ToString();

        if (timeManager != null && dateText != null)
            dateText.text = "Date: " + timeManager.GetDateString();

        // 🔥 [เพิ่มเข้ามาใหม่]: สั่งอัปเดตข้อความระบบไฟฟ้าทุกเฟรมคัป
        if (resourceManager != null && powerText != null)
        {
            powerText.text = $"Power: {resourceManager.currentPowerUsed}/{resourceManager.maxPowerAvailable}";
        }
    }
}