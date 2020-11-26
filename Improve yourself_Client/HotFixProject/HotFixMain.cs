using Improve;
using UnityEngine;
namespace HotFixProject
{
    class HotFixMainMonoBehaviour : MonoBehaviour
    {
        float time;
        void Awake()
        {
            Debug.Log("注册UI管理器");
            //初始化UI管理器
            UIManager.Instance.Init(transform);

            ////注册所有的UI
            UIRegister.Instance.RegisterAllUI();
        }

        void Start()
        {
            Debug.Log("!! SomeMonoBehaviour.Start");
        }

        void Update()
        {
            if (Time.time - time > 1)
            {
                Debug.Log("!! SomeMonoBehaviour.Update, t=" + Time.time);
                time = Time.time;
            }
            UIManager.Instance.OnUpdate();
        }
    }

    public class HotFixMain
    {
        public static void Main(GameObject obj, Transform transform)
        {
            UnityEngine.Debug.Log("启动热更DLL的Main方法");
            //将脚本添加到物体上
            obj.AddComponent<HotFixMainMonoBehaviour>();
        }

        public static void OpenLoadingUI(string name)
        {
            UnityEngine.Debug.Log("启动热更DLL的Loading界面");
            UIManager.Instance.OpenUI<LoadingWindow>(ConStr.LoadingPanel, paramList: name);
        }
    }
}
