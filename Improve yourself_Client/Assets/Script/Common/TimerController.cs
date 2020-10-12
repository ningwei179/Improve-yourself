using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 定时器控制器
/// </summary>
public class TimerController : Singleton<TimerController>
{
    //定时器相关；
    private int guid = 10001;
    private List<Timer> timers = new List<Timer>();       //update中的定时器，不精确；
    private List<Timer> fixedTimers = new List<Timer>();  //fixedUpdate中的定时器，精确；



    //时间缩放相关；
    private float startTime = 0;
    private int timeScaleType = 0;
    private int indexCurve = 0;
    private bool isTimeScale = false;
    private float timeTotal = 0;             //时间缩放时长；
    public List<AnimationCurve> timeCurves = new List<AnimationCurve>();//时间缩放曲线；
    public List<int> timeTotals = new List<int>(); //曲线对应时长；毫秒；

    private List<Timer> m_NeedDelete = new List<Timer>();

    /// <summary>
    /// fiexedUpdate 中 更新的Timer; 相对精准；
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="repeatCount"></param>
    /// <returns></returns>
    public Timer createFixedTimer(float delay, int repeatCount = 0)
    {
        Timer timer = new Timer(delay, repeatCount);
        fixedTimers.Add(timer);
        timer.id = guid;
        guid++;
        return timer;
    }


    //Timer管理；
    public Timer createTimer(float delay, int repeatCount = 0)
    {
        Timer timer = new Timer(delay, repeatCount);
        timers.Add(timer);
        timer.id = guid;
        guid++;
        return timer;
    }

    public void deleteTimer(Timer timer)
    {
        timer.stop();
        TimeOver(timer);
    }

    public void fixedUpdate()
    {
        Timer timer = null;
        for (int i = 0; i < fixedTimers.Count; i++)
        {
            timer = fixedTimers[i];
            timer.update();
        }
    }

    public void update()
    {
        Timer timer = null;  //定时器；
        for (int i = 0; i < timers.Count; i++)
        {
            timer = timers[i];
            timer.update();
        }

        if (m_NeedDelete.Count > 0)
        {
            for (int i = 0; i < m_NeedDelete.Count; i++)
            {
                if (fixedTimers.Contains(m_NeedDelete[i]))
                    fixedTimers.Remove(m_NeedDelete[i]);
                if (timers.Contains(m_NeedDelete[i]))
                    timers.Remove(m_NeedDelete[i]);
            }
            m_NeedDelete.Clear();
        }
    }

    public void clear()
    {

        for (int i = 0, len = timers.Count; i < len; i++)
        {
            timers[i].stop();
        }

        for (int i = 0, len = fixedTimers.Count; i < len; i++)
        {
            fixedTimers[i].stop();
        }

        timers.Clear();
        fixedTimers.Clear();
    }

    public void TimeOver(Timer timer)
    {
        if (!m_NeedDelete.Contains(timer))
            m_NeedDelete.Add(timer);
    }

}