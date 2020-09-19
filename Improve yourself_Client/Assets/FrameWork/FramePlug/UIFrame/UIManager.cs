/****************************************************
	文件：UIManager.cs
	作者：NingWei
	日期：2020/09/07 11:34   	
	功能：UI管理器
*****************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum UIMsgID
{
    None = 0,

}

public class UIManager : Singleton<UIManager>
{
    //UI节点
    private RectTransform m_UIRoot;

    //窗口节点
    private RectTransform m_WindowRoot;

    //UI摄像机
    private Camera m_UICamera;

    //EventSystem 节点
    private EventSystem m_EventSystem;

    //屏幕的宽高比
    private float m_CanvasRate = 0;

    private string m_UIPrefabPath = "Assets/GameData/Prefabs/UI/Panel/";

    //所有打开的窗口
    private Dictionary<string,Window> m_WindowDic = new Dictionary<string, Window>();

    //所有打开的窗口
    private List<Window> m_WindowList = new List<Window>();

    //注册的字典
    private Dictionary<string ,System.Type> m_RegisterDic = new Dictionary<string, Type>();

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="uiRoot">UI父节点</param>
    /// <param name="wndRoot">窗口父节点</param>
    /// <param name="uiCamera">UI摄像机</param>
    public void Init(Transform transform)
    {
        m_UIRoot = transform.Find("UIRoot") as RectTransform;
        m_WindowRoot = transform.Find("UIRoot/WindRoot") as RectTransform;
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
        for (int i = 0; i < m_WindowList.Count; i++)
        {
            if (m_WindowList[i] != null)
            {
                m_WindowList[i].OnUpdate();
            }
        }
    }

    /// <summary>
    /// 窗口注册方法
    /// </summary>
    /// <typeparam name="T">窗口泛型类</typeparam>
    /// <param name="name">窗口名称</param>
    public void Register<T>(string name) where T : Window
    {
        m_RegisterDic[name] = typeof(T);
    }

    /// <summary>
    /// 发送消息给一个Window
    /// </summary>
    /// <param name="name"></param>
    /// <param name="msgId"></param>
    /// <param name="paralist"></param>
    /// <returns></returns>
    public bool SendMessageToWnd(string name,UIMsgID msgId = UIMsgID.None,params object [] paralist)
    {
        Window wnd = FindWindowByName<Window>(name);
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
    /// <param name="windowName"></param>
    /// <returns></returns>
    public T FindWindowByName<T>(string windowName) where T:Window
    {
        Window wnd = null;
        if (m_WindowDic.TryGetValue(windowName, out wnd))
        {
            return (T) wnd;
        }

        return null;
    }

    /// <summary>
    /// 打开窗口
    /// </summary>
    /// <param name="name"></param>
    /// <param name="bTop"></param>
    /// <param name="para1"></param>
    /// <param name="para2"></param>
    /// <param name="para3"></param>
    /// <returns></returns>
    public Window PopUpWindow(string name,bool bTop = true,params object [] paramList)
    {
        Window wnd = FindWindowByName<Window>(name);
        if (wnd == null)
        {
            System.Type tp = null;
            if (m_RegisterDic.TryGetValue(name, out tp))
            {
                wnd = System.Activator.CreateInstance(tp) as Window;
            }
            else
            {
                Debug.LogError("找不到窗口对应的脚本，窗口名称是："+name);
                return null;
            }

            //GameObject wndObj = ObjectManager.Instance.InstantiateObject(UIPREFABPAHT + name,false,false);
            GameObject wndObj = ObjectManager.Instance.InstantiateObject(wnd.PrefabPath(), false, false);
            if (wndObj == null)
            {
                Debug.Log("创建窗口Prefagb失败："+name);
                return null;
            }

            if (!m_WindowDic.ContainsKey(name))
            {
                m_WindowList.Add(wnd);
                m_WindowDic.Add(name, wnd);
            }

            wnd.GameObject = wndObj;
            wnd.Transform = wndObj.transform;
            wnd.Name = name;
            wnd.Awake(paramList);
            wndObj.transform.SetParent(m_WindowRoot,false);

            //置顶的UI
            if (bTop)
            {
                wndObj.transform.SetAsLastSibling();
            }

            wnd.OnShow(paramList);
        }
        else
        {
            ShowWindow(wnd, bTop,paramList);
        }

        return wnd;
    }

    /// <summary>
    /// 关闭窗口，根据名称
    /// </summary>
    public void CloseWindow(string name,bool destory = false)
    {
        Window wnd = FindWindowByName<Window>(name);
        CloseWindow(wnd, destory);
    }

    /// <summary>
    /// 关闭窗口,根据窗口对象
    /// </summary>
    /// <param name="wnd"></param>
    /// <param name="destory"></param>
    public void CloseWindow(Window wnd, bool destory = false)
    {
        if (wnd != null)
        {
            wnd.OnDisable();
            wnd.OnClose();
            if (m_WindowDic.ContainsKey(wnd.Name))
            {
                m_WindowList.Remove(wnd);
                m_WindowDic.Remove(wnd.Name);
            }

            if (destory)
            {
                ObjectManager.Instance.ReleaseObject(wnd.GameObject,0,true);
            }
            else
            {
                ObjectManager.Instance.ReleaseObject(wnd.GameObject,recycleParent:false);
            }

            wnd.GameObject = null;
            wnd = null;
        }
    }

    /// <summary>
    /// 关闭所有的窗口
    /// </summary>
    public void CloseAllWindow()
    {
        for (int i = m_WindowList.Count -1; i >= 0; i--)
        {
            CloseWindow(m_WindowList[i]);
        }
    }

    /// <summary>
    /// 切换到唯一窗口
    /// </summary>
    public void SwitchStateByName(string name,bool bTop = true,params object [] paralist)
    {
        CloseAllWindow();
        PopUpWindow(name, bTop, paralist);
    }

    /// <summary>
    /// 关闭窗口，根据名称
    /// </summary>
    /// <param name="name"></param>
    public void HideWindow(string name)
    {
        Window wnd = FindWindowByName<Window>(name);
        HideWindow(wnd);
    }

    /// <summary>
    /// 关闭窗口，根据窗口对象
    /// </summary>
    /// <param name="wnd"></param>
    public void HideWindow(Window wnd)
    {
        if (wnd != null)
        {
            wnd.GameObject.SetActive(false);
            wnd.OnDisable();
        }
    }

    /// <summary>
    /// 根据窗口名称显示窗口
    /// </summary>
    /// <param name="name"></param>
    /// <param name="paramList"></param>
    public void ShowWindow(string name,bool bTop = true, params object[] paramList)
    {
        Window wnd = FindWindowByName<Window>(name);
        ShowWindow(wnd, bTop, paramList);
    }

    /// <summary>
    /// 根据窗口对象，显示窗口
    /// </summary>
    /// <param name="wnd"></param>
    /// <param name="paramList"></param>
    public void ShowWindow(Window wnd, bool bTop = true, params object[] paramList)
    {
        if(wnd != null){
            if(wnd.GameObject!= null && !wnd.GameObject.activeSelf){
                if(bTop)
                    wnd.Transform.SetAsLastSibling();
                wnd.OnShow(paramList);
            }
        }
    }
}
