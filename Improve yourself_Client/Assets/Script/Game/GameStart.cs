/****************************************************
	文件：GameStart.cs
	作者：NingWei
	日期：2020/09/07 11:28   	
	功能：游戏启动类
*****************************************************/
using UnityEngine;

public class GameStart : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this);
        
        //初始化网络通信管理器
        NetWorkManager.Instance.Init();

        //初始化热更管理器
        HotPatchManager.Instance.Init(this);

        //加载AssetBundle配置文件
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        
        //初始化资源管理器
        ResourceManager.Instance.Init(this);

        //初始化对象管理器
        ObjectManager.Instance.Init(transform.Find("ResourcePoolTrs"), transform.Find("SceneTrs"));

        //初始化场景管理器
        GameMapManager.Instance.Init(this);

        //初始化UI管理器
        UIManager.Instance.Init(transform);

        //加载配置文件
        LoadConfig();

        //注册所有的UI
        RegisterAllUI();
    }

    /// <summary>
    /// 注册所有的UI
    /// </summary>
    void RegisterAllUI()
    {
        UIManager.Instance.Register<MenuWindow>(ConStr.MenuPanel);
        UIManager.Instance.Register<LoadingWindow>(ConStr.LoadingPanel);
        UIManager.Instance.Register<HotFixWindow>(ConStr.HotFixPanel);
    }

    void LoadConfig() {

    }

    // Start is called before the first frame update
    void Start()
    {
        //实例化对象预加载
        //ObjectManager.Instance.PreLoadGameObject(ConStr.Role, 5);

        ////非实例化资源预加载
        //ResourceManager.Instance.PreloadRes(ConStr.MenuSound);

        //加载Menu场景
        GameMapManager.Instance.LoadScene(ConStr.MenuScene);
    }

    // Update is called once per frame
    void Update()
    {
        UIManager.Instance.OnUpdate();

        NetWorkManager.Instance.Update();
        if (Input.GetKeyDown(KeyCode.Space)) {
            NetWorkManager.Instance.m_Client.session.SendMsg(new Protocal.NetMsg
            {

            });
        }
    }

    private void OnApplicationQuit()
    {
#if UNITY_EDITOR
        ResourceManager.Instance.ClearCache();
        Resources.UnloadUnusedAssets();
#endif
    }
}
