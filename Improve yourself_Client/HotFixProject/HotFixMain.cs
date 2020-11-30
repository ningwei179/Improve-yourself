using Improve;
using UnityEngine;
namespace HotFixProject
{
    class HotFixMainMonoBehaviour : MonoBehaviour
    {
        float time;
        void Awake()
        {
            Debug.Log("热更DLL的注册UI管理器");
            //初始化UI管理器
            UIManager.Instance.Init(transform);

            //FairyGUI在创建UI之前要先绑定
            FairyGUIManager.Instance.BindAll();

            //FairyGUI预加载某些包
            FairyGUIManager.Instance.PreAddPackage();

            ////注册所有的UI
            UIRegister.Instance.RegisterAllUI();
        }

        void Start()
        {
            Debug.Log("!! 热更DLL的HotFixMainMonoBehaviour.Start");
        }

        void Update()
        {
            if (Time.time - time > 1)
            {
                Debug.Log("!! 热更DLL的HotFixMainMonoBehaviour.Update, t=" + Time.time);
                time = Time.time;
            }
            UIManager.Instance.OnUpdate();
        }
    }

    public class HotFixMain
    {
        public static void Main(GameObject obj, Transform transform)
        {
            //将脚本添加到物体上
            obj.AddComponent<HotFixMainMonoBehaviour>();
        }

        public static void OpenLoadingUI(string name)
        {
            UIManager.Instance.OpenUI<LoadingWindow>(ConStr.LoadingPanel, paramList: name);
        }
    }
}
