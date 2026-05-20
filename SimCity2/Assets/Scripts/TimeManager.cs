using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public float tickInterval = 2.0f; 
    private float timer;

    // ข้อมูลปฏิทิน
    public int day = 1;
    public int month = 1;
    public int year = 2024;

    public static event Action OnDayPassed;
    public static event Action OnMonthPassed; // เอาไว้เก็บภาษีรายเดือน

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= tickInterval)
        {
            timer = 0f;
            CalculateDate();
        }
    }

    void CalculateDate()
    {
        day++;
        if (day > 30) // สมมติให้ 1 เดือนมี 30 วันเพื่อความง่าย
        {
            day = 1;
            month++;
            OnMonthPassed?.Invoke(); // แจ้งเตือนระบบเก็บภาษี
        }
        if (month > 12)
        {
            month = 1;
            year++;
        }
        
        OnDayPassed?.Invoke(); // แจ้งเตือนให้ตึกผลิตรายได้ (รายวัน)
    }

    // ฟังก์ชันช่วยจัดรูปแบบวันที่สวยๆ เช่น "01/05/2024"
    public string GetDateString()
    {
        return $"{day:D2}/{month:D2}/{year}";
    }
}