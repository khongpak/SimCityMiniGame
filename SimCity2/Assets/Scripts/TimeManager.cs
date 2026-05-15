using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public float tickInterval = 2.0f; // 1 Tick ทุกๆ 2 วินาที (ปรับเปลี่ยนได้ใน Inspector)
    private float timer;

    // Event สำหรับให้สคริปต์อื่นมาลงทะเบียนรับฟัง
    public static event Action OnTick;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= tickInterval)
        {
            timer = 0f;
            // ส่งสัญญาณแจ้งเตือนทุกสคริปต์ที่รอฟังอยู่
            OnTick?.Invoke();
        }
    }
}