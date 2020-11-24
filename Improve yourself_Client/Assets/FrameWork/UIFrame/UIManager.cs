/****************************************************
	文件：UIManager.cs
	作者：NingWei
	日期：2020/09/07 11:34   	
	功能：UI管理器
*****************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;

public enum UIMsgID
{
    None = 0,

}

/// <summary>
/// UI窗体（位置）类型
/// </summary>
public enum UIRoot
{
    //普通窗体
    Normal,
    //弹出窗体
    PopUp
}

/// <summary>
/// UI的显示类型
/// </summary>
public enum UIShowMode
{
    //普通
    Normal,
    //反向切换
    ReverseChange,
    //隐藏其他
    HideOther
}

public class UIManager : Singleton<UIManager>
{
    //UI节点
    private RectTransform m_UIRoot;

    //普通UI节点
    private RectTransform m_NormalRoot;

    //弹出式窗口节点
    private RectTransform m_PopUPRoot;

    /// <summary>
    /// 打开UI的时候开启遮罩，打开后关闭遮罩
    /// </summary>
    private GameObject m_UIMask;

    //UI摄像机
    private Camera m_UICamera;

    //EventSystem 节点
    private EventSystem m_EventSystem;

    //屏幕的宽高比
    private float m_CanvasRate = 0;

    private string m_UIPrefabPath = "Assets/GameData/Prefabs/UGUI/Panel/";

    //注册的字典
    private Dictionary<string, System.Type> m_RegisterDic = new Dictionary<string, Type>();

    //缓存所有的UI
    private Dictionary<string, BaseUI> m_AllUIDic = new Dictionary<string, BaseUI>();

    //当前正在显示的UIDic
    private Dictionary<string, BaseUI> m_CurrentShowUIDic = new Dictionary<string, BaseUI>();

    //当前正在显示的UIList
    private List<BaseUI> m_CurrentShowUIList = new List<BaseUI>();

    //定义“栈”集合,存储显示当前所有[反向切换]的窗体类型
    private Stack<BaseUI> m_StackUI = new Stack<BaseUI>();


    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="uiRoot">UI父节点</param>
    /// <param name="wndRoot">窗口父节点</param>
    /// <param name="uiCamera">UI摄像机</param>
    public void Init(Transform transform)
    {
        m_UIRoot = transform.Find("UIRoot") as RectTransform;
        m_NormalRoot = transform.Find("UIRoot/Normal") as RectTransform;
        m_PopUPRoot = transform.Find("UIRoot/m_PopUP") as RectTransform;
        m_UIMask = transform.Find("UIRoot/Mask").gameObject;
        m_UICamera = transform.Find("UIRoot/UICamera").GetComponent<Camera>();
        m_EventSystem = transform.Find("UIRoot/EventSystem").GetComponent<EventSystem>();
        m_CanvasRate = Screen.height / (m_UICamera.orthographicSize * 2);
    }

    /// <summary>
    /// 设置所有节目UI路径
    /// </summary>
    /// <param name="path"></param>
    public void SetUIPrefabPath(string path)
    {
        m_UIPrefabPath = path;
    }

    /// <summary>
    /// 显示或者隐藏所有的UI
    /// </summary>
    public void ShowOrHideUI(bool show)
    {
        if (m_UIRoot != null)
        {
            m_UIRoot.gameObject.SetActive(show);
        }
    }

    /// <summary>
    /// 设置默认的选择对象
    /// </summary>
    public void SetNormalSelectObj(GameObject obj)
    {
        if (m_EventSystem == null)
        {
            m_EventSystem = EventSystem.current;
        }

        m_EventSystem.firstSelectedGameObject = obj;
    }


    public void OnUpdate()
    {
        for (int i = 0; i < m_CurrentShowUIList.Count; i++)
        {
            if (m_CurrentShowUIList[i] != null)
            {
                m_CurrentShowUIList[i].OnUpdate();
            }
        }
    }

    /// <summary>
    /// UI注册方法
    /// </summary>
    /// <typeparam name="T">窗口泛型类</typeparam>
    /// <param name="name">窗口名称</param>
    public void Register<T>(string name) where T : BaseUI
    {
        m_RegisterDic[name] = typeof(T);
    }

    /// <summary>
    /// 发送消息给一个UI
    /// </summary>
    /// <param name="name"></param>
    /// <param name="msgId"></param>
    /// <param name="paralist"></param>
    /// <returns></returns>
    public bool SendMessageToUI(string name, UIMsgID msgId = UIMsgID.None, params object[] paralist)
    {
        BaseUI wnd = FindUIByName<BaseUI>(name);
        if (wnd != null)
        {
            return wnd.OnMessage(msgId, paralist);
        }

        return false;
    }

    /// <summary>
    /// 根据窗口名称找到一个窗口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="uiName"></param>
    /// <returns></returns>
    public T FindUIByName<T>(string uiName) where T : BaseUI
    {
        BaseUI ui = null;
        if (m_AllUIDic.TryGetValue(uiName, out ui))
        {
            return (T)ui;
        }

        return null;
    }

    /// <summary>
    /// 打开UI
    /// </summary>
    /// <param name="name">注册的时候给的名称</param>
    /// <param name="bTop"></param>
    /// <param name="resource"></param>
    /// <param name="para1"></param>
    /// <param name="para2"></param>
    /// <param name="para3"></param>
    /// <returns></returns>
    public BaseUI ShowUI(string name, bool bTop = true, AssetAddress resource = AssetAddress.Addressable, params object[] paramList)
    {
        //开启遮罩避免开启UI的时候接收了操作出现异常
        SetMask(true);

        BaseUI ui = FindUIByName<BaseUI>(name);
        if (ui == null)
        {
            System.Type tp = null;
            if (m_RegisterDic.TryGetValue(name, out tp))
            {
                //if (resource)
                //{
                ui = System.Activator.CreateInstance(tp) as BaseUI;
                ui.Init();
                //}
                //else
                //{
                //string hotName = "HotFix." + name.Replace("Panel.prefab", "Ui");
                //wnd = ILRuntimeManager.Instance.ILRunAppDomain.Instantiate<Window>(hotName);
                //wnd.IsHotFix = true;
                //wnd.HotFixClassName = hotName;
                //}
            }
            else
            {
                Debug.LogError("找不到窗口对应的脚本，注册的窗口名称是：" + name);
                return null;
            }
            GameObject uiObj = null;
            if (resource == AssetAddress.Resources)
            {
                //从resource加载UI
                uiObj = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(ui.PrefabName.Replace(".prefab", ""))) as GameObject;
                InitPrefab(ui, uiObj, name, resource, bTop, paramList);
            }
            else if (resource == AssetAddress.AssetBundle)
            {
                //从AssetBundle加载UI
                ObjectManager.Instance.InstantiateObjectAsync(m_UIPrefabPath + ui.PrefabName, (string path, UnityEngine.Object obj, object[] paramArr) => {
                    uiObj = obj as GameObject;
                    InitPrefab(ui, uiObj, name, resource, bTop, paramList);
                }, LoadResPriority.RES_HIGHT, false, false);
            }
            else if (resource == AssetAddress.Addressable)
            {
                //从Addressables加载UI
                Addressables.InstantiateAsync(m_UIPrefabPath + ui.PrefabName).Completed += op =>
                {
                    uiObj = op.Result;
                    InitPrefab(ui, uiObj, name, resource, bTop, paramList);
                };
            }
        }
        else
        {
            //设置UI的显示模式
            SetUIShowMode(ui);

            OnShowUI(ui, bTop, paramList);
        }

        return ui;
    }

    /// <summary>
    /// 初始化UIPrefab
    /// </summary>
    /// <param name="ui"></param>
    /// <param name="uiObj"></param>
    /// <param name="name"></param>
    /// <param name="resource"></param>
    /// <param name="bTop"></param>
    /// <param name="paramList"></param>
    void InitPrefab(BaseUI ui, GameObject uiObj, string name, AssetAddress resource = AssetAddress.AssetBundle, bool bTop = true, params object[] paramList)
    {
        if (uiObj == null)
        {
            Debug.Log("创建窗口Prefagb失败：" + name);
            return;
        }

        //添加到所有UI字典中去
        if (!m_AllUIDic.ContainsKey(name))
        {
            m_AllUIDic.Add(name, ui);           
        }

        ui.GameObject = uiObj;
        ui.Transform = uiObj.transform;
        ui.Name = name;
        ui.Resource = resource;

        //设置UI的挂在节点
        SetUIRoot(ui);

        //设置UI的显示模式
        SetUIShowMode(ui);

        uiObj.transform.SetAsLastSibling();

        ui.Awake(paramList);
        ui.OnShow(paramList);
        //UI的开启完毕后，关闭遮罩
        SetMask(false);
    }

    /// <summary>
    /// 设置UI的挂在节点
    /// </summary>
    /// <param name="ui"></param>
    void SetUIRoot(BaseUI ui) {
        switch (ui.m_UIRoot)
        {
            case UIRoot.Normal:
                ui.Transform.SetParent(m_NormalRoot, false);
                break;
            case UIRoot.PopUp:
                ui.Transform.SetParent(m_PopUPRoot, false);
                break;
        }
    }

    /// <summary>
    /// 设置UI的显示模式
    /// </summary>
    /// <param name="ui"></param>
    void SetUIShowMode(BaseUI ui)
    {
        //根据不同的UI窗体的显示模式，分别作不同的加载处理
        switch (ui.m_ShowMode)
        {
            case UIShowMode.Normal:                 //“普通显示”窗口模式
                LoadUIToCurrentCache(ui);
                break;
            case UIShowMode.ReverseChange:          //需要“反向切换”窗口模式
                PushUIToStack(ui);
                break;
            case UIShowMode.HideOther:              //“隐藏其他”窗口模式
                EnterUIAndHideOther(ui);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 将UI添加到正在显示UI字典
    /// </summary>
    /// <param name="ui"></param>
    private void LoadUIToCurrentCache(BaseUI ui)
    {
        //容错处理，“正在显示”的集合中，存在这个UI窗体，则直接返回不处理了
        if (m_CurrentShowUIDic.ContainsKey(ui.Name))
            return;
        m_CurrentShowUIDic.Add(ui.Name, ui);
        m_CurrentShowUIList.Add(ui);
    }

    /// <summary>
    /// 将UI添加到栈里
    /// </summary>
    /// <param name="ui"></param>
    private void PushUIToStack(BaseUI ui)
    {
        //判断“栈”集合中，是否有其他的窗体，有则关闭栈顶
        if (m_StackUI.Count > 0)
        {
            BaseUI topUIForm = m_StackUI.Peek();
            //栈顶元素作冻结处理
            HideUI(topUIForm);
        }
        //把指定的UI窗体，入栈操作。
        m_StackUI.Push(ui);
    }

    private void EnterUIAndHideOther(BaseUI ui)
    {
        //容错处理，“正在显示”的集合中，存在这个UI窗体，则直接返回不处理了
        if (m_CurrentShowUIDic.ContainsKey(ui.Name))
            return;

        //把“正在显示集合”所有窗体都隐藏。
        foreach (BaseUI baseUI in m_CurrentShowUIDic.Values)
        {
            HideUI(baseUI);
        }

        //把栈集合的栈顶元素隐藏
        if (m_StackUI.Count > 0)
        {
            BaseUI topUIForm = m_StackUI.Peek();
            //栈顶元素作冻结处理
            HideUI(topUIForm);
        }

        //把当前窗体加入到“正在显示窗体”集合中，且做显示处理。
        m_CurrentShowUIDic.Add(ui.Name, ui);
        m_CurrentShowUIList.Add(ui);
    }

    void SetMask(bool show) {
        if(show != m_UIMask.activeSelf)
            m_UIMask.SetActive(show);
    }

    /// <summary>
    /// 是否存在Window
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool ExisWindow(string name) {
        if (m_AllUIDic.ContainsKey(name))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 关闭窗口，根据名称
    /// </summary>
    public void CloseUI(string name,bool destory = false)
    {
        BaseUI ui = FindUIByName<BaseUI>(name);
        CloseUI(ui, destory);
    }

    /// <summary>
    /// 关闭窗口,根据窗口对象
    /// </summary>
    /// <param name="ui"></param>
    /// <param name="destory"></param>
    public void CloseUI(BaseUI ui, bool destory = false)
    {
        if (ui != null)
        {
            ui.OnDisable();
            if (m_AllUIDic.ContainsKey(ui.Name))
            {
                m_CurrentShowUIList.Remove(ui);
                m_AllUIDic.Remove(ui.Name);
            }

            if (destory) {
                ///从AssetBundel加载的
                if (ui.Resource == AssetAddress.AssetBundle)
                {
                    ObjectManager.Instance.ReleaseObject(ui.GameObject, 0, true);
                }
                else if (ui.Resource == AssetAddress.Addressable)
                {
                    Addressables.Release(ui.GameObject);
                }
                else if (ui.Resource == AssetAddress.Resources)
                {
                    //从Resource加载的
                    GameObject.Destroy(ui.GameObject);
                }
                ui.GameObject = null;
                ui = null;
            }
            else {
                if (ui.Resource == AssetAddress.AssetBundle)
                {
                    ObjectManager.Instance.ReleaseObject(ui.GameObject, recycleParent: false);
                }
                HideUI(ui);
            }
        }
    }

    /// <summary>
    /// 关闭所有的窗口
    /// </summary>
    public void CloseAllUI()
    {
        for (int i = m_CurrentShowUIList.Count -1; i >= 0; i--)
        {
            CloseUI(m_CurrentShowUIList[i]);
        }
    }

    /// <summary>
    /// 切换到唯一窗口
    /// </summary>
    public void SwitchStateByName(string name, bool bTop = true, AssetAddress resource = AssetAddress.AssetBundle,params object [] paralist)
    {
        CloseAllUI();
        ShowUI(name, bTop, resource, paralist);
    }

    /// <summary>
    /// 关闭窗口，根据名称
    /// </summary>
    /// <param name="name"></param>
    public void HideUI(string name)
    {
        BaseUI ui = FindUIByName<BaseUI>(name);
        HideUI(ui);
    }

    /// <summary>
    /// 关闭窗口，根据窗口对象
    /// </summary>
    /// <param name="ui"></param>
    public void HideUI(BaseUI ui)
    {
        if (ui != null)
        {
            ui.GameObject.SetActive(false);
            ui.OnDisable();
        }
    }

    /// <summary>
    /// 根据窗口名称显示窗口
    /// </summary>
    /// <param name="name"></param>
    /// <param name="paramList"></param>
    public void OnShowUI(string name,bool bTop = true, params object[] paramList)
    {
        BaseUI ui = FindUIByName<BaseUI>(name);
        OnShowUI(ui, bTop, paramList);
    }

    /// <summary>
    /// 根据窗口对象，显示窗口
    /// </summary>
    /// <param name="wnd"></param>
    /// <param name="paramList"></param>
    public void OnShowUI(BaseUI wnd, bool bTop = true, params object[] paramList)
    {
        if(wnd != null){
            if(wnd.GameObject!= null && !wnd.GameObject.activeSelf){
                if(bTop)
                    wnd.Transform.SetAsLastSibling();
                wnd.OnShow(paramList);
                //UI的开启完毕后，关闭遮罩
                SetMask(false);
            }
        }
    }
}
