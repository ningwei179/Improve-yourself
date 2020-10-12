using UnityEngine;
/*===================================================
* 类名称: Timer
* 类描述: Timer定时器
=====================================================*/
public class Timer
{
    public delegate void TimerEvent(Timer timer);

    public int id = -1;
    public string name = "";
    public float delay = 0;
    public object[] param = null;

    private TimerEvent onTimer = null;
    private TimerEvent onTimerComplete = null;
    public bool isRuning = false;


    public int repeatCount = 0;
    private float lastUpdateTime = 0;

    private bool isCircly = false;



    public void addTimerEventListener(TimerEvent onTimer)
    {
        this.onTimer = onTimer;
    }

    public void addTimerCompleteEventListener(TimerEvent onTimerComplete)
    {
        this.onTimerComplete = onTimerComplete;
    }


    /// <summary>
    /// 定时器；
    /// </summary>
    /// <param name="delay">秒数</param>
    /// <param name="circlyNum">循环次数</param>
    public Timer(float delay, int circlyNum = 0, bool autoStart = true)
    {
        this.delay = delay;
        this.repeatCount = circlyNum;
        if (autoStart)
            start();
    }

    public void start()
    {
        lastUpdateTime = Time.fixedTime;
        isRuning = true;
        isCircly = (repeatCount == 0);
    }

    public void start(float delay, int repeatCount = 0)
    {
        this.isCircly = (repeatCount == 0);
        this.repeatCount = repeatCount;
        this.delay = delay;
        this.lastUpdateTime = Time.fixedTime;
        this.isRuning = true;
    }

    public void stop()
    {
        lastUpdateTime = 0;
        repeatCount = 0;
        delay = 0;
        isRuning = false;
    }

    public void update()
    {
        if (!isRuning) return;
        float curTime = Time.fixedTime;
        if (curTime - lastUpdateTime < delay) return;

        //是循环TIMER时；
        if (isCircly)
        {
            onTimer?.Invoke(this);
            lastUpdateTime = curTime;
            return;
        }

        if (repeatCount > 1)
        {
            repeatCount--;
            onTimer?.Invoke(this);
            lastUpdateTime = curTime;
        }
        else
        {
            repeatCount--;
            onTimer?.Invoke(this);
            onTimerComplete?.Invoke(this);
            stop();
            TimerController.Instance.TimeOver(this);
        }
    }
}
