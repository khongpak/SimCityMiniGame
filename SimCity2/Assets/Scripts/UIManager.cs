using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI dateText; // ลาก Text อันใหม่มาใส่ที่นี่
    public ResourceManager resourceManager;
    public TimeManager timeManager; // ลาก TimeManager มาใส่

    void Update()
    {
        if (resourceManager != null && goldText != null)
            goldText.text = "Gold: " + resourceManager.gold.ToString();

        if (timeManager != null && dateText != null)
            dateText.text = "Date: " + timeManager.GetDateString();
    }
}