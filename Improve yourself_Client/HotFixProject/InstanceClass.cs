using UnityEngine;

namespace HotFixProject
{
    public class InstanceClass:MonoBehaviour
    {
        // static method
        public static void StaticFunTest(string name)
        {
            UnityEngine.Debug.Log("!!! InstanceClass.StaticFunTest()");
            UIManager.Instance.OpenUI<LoadingWindow>(ConStr.LoadingPanel, paramList: name);
        }

        public static void StaticFunTest2(Transform transform)
        {
            UnityEngine.Debug.Log("一个参数方法的调用");
            //初始化UI管理器
            UIManager.Instance.Init(transform);

            ////注册所有的UI
            UIRegister.Instance.RegisterAllUI();
        }

        //public static void StaticFunTest2(int num1,int num2)
        //{
        //    UnityEngine.Debug.Log("2个参数方法的调用:param1=" + num1+"==param2="+num2);
        //}

        private void Update()
        {
            UIManager.Instance.OnUpdate();
        }
    }
}
