using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ILRuntimeRegister : Singleton<ILRuntimeRegister>
{
    internal void InitializeIlRuntime()
    {
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
        //����Unity��Profiler�ӿ�ֻ���������߳�ʹ�ã�Ϊ�˱�����쳣����Ҫ����ILRuntime���̵߳��߳�ID������ȷ���������к�ʱ�����Profiler
        ILRuntimeManager.Instance.ILRunAppDomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif
        ILRuntimeManager.Instance.ILRunAppDomain.RegisterCrossBindingAdaptor(new IYMonoBehaviourAdapter());
        ILRuntimeManager.Instance.ILRunAppDomain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());
    }
}
