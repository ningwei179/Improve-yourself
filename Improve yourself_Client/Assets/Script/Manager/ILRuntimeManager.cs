/****************************************************
    文件：ILRuntimeManager.cs
	作者：NingWei
    日期：2020/10/13 15:49:48
	功能：ILRuntime热更管理类
*****************************************************/
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
#region 测试代码
//public abstract class TestClassBase
//{
//    public virtual int Value
//    {
//        get { return 0; }
//    }

//    public virtual void TestVirtual(string str)
//    {
//        Debug.Log("TestClassBase TestVirtual   str=" + str);
//    }

//    public abstract void TestAbstract(int a);
//}

//public class InheritanceAdapter : CrossBindingAdaptor
//{
//    public override System.Type BaseCLRType
//    {
//        get
//        {
//            //想继承的类
//            return typeof(TestClassBase);
//        }
//    }

//    public override System.Type AdaptorType
//    {
//        get
//        {
//            //实际的适配器类
//            return typeof(Adapter);
//        }
//    }

//    public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
//    {
//        return new Adapter(appdomain, instance);
//    }

//    class Adapter : TestClassBase, CrossBindingAdaptorType
//    {
//        private ILRuntime.Runtime.Enviorment.AppDomain m_Appdomain;
//        private ILTypeInstance m_Instance;
//        private IMethod m_TestAbstract;
//        private IMethod m_TestVirtual;
//        private IMethod m_GetValue;
//        private IMethod m_ToString;
//        object[] param1 = new object[1];
//        private bool m_TestVirtualInvoking = false;
//        private bool m_GetValueInvoking = false;

//        public Adapter()
//        {

//        }

//        public Adapter(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
//        {
//            m_Appdomain = appdomain;
//            m_Instance = instance;
//        }

//        public ILTypeInstance ILInstance
//        {
//            get
//            {
//                return m_Instance;
//            }
//        }

//        //在适配器中重写所有需要在热更脚本重写的方法，并且将控制权转移到脚本里去
//        public override void TestAbstract(int a)
//        {
//            if (m_TestAbstract == null)
//            {
//                m_TestAbstract = m_Instance.Type.GetMethod("TestAbstract", 1);
//            }

//            if (m_TestAbstract != null)
//            {
//                param1[0] = a;
//                m_Appdomain.Invoke(m_TestAbstract, m_Instance, param1);
//            }
//        }

//        public override void TestVirtual(string str)
//        {
//            if (m_TestVirtual == null)
//            {
//                m_TestVirtual = m_Instance.Type.GetMethod("TestVirtual", 1);
//            }

//            //必须要设定一个标识位来表示当前是否在调用中, 否则如果脚本类里调用了base.TestVirtual()就会造成无限循环
//            if (m_TestVirtual != null && !m_TestVirtualInvoking)
//            {
//                m_TestVirtualInvoking = true;
//                param1[0] = str;
//                m_Appdomain.Invoke(m_TestVirtual, m_Instance, param1);
//                m_TestVirtualInvoking = false;
//            }
//            else
//            {
//                base.TestVirtual(str);
//            }
//        }

//        public override int Value
//        {
//            get
//            {
//                if (m_GetValue == null)
//                {
//                    m_GetValue = m_Instance.Type.GetMethod("get_Value", 1);
//                }

//                if (m_GetValue != null && !m_GetValueInvoking)
//                {
//                    m_GetValueInvoking = true;
//                    int res = (int)m_Appdomain.Invoke(m_GetValue, m_Instance, null);
//                    m_GetValueInvoking = false;
//                    return res;
//                }
//                else
//                {
//                    return base.Value;
//                }
//            }
//        }

//        public override string ToString()
//        {
//            if (m_ToString == null)
//            {
//                m_ToString = m_Appdomain.ObjectType.GetMethod("ToString", 0);
//            }
//            IMethod m = m_Instance.Type.GetVirtualMethod(m_ToString);
//            if (m == null || m is ILMethod)
//            {
//                return m_Instance.ToString();
//            }
//            else
//            {
//                return m_Instance.Type.FullName;
//            }
//        }
//    }
//}

//public delegate void TestDelegateMeth(int a);
//public delegate string TestDelegateFunction(int a);

//public class CLRBingdingTestClass
//{
//    public static float DoSomeTest(int a, float b)
//    {
//        return a + b;
//    }
//}

//public class CoroutineAdapter : CrossBindingAdaptor
//{
//    public override System.Type BaseCLRType
//    {
//        get
//        {
//            return null;
//        }
//    }

//    public override System.Type AdaptorType
//    {
//        get
//        {
//            return typeof(Adaptor);
//        }
//    }

//    public override System.Type[] BaseCLRTypes
//    {
//        get
//        {
//            return new System.Type[] { typeof(IEnumerator<object>), typeof(IEnumerator), typeof(System.IDisposable) };
//        }
//    }

//    public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
//    {
//        return new Adaptor(appdomain, instance);
//    }

//    public class Adaptor : IEnumerator<System.Object>, IEnumerator, System.IDisposable, CrossBindingAdaptorType
//    {
//        private ILTypeInstance m_Instance;
//        private ILRuntime.Runtime.Enviorment.AppDomain m_Appdamain;
//        private IMethod m_CurMethod;
//        private IMethod m_DisposeMethod;
//        private IMethod m_MoveNextMethod;
//        private IMethod m_ResetMethod;
//        private IMethod m_ToString;

//        public Adaptor()
//        {

//        }

//        public Adaptor(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
//        {
//            m_Instance = instance;
//            m_Appdamain = appdomain;
//        }


//        public object Current
//        {
//            get
//            {
//                if (m_CurMethod == null)
//                {
//                    m_CurMethod = m_Instance.Type.GetMethod("get_Current", 0);
//                    if (m_CurMethod == null)
//                    {
//                        m_CurMethod = m_Instance.Type.GetMethod("System.Collections.IEnumerator.get_Current", 0);
//                    }
//                }

//                if (m_CurMethod != null)
//                {
//                    var res = m_Appdamain.Invoke(m_CurMethod, m_Instance, null);
//                    return res;
//                }
//                else
//                {
//                    return null;
//                }
//            }
//        }

//        public ILTypeInstance ILInstance
//        {
//            get
//            {
//                return m_Instance;
//            }
//        }

//        public void Dispose()
//        {
//            if (m_DisposeMethod == null)
//            {
//                m_DisposeMethod = m_Instance.Type.GetMethod("Dispose", 0);
//                if (m_DisposeMethod == null)
//                {
//                    m_DisposeMethod = m_Instance.Type.GetMethod("System.IDisposable.Dispose", 0);
//                }
//            }

//            if (m_DisposeMethod != null)
//            {
//                m_Appdamain.Invoke(m_DisposeMethod, m_Instance, null);
//            }
//        }

//        public bool MoveNext()
//        {
//            if (m_MoveNextMethod == null)
//            {
//                m_MoveNextMethod = m_Instance.Type.GetMethod("MoveNext", 0);
//            }

//            if (m_MoveNextMethod != null)
//            {
//                return (bool)m_Appdamain.Invoke(m_MoveNextMethod, m_Instance, null);
//            }
//            else
//            {
//                return false;
//            }
//        }

//        public void Reset()
//        {
//            if (m_ResetMethod == null)
//            {
//                m_ResetMethod = m_Instance.Type.GetMethod("Reset", 0);
//            }

//            if (m_ResetMethod != null)
//            {
//                m_Appdamain.Invoke(m_ResetMethod, m_Instance, null);
//            }
//        }

//        public override string ToString()
//        {
//            if (m_ToString == null)
//            {
//                m_ToString = m_Appdamain.ObjectType.GetMethod("ToString", 0);
//            }
//            IMethod m = m_Instance.Type.GetVirtualMethod(m_ToString);
//            if (m == null || m is ILMethod)
//            {
//                return m_Instance.ToString();
//            }
//            else
//            {
//                return m_Instance.Type.FullName;
//            }
//        }
//    }
//}

//public class MonoBehaviourAdapter : CrossBindingAdaptor
//{
//    public override System.Type BaseCLRType
//    {
//        get
//        {
//            return typeof(MonoBehaviour);
//        }
//    }

//    public override System.Type AdaptorType
//    {
//        get { return typeof(Adaptor); }
//    }

//    public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
//    {
//        return new Adaptor(appdomain, instance);
//    }

//    public class Adaptor : MonoBehaviour, CrossBindingAdaptorType
//    {
//        private ILRuntime.Runtime.Enviorment.AppDomain m_Appdomain;
//        private ILTypeInstance m_Instance;
//        private IMethod m_AwakeMethod;
//        private IMethod m_StartMethod;
//        private IMethod m_UpdateMethod;
//        private IMethod m_ToString;

//        public Adaptor() { }

//        public Adaptor(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
//        {
//            m_Appdomain = appdomain;
//            m_Instance = instance;
//        }

//        public ILTypeInstance ILInstance
//        {
//            get
//            {
//                return m_Instance;
//            }
//            set
//            {
//                m_Instance = value;
//                m_AwakeMethod = null;
//                m_StartMethod = null;
//                m_UpdateMethod = null;
//            }
//        }

//        public ILRuntime.Runtime.Enviorment.AppDomain AppDomain
//        {
//            get { return m_Appdomain; }
//            set { m_Appdomain = value; }
//        }

//        public void Awake()
//        {
//            if (m_Instance != null)
//            {
//                if (m_AwakeMethod == null)
//                {
//                    m_AwakeMethod = m_Instance.Type.GetMethod("Awake", 0);
//                }

//                if (m_AwakeMethod != null)
//                {
//                    m_Appdomain.Invoke(m_AwakeMethod, m_Instance, null);
//                }
//            }
//        }

//        void Start()
//        {
//            if (m_StartMethod == null)
//            {
//                m_StartMethod = m_Instance.Type.GetMethod("Start", 0);
//            }

//            if (m_StartMethod != null)
//            {
//                m_Appdomain.Invoke(m_StartMethod, m_Instance, null);
//            }
//        }


//        void Update()
//        {
//            if (m_UpdateMethod == null)
//            {
//                m_UpdateMethod = m_Instance.Type.GetMethod("Update", 0);
//            }

//            if (m_UpdateMethod != null)
//            {
//                m_Appdomain.Invoke(m_UpdateMethod, m_Instance, null);
//            }
//        }

//        public override string ToString()
//        {
//            if (m_ToString == null)
//            {
//                m_ToString = m_Appdomain.ObjectType.GetMethod("ToString", 0);
//            }
//            IMethod m = m_Instance.Type.GetVirtualMethod(m_ToString);
//            if (m == null || m is ILMethod)
//            {
//                return m_Instance.ToString();
//            }
//            else
//            {
//                return m_Instance.Type.FullName;
//            }
//        }
//    }
//}
#endregion

public class ILRuntimeManager : Singleton<ILRuntimeManager>
{
    //public TestDelegateMeth DelegateMethod;
    //public TestDelegateFunction DelegateFunc;
    public System.Action<string> DelegateAction;

    private const string DLLPATH = "Assets/GameData/Data/HotFix/HotFixProject.dll.txt";
    private const string PDBPATH = "Assets/GameData/Data/HotFix/HotFixProject.pdb.txt";
    private ILRuntime.Runtime.Enviorment.AppDomain m_AppDomain;
    public ILRuntime.Runtime.Enviorment.AppDomain ILRunAppDomain
    {
        get { return m_AppDomain; }
    }

    public void Init()
    {
        LoadHotFixAssembly();
    }

    void LoadHotFixAssembly()
    {
        //整个工程只有一个ILRuntime的AppDomain
        m_AppDomain = new ILRuntime.Runtime.Enviorment.AppDomain();
        //读取热更资源的dll
        TextAsset dllText = ResourceManager.Instance.LoadResource<TextAsset>(DLLPATH);
        //PBD文件，调试数据可，日志报错
        TextAsset pdbText = ResourceManager.Instance.LoadResource<TextAsset>(PDBPATH);
        MemoryStream ms = new MemoryStream(dllText.bytes);
        //using (MemoryStream ms = new MemoryStream(dllText.bytes))
        //{
        MemoryStream p = new MemoryStream(pdbText.bytes);
        //using (MemoryStream p = new MemoryStream(pdbText.bytes))
        //{
        m_AppDomain.LoadAssembly(ms, null, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
            //}
        //}

        InitializeIlRuntime();
        OnHotFixLoaded();
    }

    void InitializeIlRuntime()
    {
        //默认委托注册仅仅支持系统自带的Action以及Function
        //m_AppDomain.DelegateManager.RegisterMethodDelegate<bool>();
        //m_AppDomain.DelegateManager.RegisterFunctionDelegate<int, string>();
        //m_AppDomain.DelegateManager.RegisterMethodDelegate<int>();
        //m_AppDomain.DelegateManager.RegisterMethodDelegate<string>();
        //m_AppDomain.DelegateManager.RegisterMethodDelegate<System.String, UnityEngine.Object, System.Object, System.Object, System.Object>();
        //自定义委托或Unity委托注册
        //m_AppDomain.DelegateManager.RegisterDelegateConvertor<TestDelegateMeth>((action) =>
        //{
        //    return new TestDelegateMeth((a) =>
        //    {
        //        ((System.Action<int>)action)(a);
        //    });
        //});

        //m_AppDomain.DelegateManager.RegisterDelegateConvertor<TestDelegateFunction>((action) =>
        //{
        //    return new TestDelegateFunction((a) =>
        //    {
        //        return ((System.Func<int, string>)action)(a);
        //    });
        //});

        //m_AppDomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<bool>>((action) =>
        //{
        //    return new UnityEngine.Events.UnityAction<bool>((a) =>
        //    {
        //        ((System.Action<bool>)action)(a);
        //    });
        //});

        //m_AppDomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction>((action) =>
        //{
        //    return new UnityEngine.Events.UnityAction(() =>
        //    {
        //        ((System.Action)action)();
        //    });
        //});

        //m_AppDomain.DelegateManager.RegisterDelegateConvertor<OnAsyncObjFinish>((action) =>
        //{
        //    return new OnAsyncObjFinish((path, obj, param1, param2, param3) =>
        //    {
        //        ((System.Action<System.String, UnityEngine.Object, System.Object, System.Object, System.Object>)action)(path, obj, param1, param2, param3);
        //    });
        //});

        //跨域继承的注册
        //m_AppDomain.RegisterCrossBindingAdaptor(new InheritanceAdapter());
        ////注册协程适配器
        //m_AppDomain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
        ////注册Mono适配器
        //m_AppDomain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
        //注册Window适配器
        //m_AppDomain.RegisterCrossBindingAdaptor(new WindowAdapter());

        //SetupCLRAddCompontent();
        //SetUpCLRGetCompontent();

        //绑定注册 (最后执行)
        //ILRuntime.Runtime.Generated.CLRBindings.Initialize(m_AppDomain);
    }

    void OnHotFixLoaded()
    {
        //第一个简单方法的调用
        m_AppDomain.Invoke("HotFixProject.InstanceClass", "StaticFunTest", null, null);

        //先单独获取类，之后一直使用这个类来调用
        //IType type = m_AppDomain.LoadedTypes["HotFix.TestClass"];

        //根据方法名称和参数个数获取方法(学习获取函数进行调用)
        //IMethod method = type.GetMethod("StaticFunTest", 0);
        //m_AppDomain.Invoke(method, null, null);

        //根据获取函数来调用有参的函数
        //第一种含参调用
        //IMethod method = type.GetMethod("StaticFunTest2", 1);
        //m_AppDomain.Invoke(method, null, 5);

        //第二种含参调用
        //IType intType = m_AppDomain.GetType(typeof(int));
        //List<IType> paraList = new List<IType>();
        //paraList.Add(intType);
        //IMethod method = type.GetMethod("StaticFunTest2", paraList, null);
        //m_AppDomain.Invoke(method, null, 5);

        //实例化热更工程里的类
        //第一种实例化(可以带参数)
        //object obj = m_AppDomain.Instantiate("HotFix.TestClass", new object[] { 15 });
        //第二种实例化（不带参数）
        //object obj = ((ILType)type).Instantiate();
        //int id = (int)m_AppDomain.Invoke("HotFix.TestClass", "get_ID", obj, null);
        //Debug.Log("TestClass 中 ID:" + id);

        //第一种泛型方法调用
        //IType stringType = m_AppDomain.GetType(typeof(string));
        //IType[] genericArguments = new IType[] { stringType };
        //m_AppDomain.InvokeGenericMethod("HotFix.TestClass", "GenericMethod", genericArguments, null, "Ocean");

        //paraList.Clear();
        //paraList.Add(stringType);
        //method = type.GetMethod("GenericMethod", paraList, genericArguments);
        //m_AppDomain.Invoke(method, null, "Ocean2222222222222");

        //-----------------------------------------------------------------------------------------------------------------

        //委托调用
        //热更内部委托调用
        //m_AppDomain.Invoke("HotFix.TestDele", "Initialize", null, null);
        //m_AppDomain.Invoke("HotFix.TestDele", "RunTest", null, null);

        //m_AppDomain.Invoke("HotFix.TestDele", "Initialize2", null, null);
        //m_AppDomain.Invoke("HotFix.TestDele", "RunTest2", null, null);

        //if (DelegateMethod != null)
        //{
        //    DelegateMethod(666);
        //}
        //if (DelegateFunc != null)
        //{
        //    string str = DelegateFunc(789);
        //    Debug.Log(str);
        //}
        //if (DelegateAction != null)
        //{
        //    DelegateAction("Ocean666");
        //}

        //-----------------------------------------------------------------------------------------------------------------

        //跨域继承
        //TestClassBase obj = m_AppDomain.Instantiate<TestClassBase>("HotFix.TestInheritance");
        //obj.TestAbstract(556);
        //obj.TestVirtual("Ocean");

        //TestClassBase obj = m_AppDomain.Invoke("HotFix.TestInheritance", "NewObject", null, null) as TestClassBase;
        //obj.TestAbstract(721);
        //obj.TestVirtual("Ocean123");

        //-----------------------------------------------------------------------------------------------------------------

        //CLR绑定测试
        //long curTime = System.DateTime.Now.Ticks;
        //m_AppDomain.Invoke("HotFix.TestCLRBinding", "RunTest", null, null);
        //Debug.Log("使用时间：" + (System.DateTime.Now.Ticks - curTime));
        //5136253

        //-----------------------------------------------------------------------------------------------------------------

        //协程测试
        //m_AppDomain.Invoke("HotFix.TestCor", "RunTest", null, null);


        //-----------------------------------------------------------------------------------------------------------------

        //Mono测试
        //m_AppDomain.Invoke("HotFix.TestMono", "RunTest", null, GameStart.Instance.gameObject);
        //m_AppDomain.Invoke("HotFix.TestMono", "RunTest1", null, GameStart.Instance.gameObject);

    }

    //unsafe void SetUpCLRGetCompontent()
    //{
    //    var arr = typeof(GameObject).GetMethods();
    //    foreach (var i in arr)
    //    {
    //        if (i.Name == "GetCompontent" && i.GetGenericArguments().Length == 1)
    //        {
    //            m_AppDomain.RegisterCLRMethodRedirection(i, GetCompontent);
    //        }
    //    }
    //}

    //private unsafe StackObject* GetCompontent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
    //{
    //    ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;

    //    var ptr = __esp - 1;
    //    GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
    //    if (instance == null)
    //        throw new System.NullReferenceException();

    //    __intp.Free(ptr);

    //    var genericArgument = __method.GenericArguments;
    //    if (genericArgument != null && genericArgument.Length == 1)
    //    {
    //        var type = genericArgument[0];
    //        object res = null;
    //        if (type is CLRType)
    //        {
    //            res = instance.GetComponent(type.TypeForCLR);
    //        }
    //        else
    //        {
    //            var clrInstances = instance.GetComponents<MonoBehaviourAdapter.Adaptor>();
    //            foreach (var clrInstance in clrInstances)
    //            {
    //                if (clrInstance.ILInstance != null)
    //                {
    //                    if (clrInstance.ILInstance.Type == type)
    //                    {
    //                        res = clrInstance.ILInstance;
    //                        break;
    //                    }
    //                }
    //            }
    //        }

    //        return ILIntepreter.PushObject(ptr, __mStack, res);
    //    }

    //    return __esp;
    //}

    //unsafe void SetupCLRAddCompontent()
    //{
    //    var arr = typeof(GameObject).GetMethods();
    //    foreach (var i in arr)
    //    {
    //        if (i.Name == "AddComponent" && i.GetGenericArguments().Length == 1)
    //        {
    //            m_AppDomain.RegisterCLRMethodRedirection(i, AddCompontent);
    //        }
    //    }
    //}

    //private unsafe StackObject* AddCompontent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
    //{
    //    ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;

    //    var ptr = __esp - 1;
    //    GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
    //    if (instance == null)
    //    {
    //        throw new System.NullReferenceException();
    //    }
    //    __intp.Free(ptr);

    //    var genericArgument = __method.GenericArguments;
    //    if (genericArgument != null && genericArgument.Length == 1)
    //    {
    //        var type = genericArgument[0];
    //        object res;
    //        if (type is CLRType)//CLRType表示这个类型是Unity工程里的类型   //ILType表示是热更dll里面的类型
    //        {
    //            //Unity主工程的类，不需要做处理
    //            res = instance.AddComponent(type.TypeForCLR);
    //        }
    //        else
    //        {
    //            //创建出来MonoTest
    //            var ilInstance = new ILTypeInstance(type as ILType, false);
    //            var clrInstance = instance.AddComponent<MonoBehaviourAdapter.Adaptor>();
    //            clrInstance.ILInstance = ilInstance;
    //            clrInstance.AppDomain = __domain;
    //            //这个实例默认创建的CLRInstance不是通过AddCompontent出来的有效实例，所以要替换
    //            ilInstance.CLRInstance = clrInstance;

    //            res = clrInstance.ILInstance;

    //            //补掉Awake
    //            clrInstance.Awake();
    //        }
    //        return ILIntepreter.PushObject(ptr, __mStack, res);
    //    }

    //    return __esp;
    //}
}