using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class TimeManager : MonoBehaviour
{
    public float TimeScale;

    public static TimeManager Instance
    {
        get {
            return m_instance;
        }
    }
    private static TimeManager m_instance;

    public CommonDelegates.SimpleDelegate OnHourPassed;

    private float m_timerSinceLastHour;
    float m_lastTime;
    private void Awake()
    {
        if (m_instance != null && m_instance != this)
        {
            Destroy(gameObject);
            return;
        }
        m_instance = this;
    }

    private void Update()
    {
        m_timerSinceLastHour += TimeScale * (Time.time - m_lastTime);
        m_lastTime = Time.time;
        if (m_timerSinceLastHour >= (60*60))
        {
            if (OnHourPassed != null)
                OnHourPassed();
            m_timerSinceLastHour = 0;
        }
    }
}
