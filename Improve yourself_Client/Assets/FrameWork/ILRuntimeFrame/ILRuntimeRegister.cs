using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using System.Collections.Generic;
using UnityEngine;

namespace Improve
{
    public class ILRuntimeRegister : Singleton<ILRuntimeRegister>
    {
        ILRuntime.Runtime.Enviorment.AppDomain m_AppDomain;

        unsafe public void InitializeILRuntime(ILRuntime.Runtime.Enviorment.AppDomain AppDomain)
        {
            m_AppDomain = AppDomain;
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
            //����Unity��Profiler�ӿ�ֻ���������߳�ʹ�ã�Ϊ�˱�����쳣����Ҫ����ILRuntime���̵߳��߳�ID������ȷ���������к�ʱ�����Profiler
            m_AppDomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
            //m_AppDomain.DebugService.StartDebugService(56000);
#endif
            //������һЩILRuntime��ע��,ע��©�˵�ί�У����е�ʱ��ᱨ����ʾ��

            //ί�У�����������ֵ���Ͱ󶨵ȵȵ�ע��
            m_AppDomain.DelegateManager.RegisterMethodDelegate<GameObject>();

            m_AppDomain.DelegateManager.RegisterMethodDelegate<UnityEngine.Sprite>();

            m_AppDomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction>((act) =>
            {
                return new UnityEngine.Events.UnityAction(() =>
                {
                    ((System.Action)act)();
                });
            });

            m_AppDomain.DelegateManager.RegisterMethodDelegate<System.String, UnityEngine.Object, System.Object[]>();

            m_AppDomain.DelegateManager.RegisterDelegateConvertor<Improve.OnAsyncFinish>((act) =>
            {
                return new Improve.OnAsyncFinish((path, obj, paramList) =>
                {
                    ((System.Action<System.String, UnityEngine.Object, System.Object[]>)act)(path, obj, paramList);
                });
            });



            m_AppDomain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
            m_AppDomain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
            m_AppDomain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());

            //CLR�ض����ע��
            var mi = typeof(Debug).GetMethod("Log", new System.Type[] { typeof(object) });
            m_AppDomain.RegisterCLRMethodRedirection(mi, Log_11);

            //GameObject��get,add Commponent��ע��
            SetupCLRRedirectionAddGetComponent();


            //��ʼ��CLR������ڳ�ʼ�������һ������
            //CLR�󶨽�����ILRuntime��CLR�ض��������ʵ�֣���Ϊʵ����Ҳ�ǽ���CLR�����ķ�������ض��������Լ�����ķ�����������
            //�����ֶ���дCLR�ض��򷽷��Ǹ��������ǳ��޴���£�����Ҫ���ILRuntime�ײ���Ʒǳ��˽⣨�������װ����������ͣ���ô����Ref/Out���õȵȣ���
            //���ILRuntime�ṩ��һ���������ɹ������Զ�����CLR�󶨴��롣
            //��CLR�󶨴�������֮����Ҫ����Щ�󶨴���ע�ᵽAppDomain�в���ʹCLR����Ч��
            //����һ��Ҫ�ǵý�CLR�󶨵�ע��д��CLR�ض����ע����棬��Ϊͬһ������ֻ�ܱ��ض���һ�Σ�ֻ����ע����Ǹ�������Ч��
            //���������˰󶨴����������ĵ�ע��,����Щ�󶨴���ע�ᵽAppDomain��
            //ILRuntime.Runtime.Generated.CLRBindings.Initialize(m_AppDomain);
        }

        //��д�ض��򷽷����ڸսӴ�ILRuntime�����ѿ��ܱȽ����ѣ��Ƚϼ򵥵ķ�ʽ��ͨ��CLR�����ɰ󶨴��룬Ȼ������������ϸģ�����������������Ǵ�UnityEngine_Debug_Binding���渴�����ĵ�
        //���ʹ��CLR���뿴��ؽ̳̺��ĵ�
        unsafe static StackObject* Log_11(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            //ILRuntime�ĵ���Լ��Ϊ�������������ջ�����ִ�������������Ҫ�������Ӷ�ջ����ɾ������ѷ���ֵ����ջ���������뿴ILRuntimeʵ��ԭ���ĵ�
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            //�������󷽷����غ�espջָ���ֵ��Ӧ�÷��������������ָ�򷵻�ֵ��������ֻ��Ҫ���������������ֵ����
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            //ȡLog�����Ĳ�������������������Ļ�����һ��������esp - 2,�ڶ���������esp -1, ��ΪMono��bug��ֱ��-2ֵ���������Ҫ����ILIntepreter.Minus
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);

            //�����ǽ�ջָ���ϵ�ֵת����object������ǻ������Ϳ�ֱ��ͨ��ptr->Value��ptr->ValueLow���ʵ�ֵ�������뿴ILRuntimeʵ��ԭ���ĵ�
            object message = typeof(object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            //���зǻ������Ͷ��õ���Free���ͷ��йܶ�ջ
            __intp.Free(ptr_of_this_method);

            //����ʵ����Debug.Logǰ�������Ȼ�ȡDLL�ڵĶ�ջ
            var stacktrace = __domain.DebugService.GetStackTrace(__intp);

            //�����������Ϣ�������DLL��ջ
            UnityEngine.Debug.Log(message + "\n" + stacktrace);

            return __ret;
        }


        unsafe void SetupCLRRedirectionAddGetComponent()
        {
            //�������ͨ��Ӧ��д��InitializeILRuntime������Ϊ����ʾд����
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

        unsafe static StackObject* AddComponent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            //CLR�ض����˵���뿴����ĵ��ͽ̳̣����ﲻ��������
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;

            var ptr = __esp - 1;
            //��Ա�����ĵ�һ������Ϊthis
            GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
            if (instance == null)
                throw new System.NullReferenceException();
            __intp.Free(ptr);

            var genericArgument = __method.GenericArguments;
            //AddComponentӦ������ֻ��1�����Ͳ���
            if (genericArgument != null && genericArgument.Length == 1)
            {
                var type = genericArgument[0];
                object res;
                if (type is CLRType)
                {
                    //Unity�����̵��಻��Ҫ�κ����⴦��ֱ�ӵ���Unity�ӿ�
                    res = instance.AddComponent(type.TypeForCLR);
                }
                else
                {
                    //�ȸ�DLL�ڵ����ͱȽ��鷳���������ǵ��Լ��ֶ�����ʵ��
                    var ilInstance = new ILTypeInstance(type as ILType, false);//�ֶ�����ʵ������ΪĬ�Ϸ�ʽ��new MonoBehaviour������Unity�ﲻ����
                                                                               //����������Adapterʵ��
                    var clrInstance = instance.AddComponent<MonoBehaviourAdapter.Adaptor>();
                    //unity������ʵ����û���ȸ�DLL�����ʵ����������Ҫ�ֶ���ֵ
                    clrInstance.ILInstance = ilInstance;
                    clrInstance.AppDomain = __domain;
                    //���ʵ��Ĭ�ϴ�����CLRInstance����ͨ��AddComponent��������Чʵ�������Ե��ֶ��滻
                    ilInstance.CLRInstance = clrInstance;

                    res = clrInstance.ILInstance;//����ILRuntime��ʵ��Ӧ��ΪILInstance

                    clrInstance.Awake();//��ΪUnity�����������ʱ��û׼�����������ﲹ��һ��
                }

                return ILIntepreter.PushObject(ptr, __mStack, res);
            }

            return __esp;
        }

        unsafe static StackObject* GetComponent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            //CLR�ض����˵���뿴����ĵ��ͽ̳̣����ﲻ��������
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;

            var ptr = __esp - 1;
            //��Ա�����ĵ�һ������Ϊthis
            GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
            if (instance == null)
                throw new System.NullReferenceException();
            __intp.Free(ptr);

            var genericArgument = __method.GenericArguments;
            //AddComponentӦ������ֻ��1�����Ͳ���
            if (genericArgument != null && genericArgument.Length == 1)
            {
                var type = genericArgument[0];
                object res = null;
                if (type is CLRType)
                {
                    //Unity�����̵��಻��Ҫ�κ����⴦��ֱ�ӵ���Unity�ӿ�
                    res = instance.GetComponent(type.TypeForCLR);
                }
                else
                {
                    //��Ϊ����DLL�����MonoBehaviourʵ�ʶ������Component����������ֻ��ȫȡ������������
                    var clrInstances = instance.GetComponents<MonoBehaviourAdapter.Adaptor>();
                    for (int i = 0; i < clrInstances.Length; i++)
                    {
                        var clrInstance = clrInstances[i];
                        if (clrInstance.ILInstance != null)//ILInstanceΪnull, ��ʾ����Ч��MonoBehaviour��Ҫ�Թ�
                        {
                            if (clrInstance.ILInstance.Type == type)
                            {
                                res = clrInstance.ILInstance;//����ILRuntime��ʵ��Ӧ��ΪILInstance
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
