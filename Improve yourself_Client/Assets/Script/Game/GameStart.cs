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

        if (FrameConstr.UseAssetAddress == AssetAddress.Addressable)
        {
            //初始化Addressable管理器
            AddressableManager.Instance.Init(this);
            //初始化Addressable资源更新管理器
            AddressableUpdateManager.Instance.Init(this);
        }
        else {
            //初始化AB配置表
            AssetBundleManager.Instance.LoadAssetBundleConfig();
            //初始化资源管理器
            ResourceManager.Instance.Init(this);
            //初始化对象管理器
            ObjectManager.Instance.Init(transform.Find("ResourcePoolTrs"), transform.Find("SceneTrs"));
            //初始化热更管理器
            HotPatchManager.Instance.Init(this);
        }

        

        ////初始化网络通信管理器
        //NetWorkManager.Instance.Init();

        //初始化UI管理器
        UIManager.Instance.Init(transform);

        //注册所有的UI
        UIRegister.Instance.RegisterAllUI();
    }

    // Start is called before the first frame update
    void Start()
    {
        //启动热更UI
        UIManager.Instance.PopUpWindow(ConStr.HotFixPanel,true,AssetAddress.Resources);
    }

    WaitForSeconds wait3f = new WaitForSeconds(0.3f);

    public IEnumerator StartGame(Image image, Text text,Text progress)
    {
        image.fillAmount = 0;
        yield return wait3f;
        text.text = "加载本地数据... ...";
        image.fillAmount = 0.1f;
        progress.text = string.Format("{0}%", (int)(image.fillAmount * 100));

        if (FrameConstr.UseAssetAddress != AssetAddress.Addressable)
        {
            //热更完成后检查下AB配置表，这个文件可能被热更了
            AssetBundleManager.Instance.LoadAssetBundleConfig(false);
        }
        yield return wait3f;
        text.text = "加载dll... ...";
        image.fillAmount = 0.2f;
        progress.text = string.Format("{0}%", (int)(image.fillAmount * 100));
        //初始化ILRuntime热更管理器
        ILRuntimeManager.Instance.Init();
        //热更修复代码
        yield return StartCoroutine(InjectFixManager.Instance.LoadHotFixPatch());

        text.text = "加载数据表... ...";
        image.fillAmount = 0.7f;
        progress.text = string.Format("{0}%", (int)(image.fillAmount * 100));
        //加载配置文件
        LoadConfig();
        yield return wait3f;
        text.text = "加载配置... ...";
        image.fillAmount = 0.9f;
        progress.text = string.Format("{0}%", (int)(image.fillAmount * 100));
        yield return wait3f;
        text.text = "初始化地图... ...";
        image.fillAmount = 1f;
        progress.text = string.Format("{0}%", (int)(image.fillAmount * 100));
        //初始化场景管理器
        GameMapManager.Instance.Init(this);
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
                if (FrameConstr.UseAssetAddress == AssetAddress.Addressable)
                {
                    AddressableUpdateManager.Instance.CurVersion = infoList[0].Split('|')[1];
                    AddressableUpdateManager.Instance.m_CurPackName = infoList[1].Split('|')[1];
                }
                else {
                    HotPatchManager.Instance.CurVersion = infoList[0].Split('|')[1];
                    HotPatchManager.Instance.m_CurPackName = infoList[1].Split('|')[1];
                    AssetBundleManager.Instance.Encrypt = infoList[2].Split('|')[1].Equals("True");
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        UIManager.Instance.OnUpdate();

        NetWorkManager.Instance.Update();
        if (Input.GetKeyDown(KeyCode.Space)) {
            NetWorkManager.Instance.m_Client.session.SendMsg(new IYProtocal.NetMsg
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
