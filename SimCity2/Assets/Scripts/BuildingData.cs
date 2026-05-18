using UnityEngine;

[System.Serializable] // เพื่อให้แสดงผลใน Inspector ได้
public class BuildingData
{
    public string name;
    public GameObject prefab;
    public int cost;
    public int incomePerTick;
}