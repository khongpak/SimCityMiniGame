using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI moneyText; 
    public ResourceManager resourceManager; 

    void Update()
    {
        // เปลี่ยนจาก currentMoney เป็น gold ตามที่คุณเพชรตั้งชื่อไว้
        if (resourceManager != null && moneyText != null)
        {
            moneyText.text = "Gold: " + resourceManager.gold.ToString();
        }
    }
}