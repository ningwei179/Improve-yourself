/****************************************************
	文件：GameStart.cs
	作者：NingWei
	日期：2020/09/07 11:28   	
	功能：游戏启动类
*****************************************************/
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameStart : MonoSingleton<GameStart>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
        //读取打包时的版本信息
        ReadVersion();

        ////初始化网络通信管理器
        NetWorkManager.Instance.Init();

        //初始化AB配置表
        AssetBundleManager.Instance.LoadAssetBundleConfig();

        //初始化资源管理器
        ResourceManager.Instance.Init(this);

        //初始化对象管理器
        ObjectManager.Instance.Init(transform.Find("ResourcePoolTrs"), transform.Find("SceneTrs"));

        //初始化热更管理器
        HotPatchManager.Instance.Init(this);

        //初始化UI管理器
        UIManager.Instance.Init(transform);

        //注册所有的UI
        UIRegister.Instance.RegisterAllUI();
    }

    // Start is called before the first frame update
    void Start()
    {
        //启动热更UI
        UIManager.Instance.PopUpWindow(ConStr.HotFixPanel,true,UISource.Resources);
    }

    public IEnumerator StartGame(Image image, Text text)
    {
        image.fillAmount = 0;
        yield return null;

        text.text = "加载本地数据... ...";

        //热更完成后检查下AB配置表，这个文件可能被热更了
        AssetBundleManager.Instance.LoadAssetBundleConfig(false);

        image.fillAmount = 0.1f;
        yield return null;
        text.text = "加载dll... ...";
        //初始化ILRuntime热更管理器
        ILRuntimeManager.Instance.Init();
        image.fillAmount = 0.2f;
        yield return null;
        text.text = "加载数据表... ...";
        //加载配置文件
        LoadConfig();
        image.fillAmount = 0.7f;
        yield return null;
        text.text = "加载配置... ...";
        image.fillAmount = 0.9f;
        yield return null;
        text.text = "初始化地图... ...";
        //初始化场景管理器
        GameMapManager.Instance.Init(this);
        image.fillAmount = 1f;
        yield return null;
    }

    void LoadConfig()
    {
        
    }

    /// <summary>
    /// 读取打包时的版本
    /// </summary>
    void ReadVersion()
    {
        TextAsset versionTex = Resources.Load<TextAsset>("Version");
        if (versionTex == null)
        {
            Debug.LogError("未读到本地版本！");
            return;
        }
        string[] all = versionTex.text.Split('\r');
        if (all.Length > 0)
        {
            string[] infoList = all[0].Split(';');
            if (infoList.Length >= 2)
            {
                HotPatchManager.Instance.CurVersion = infoList[0].Split('|')[1];
                HotPatchManager.Instance.m_CurPackName = infoList[1].Split('|')[1];
                AssetBundleManager.Instance.Encrypt = infoList[2].Split('|')[1].Equals("True");
            }
        }
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

    void FixedUpdate()
    {
        TimerController.Instance.fixedUpdate();
    }

    private void OnApplicationQuit()
    {
#if UNITY_EDITOR
        ResourceManager.Instance.ClearCache();
        Resources.UnloadUnusedAssets();
#endif
    }
}
