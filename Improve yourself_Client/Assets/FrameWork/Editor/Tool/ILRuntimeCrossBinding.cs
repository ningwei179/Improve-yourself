using UnityEditor;

namespace Improve
{
    [System.Reflection.Obfuscation(Exclude = true)]
    public class ILRuntimeCrossBinding
    {
        [MenuItem("IYILRuntime/���ɿ���̳�������")]
        static void GenerateCrossbindAdapter()
        {
            //���ڿ���̳�������̫�࣬�Զ������޷�ʵ����ȫ�޸��������ɣ����������ṩ�Ĵ����Զ�������Ҫ�Ǹ�������ɸ���ʼģ�棬�򻯴�ҵĹ���
            //��������ֱ��ʹ���Զ����ɵ�ģ�漴�ɣ����������������ֶ�ȥ�޸����ɺ���ļ������������Ҫ������д����Ƿ񸲸ǵ�����

            //using (System.IO.StreamWriter sw = new System.IO.StreamWriter("Assets/Script/ILRuntime/Adapter/xxxAdapter.cs"))
            //{
            //    sw.WriteLine(ILRuntime.Runtime.Enviorment.CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(typeof(xxx), "Improve"));
            //}
            AssetDatabase.Refresh();
        }
    }
}