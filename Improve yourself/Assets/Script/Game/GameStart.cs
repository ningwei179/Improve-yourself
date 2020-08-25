using UnityEngine;

public class GameStart : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this);
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

        RegisterAllUI();
    }

    /// <summary>
    /// 注册所有的UI
    /// </summary>
    void RegisterAllUI()
    {
        UIManager.Instance.Register<MenuWindow>(ConStr.MenuPanel);
        UIManager.Instance.Register<LoadingWindow>(ConStr.LoadingPanel);
    }

    // Start is called before the first frame update
    void Start()
    {
        //实例化对象预加载
        ObjectManager.Instance.PreLoadGameObject(ConStr.Role, 5);

        //非实例化资源预加载
        ResourceManager.Instance.PreloadRes(ConStr.MenuSound);

        //加载Menu场景
        GameMapManager.Instance.LoadScene(ConStr.MenuScene);
    }

    // Update is called once per frame
    void Update()
    {
        UIManager.Instance.OnUpdate();
    }

    private void OnApplicationQuit()
    {
#if UNITY_EDITOR
        ResourceManager.Instance.ClearCache();
        Resources.UnloadUnusedAssets();
#endif
    }
}
