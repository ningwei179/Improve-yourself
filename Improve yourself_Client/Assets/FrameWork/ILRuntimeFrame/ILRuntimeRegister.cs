using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ILRuntimeRegister : Singleton<ILRuntimeRegister>
{
    internal void InitializeIlRuntime()
    {
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
        //由于Unity的Profiler接口只允许在主线程使用，为了避免出异常，需要告诉ILRuntime主线程的线程ID才能正确将函数运行耗时报告给Profiler
        ILRuntimeManager.Instance.ILRunAppDomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif
        ILRuntimeManager.Instance.ILRunAppDomain.RegisterCrossBindingAdaptor(new IYMonoBehaviourAdapter());
        ILRuntimeManager.Instance.ILRunAppDomain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());
    }
}
