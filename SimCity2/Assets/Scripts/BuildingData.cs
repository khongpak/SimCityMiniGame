using UnityEngine;

// ✨ เติมบรรทัดนี้เพื่อให้คลิกขวาในโฟลเดอร์ Project แล้วสร้างไฟล์ข้อมูลตึกได้คัป
[CreateAssetMenu(fileName = "NewBuildingData", menuName = "CityBuilder/Building Data")]
public class BuildingData : ScriptableObject // 🌟 เปลี่ยนจากคลาสธรรมดาเป็น ScriptableObject คัป
{
    public string buildingName;
    public GameObject prefab;
    public int cost;
    public int incomePerTick;
    public int powerGeneratedOrConsumed; // ช่องกรอกระบบไฟฟ้า
}