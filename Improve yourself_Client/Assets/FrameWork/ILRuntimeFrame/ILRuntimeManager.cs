/****************************************************     文件：ILRuntimeManager.cs 	作者：NingWei     日期：2020/10/13 15:49:48 	功能：ILRuntime热更管理类 *****************************************************/
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Improve
{
    public class ILRuntimeManager : Singleton<ILRuntimeManager>
    {
        private const string DLLPATH = "Assets/GameData/Data/ILRuntimeHotFix/HotFixProject.dll.bytes";
        private const string PDBPATH = "Assets/GameData/Data/ILRuntimeHotFix/HotFixProject.pdb.bytes";
        private AppDomain m_AppDomain;

        public AppDomain ILRunAppDomain
        {
            get { return m_AppDomain; }
        }

        internal IEnumerator LoadHotFixAssembly(GameObject obj, Transform transform)
        {
            MemoryStream dllMs = null;
            AsyncOperationHandle<TextAsset> dllHandle = Addressables.LoadAssetAsync<TextAsset>(DLLPATH);
            yield return dllHandle;
            if (dllHandle.Status == AsyncOperationStatus.Succeeded)
            {
                dllMs = new MemoryStream(dllHandle.Result.bytes);
            }

            MemoryStream pdbMs = null;
            AsyncOperationHandle<TextAsset> pdbHandle = Addressables.LoadAssetAsync<TextAsset>(PDBPATH);
            yield return pdbHandle;
            if (pdbHandle.Status == AsyncOperationStatus.Succeeded)
            {
                pdbMs = new MemoryStream(pdbHandle.Result.bytes);
            }

            if (dllMs != null && pdbMs != null)
            {
                //整个工程只有一个ILRuntime的AppDomain
                m_AppDomain = new AppDomain();

                m_AppDomain.LoadAssembly(dllMs);

                InitializeILRuntime();

                OnHotFixLoaded(obj, transform);

            }
            yield return null;
        }

        void InitializeILRuntime()
        {
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
            //由于Unity的Profiler接口只允许在主线程使用，为了避免出异常，需要告诉ILRuntime主线程的线程ID才能正确将函数运行耗时报告给Profiler
            m_AppDomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
            m_AppDomain.DebugService.StartDebugService(56000);
#endif
            //这里做一些ILRuntime的注册

            //委托，适配器，，值类型绑定等等的注册
            m_AppDomain.DelegateManager.RegisterMethodDelegate<GameObject>();
            m_AppDomain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
            m_AppDomain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
            m_AppDomain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());

            //CLR重定向的注册
            //GameObject的get,add Commponent的注册
            SetupCLRRedirectionAddGetComponent();


            //初始化CLR绑定请放在初始化的最后一步！！
            //CLR绑定借助了ILRuntime的CLR重定向机制来实现，因为实质上也是将对CLR方法的反射调用重定向到我们自己定义的方法里面来。
            //但是手动编写CLR重定向方法是个工作量非常巨大的事，而且要求对ILRuntime底层机制非常了解（比如如何装拆箱基础类型，怎么处理Ref/Out引用等等），
            //因此ILRuntime提供了一个代码生成工具来自动生成CLR绑定代码。
            //在CLR绑定代码生成之后，需要将这些绑定代码注册到AppDomain中才能使CLR绑定生效，
            //但是一定要记得将CLR绑定的注册写在CLR重定向的注册后面，因为同一个方法只能被重定向一次，只有先注册的那个才能生效。
            //请在生成了绑定代码后解除下面的的注释,将这些绑定代码注册到AppDomain中
            ILRuntime.Runtime.Generated.CLRBindings.Initialize(m_AppDomain);
        }

        internal void OpenLoadingUI(string name)
        {
            m_AppDomain.Invoke("HotFixProject.HotFixMain", "OpenLoadingUI", null, name);
        }

        void OnHotFixLoaded(GameObject obj, Transform transform)
        {
            //启动热更DLL的Main方法
            m_AppDomain.Invoke("HotFixProject.HotFixMain", "Main", null, obj, transform);
        }


        unsafe void SetupCLRRedirectionAddGetComponent()
        {
            //这里面的通常应该写在InitializeILRuntime，这里为了演示写这里
            var arr = typeof(GameObject).GetMethods();
            foreach (var i in arr)
            {
                if (i.Name == "AddComponent" && i.GetGenericArguments().Length == 1)
                {
                    m_AppDomain.RegisterCLRMethodRedirection(i, AddComponent);
                }
                if (i.Name == "GetComponent" && i.GetGenericArguments().Length == 1)
                {
                    m_AppDomain.RegisterCLRMethodRedirection(i, GetComponent);
                }
            }
        }

        unsafe void SetupCLRRedirection2()
        {
            //这里面的通常应该写在InitializeILRuntime，这里为了演示写这里
            var arr = typeof(GameObject).GetMethods();
            foreach (var i in arr)
            {
                if (i.Name == "GetComponent" && i.GetGenericArguments().Length == 1)
                {
                    m_AppDomain.RegisterCLRMethodRedirection(i, GetComponent);
                }
            }
        }

        unsafe static StackObject* AddComponent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            //CLR重定向的说明请看相关文档和教程，这里不多做解释
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;

            var ptr = __esp - 1;
            //成员方法的第一个参数为this
            GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
            if (instance == null)
                throw new System.NullReferenceException();
            __intp.Free(ptr);

            var genericArgument = __method.GenericArguments;
            //AddComponent应该有且只有1个泛型参数
            if (genericArgument != null && genericArgument.Length == 1)
            {
                var type = genericArgument[0];
                object res;
                if (type is CLRType)
                {
                    //Unity主工程的类不需要任何特殊处理，直接调用Unity接口
                    res = instance.AddComponent(type.TypeForCLR);
                }
                else
                {
                    //热更DLL内的类型比较麻烦。首先我们得自己手动创建实例
                    var ilInstance = new ILTypeInstance(type as ILType, false);//手动创建实例是因为默认方式会new MonoBehaviour，这在Unity里不允许
                                                                               //接下来创建Adapter实例
                    var clrInstance = instance.AddComponent<MonoBehaviourAdapter.Adaptor>();
                    //unity创建的实例并没有热更DLL里面的实例，所以需要手动赋值
                    clrInstance.ILInstance = ilInstance;
                    clrInstance.AppDomain = __domain;
                    //这个实例默认创建的CLRInstance不是通过AddComponent出来的有效实例，所以得手动替换
                    ilInstance.CLRInstance = clrInstance;

                    res = clrInstance.ILInstance;//交给ILRuntime的实例应该为ILInstance

                    clrInstance.Awake();//因为Unity调用这个方法时还没准备好所以这里补调一次
                }

                return ILIntepreter.PushObject(ptr, __mStack, res);
            }

            return __esp;
        }

        unsafe static StackObject* GetComponent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            //CLR重定向的说明请看相关文档和教程，这里不多做解释
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;

            var ptr = __esp - 1;
            //成员方法的第一个参数为this
            GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
            if (instance == null)
                throw new System.NullReferenceException();
            __intp.Free(ptr);

            var genericArgument = __method.GenericArguments;
            //AddComponent应该有且只有1个泛型参数
            if (genericArgument != null && genericArgument.Length == 1)
            {
                var type = genericArgument[0];
                object res = null;
                if (type is CLRType)
                {
                    //Unity主工程的类不需要任何特殊处理，直接调用Unity接口
                    res = instance.GetComponent(type.TypeForCLR);
                }
                else
                {
                    //因为所有DLL里面的MonoBehaviour实际都是这个Component，所以我们只能全取出来遍历查找
                    var clrInstances = instance.GetComponents<MonoBehaviourAdapter.Adaptor>();
                    for (int i = 0; i < clrInstances.Length; i++)
                    {
                        var clrInstance = clrInstances[i];
                        if (clrInstance.ILInstance != null)//ILInstance为null, 表示是无效的MonoBehaviour，要略过
                        {
                            if (clrInstance.ILInstance.Type == type)
                            {
                                res = clrInstance.ILInstance;//交给ILRuntime的实例应该为ILInstance
                                break;
                            }
                        }
                    }
                }

                return ILIntepreter.PushObject(ptr, __mStack, res);
            }

            return __esp;
        }
    }
}