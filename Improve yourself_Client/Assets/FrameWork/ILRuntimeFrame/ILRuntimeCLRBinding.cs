using UnityEditor;
using UnityEngine;
namespace Improve
{
    /// <summary>
    /// ILRuntime����������ű������ͻ���Ըĸ�����
    /// </summary>
    public class ILRuntimeCLRBinding
    {
        [MenuItem("IYILRuntime/ͨ���Զ������ȸ�DLL����CLR��")]
        static void GenerateCLRBindingByAnalysis()
        {
            //���µķ����ȸ�dll�������������ɰ󶨴���
            ILRuntime.Runtime.Enviorment.AppDomain domain = new ILRuntime.Runtime.Enviorment.AppDomain();
            using (System.IO.FileStream fs = new System.IO.FileStream("Assets/GameData/Data/ILRuntimeHotFix/HotFixProject.dll.bytes", System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                domain.LoadAssembly(fs);

                //Crossbind Adapter is needed to generate the correct binding code
                InitILRuntime(domain);
                ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(domain, "Assets/Script/ILRuntime/Generated");
            }

            AssetDatabase.Refresh();
        }

        static void InitILRuntime(ILRuntime.Runtime.Enviorment.AppDomain domain)
        {
            //������Ҫע�������ȸ�DLL���õ��Ŀ���̳�Adapter�������޷���ȷץȡ����
            domain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
            domain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
            //domain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());
        }
    }
}