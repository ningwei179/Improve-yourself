/****************************************************
	文件：TimerSvc.cs
	作者：Plane
	邮箱: 1785275942@qq.com
	日期：2019/02/24 5:56   	
	功能：计时服务
*****************************************************/

using System;
namespace Improve
{
    public class TimerController : Singleton<TimerController>
    {
        private Timer timer;

        public void Init()
        {
            timer = new Timer();
        }

        public void Update()
        {
            timer.Update();
        }

        public int AddTimeTask(Action<int> callback, double delay, TimeUnit timeUnit = TimeUnit.Millisecond, int count = 1)
        {
            return timer.AddTimeTask(callback, delay, timeUnit, count);
        }

        public double GetNowTime()
        {
            return timer.GetMillisecondsTime();
        }

        public void DelTask(int tid)
        {
            timer.DeleteTimeTask(tid);
        }
    }
}