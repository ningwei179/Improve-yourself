/****************************************************
	文件：UIManager.cs
	作者：NingWei
	日期：2020/09/07 11:34   	
	功能：UI管理器
*****************************************************/
using FairyGUI;
using HotFixProject;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Improve
{

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
    /// UI窗体的显示类型
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

    public class UIManager : HotSingleton<UIManager>
    {
        //UI节点
        private RectTransform m_UIRoot;

        //普通UI节点
        private RectTransform m_NormalRoot;

        //弹出式窗口节点
        private RectTransform m_PopUPRoot;

        //UI遮罩，打开UI的时候开启遮罩，打开后关闭遮罩
        private GameObject m_UIMask;

        //UI摄像机
        private Camera m_UICamera;

        //EventSystem 节点
        private EventSystem m_EventSystem;

        //屏幕的宽高比
        private float m_CanvasRate = 0;

        //UI的路径
        private string m_UIPrefabPath = "Assets/GameData/Prefabs/UGUI/Panel/";

        //缓存所有的UI
        private Dictionary<string, BaseUI> m_AllUIDic = new Dictionary<string, BaseUI>();

        //当前正在显示的UIDic
        private Dictionary<string, BaseUI> m_CurrentShowUIDic = new Dictionary<string, BaseUI>();

        private List<BaseUI> m_CurrentShowList = new List<BaseUI>();

        //定义“栈”集合,存储显示当前所有[反向切换]的窗体类型
        private Stack<BaseUI> m_StackUI = new Stack<BaseUI>();

        //注册UI的字典
        private Dictionary<string, System.Type> m_RegisterDic = new Dictionary<string, Type>();

        /// <summary>
        /// UI注册方法
        /// </summary>
        /// <typeparam name="T">窗口泛型类</typeparam>
        /// <param name="name">窗口名称</param>
        public void Register<T>(string name) where T : BaseUI
        {
            if (!m_RegisterDic.ContainsKey(name))
                m_RegisterDic[name] = typeof(T);
        }

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

        public void OnUpdate()
        {
            for (int i = 0; i < m_CurrentShowList.Count; ++i)
            {
                m_CurrentShowList[i].OnUpdate();
            }

            if (m_StackUI.Count > 0)
            {
                m_StackUI.Peek().OnUpdate();
            }
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
        public void OpenUI<T>(string name, Action<bool, T> callback = null, AssetAddress resource = FrameConstr.UseAssetAddress, params object[] paramList) where T : BaseUI
        {
            //开启遮罩避免开启UI的时候接收了操作出现异常
            SetMask(true);
            BaseUI ui = FindUIByName<BaseUI>(name);
            if (ui == null)
            {
                System.Type tp = null;
                if (m_RegisterDic.TryGetValue(name, out tp))
                {
                    ui = System.Activator.CreateInstance(tp) as BaseUI;
                    ui.Init();
                }
                else
                {
                    Debug.LogError("找不到窗口对应的脚本，注册的窗口名称是：" + name);
                }
                //FGUI 用Addressable好困难,先用AssetBundle实现功能先，实现后看怎么优化让Addressable也能和FGUI融合
                if (ui.m_UIType == UIType.FariyGUI)
                {
                    GComponent view;
                    if (!FairyGUIManager.Instance.m_PreFairyGUIList.ContainsKey(ui.PrefabName))
                    {
                        //没有预加载过的Fairy包,我们就加载这个包
                        if (resource == AssetAddress.Resources)
                        {
                            UIPackage.AddPackage(ui.PrefabName);
                        }
                        else if (resource == AssetAddress.AssetBundle)
                        {
                            string abName;
                            if (FairyGUIManager.Instance.m_FairyGUIList.TryGetValue(ui.PrefabName, out abName))
                            {
                                AssetBundle ab = AssetBundleManager.Instance.LoadAssetBundle(abName);
                                UIPackage.AddPackage(ab);
                            }
                        }
                        else if (resource == AssetAddress.Addressable)
                        {

                        }
                    }
                    ui.wnd = new FairyGUI.Window();
                    InitFairyGUIPanel(ui, name, resource, paramList);
                }
                else
                {
                    GameObject uiObj = null;
                    if (resource == AssetAddress.Resources)
                    {
                        //从resource加载UI
                        uiObj = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(ui.PrefabName.Replace(".prefab", ""))) as GameObject;
                        InitPrefab(ui, uiObj, name, resource, paramList);
                    }
                    else if (resource == AssetAddress.AssetBundle)
                    {
                        //从AssetBundle加载UI
                        ObjectManager.Instance.InstantiateObjectAsync(m_UIPrefabPath + ui.PrefabName, (string path, UnityEngine.Object obj, object[] paramArr) =>
                        {
                            uiObj = obj as GameObject;
                            InitPrefab(ui, uiObj, name, resource, paramList);
                        }, LoadResPriority.RES_HIGHT, false, false);
                    }
                    else if (resource == AssetAddress.Addressable)
                    {
                        AddressableManager.Instance.AsyncInstantiate(m_UIPrefabPath + ui.PrefabName, (GameObject obj) =>
                        {
                            uiObj = obj;
                            InitPrefab(ui, uiObj, name, resource, paramList);
                        });
                    }
                }
            }
            else
            {
                //设置UI的显示模式
                SetUIShowMode(ui);
                ShowUI(ui, paramList);
            }
        }

        /// <summary>
        /// 初始化UIPrefab
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="uiObj"></param>
        /// <param name="name"></param>
        /// <param name="resource"></param>
        /// <param name="paramList"></param>
        void InitPrefab(BaseUI ui, GameObject uiObj, string name, AssetAddress resource = AssetAddress.AssetBundle, params object[] paramList)
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

            ui.Awake(paramList);

            ShowUI(ui, paramList);
        }

        /// <summary>
        /// 初始化UIPrefab
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="uiObj"></param>
        /// <param name="name"></param>
        /// <param name="resource"></param>
        /// <param name="paramList"></param>
        void InitFairyGUIPanel(BaseUI ui, string name, AssetAddress resource = AssetAddress.AssetBundle, params object[] paramList)
        {
            //添加到所有UI字典中去
            if (!m_AllUIDic.ContainsKey(name))
            {
                m_AllUIDic.Add(name, ui);
            }

            //设置UI的显示模式
            SetUIShowMode(ui);

            ui.Awake(paramList);

            ShowUI(ui, paramList);
        }

        /// <summary>
        /// 设置UI的挂在节点
        /// </summary>
        /// <param name="ui"></param>
        void SetUIRoot(BaseUI ui)
        {
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
                    ShowUI_Normal(ui);
                    break;
                case UIShowMode.ReverseChange:          //需要“反向切换”窗口模式
                    ShowUI_ReverseChange(ui);
                    break;
                case UIShowMode.HideOther:              //“隐藏其他”窗口模式
                    ShowUI_HideOther(ui);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 将UI添加到正在显示UI字典
        /// </summary>
        /// <param name="ui"></param>
        private void ShowUI_Normal(BaseUI ui)
        {
            //容错处理，“正在显示”的集合中，存在这个UI窗体，则直接返回不处理了
            if (m_CurrentShowUIDic.ContainsKey(ui.Name))
                return;
            m_CurrentShowUIDic.Add(ui.Name, ui);
            m_CurrentShowList.Add(ui);
        }

        /// <summary>
        /// 将UI添加到栈里
        /// </summary>
        /// <param name="ui"></param>
        private void ShowUI_ReverseChange(BaseUI ui)
        {
            //判断“栈”集合中，是否有其他的窗体，有则关闭栈顶
            if (m_StackUI.Count > 0)
            {
                BaseUI topUIForm = m_StackUI.Peek();
                //栈顶元素作隐藏
                HideUI(topUIForm);
            }
            //把指定的UI窗体，入栈操作。
            m_StackUI.Push(ui);
        }

        /// <summary>
        /// 隐藏其他UI
        /// </summary>
        /// <param name="ui"></param>
        private void ShowUI_HideOther(BaseUI ui)
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
            m_CurrentShowList.Add(ui);
        }

        /// <summary>
        /// 设置UI遮罩
        /// </summary>
        /// <param name="show"></param>
        void SetMask(bool show)
        {
            if (show != m_UIMask.activeSelf)
                m_UIMask.SetActive(show);
        }

        /// <summary>
        /// 根据窗口名称显示窗口
        /// </summary>
        /// <param name="name"></param>
        /// <param name="paramList"></param>
        public void ShowUI(string name, params object[] paramList)
        {
            BaseUI ui = FindUIByName<BaseUI>(name);
            ShowUI(ui, paramList);
        }

        /// <summary>
        /// 根据窗口对象，显示窗口
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="paramList"></param>
        public void ShowUI(BaseUI ui, params object[] paramList)
        {
            if (ui.m_UIType == UIType.FariyGUI)
            {
                ui.wnd.Show();
            }
            else
            {
                ui.GameObject.SetActive(true);
                ui.Transform.SetAsLastSibling();
            }
            ui.OnShow(paramList);
            //UI的开启完毕后，关闭遮罩
            SetMask(false);
        }

        /// <summary>
        /// 关闭窗口，根据名称
        /// </summary>
        public void CloseUI(string name, bool destory = false)
        {
            BaseUI ui = FindUIByName<BaseUI>(name);
            if (ui != null)
                CloseUI(ui, destory);
        }

        /// <summary>
        /// 关闭窗口,根据窗口对象
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="destory"></param>
        public void CloseUI(BaseUI ui, bool destory = false)
        {
            switch (ui.m_ShowMode)
            {
                case UIShowMode.Normal:
                    //普通窗体的关闭
                    CloseUI_Normal(ui);
                    break;
                case UIShowMode.ReverseChange:
                    //反向切换窗体的关闭
                    CloseUI_ReverseChange(ui);
                    break;
                case UIShowMode.HideOther:
                    //隐藏其他窗体关闭
                    CloseUI_HideOther(ui);
                    break;
                default:
                    break;
            }

            //是否销毁UI
            if (destory)
            {
                ///从AssetBundel加载的
                if (ui.Resource == AssetAddress.AssetBundle)
                {
                    ObjectManager.Instance.ReleaseObject(ui.GameObject, 0, true);
                }
                else if (ui.Resource == AssetAddress.Addressable)
                {
                    AddressableManager.Instance.Release(ui.GameObject);
                }
                else if (ui.Resource == AssetAddress.Resources)
                {
                    //从Resource加载的
                    GameObject.Destroy(ui.GameObject);
                }
                if (m_AllUIDic.ContainsKey(ui.Name))
                {
                    m_AllUIDic.Remove(ui.Name);
                    ui.GameObject = null;
                    ui = null;
                }
            }
            else
            {
                if (ui.Resource == AssetAddress.AssetBundle)
                {
                    ObjectManager.Instance.ReleaseObject(ui.GameObject, recycleParent: false);
                }
            }
        }

        /// <summary>
        /// 退出指定UI窗体
        /// </summary>
        /// <param name="strUIFormName"></param>
        private void CloseUI_Normal(BaseUI ui)
        {
            //容错处理，“正在显示”的集合中，不存在这个UI窗体，则直接返回不处理了
            if (!m_CurrentShowUIDic.ContainsKey(ui.Name))
                return;
            //指定窗体，标记为“隐藏状态”，且从"正在显示集合"中移除。
            HideUI(ui);
            m_CurrentShowUIDic.Remove(ui.Name);
            m_CurrentShowList.Remove(ui);
        }

        //（“反向切换”属性）窗体的出栈逻辑
        private void CloseUI_ReverseChange(BaseUI ui)
        {
            if (m_StackUI.Count >= 2)
            {
                //出栈处理
                BaseUI topUIForms = m_StackUI.Pop();
                //做隐藏处理
                HideUI(topUIForms);
                //出栈后，下一个窗体做“重新显示”处理。
                BaseUI nextUIForms = m_StackUI.Peek();
                ShowUI(nextUIForms, nextUIForms.m_Params);
            }
            else if (m_StackUI.Count == 1)
            {
                //出栈处理
                BaseUI topUIForms = m_StackUI.Pop();
                //做隐藏处理
                HideUI(topUIForms);
            }
        }

        /// <summary>
        /// (“隐藏其他”属性)关闭窗体，且显示其他窗体
        /// </summary>
        /// <param name="strUIName">打开的指定窗体名称</param>
        private void CloseUI_HideOther(BaseUI ui)
        {
            //容错处理，“正在显示”的集合中，不存在这个UI窗体，则直接返回不处理了
            if (!m_CurrentShowUIDic.ContainsKey(ui.Name))
                return;

            //当前窗体隐藏状态，且“正在显示”集合中，移除本窗体
            HideUI(ui);
            m_CurrentShowUIDic.Remove(ui.Name);
            m_CurrentShowList.Remove(ui);
            //把“正在显示集合”所有窗体都定义重新显示状态。
            foreach (BaseUI baseUI in m_CurrentShowUIDic.Values)
            {
                ShowUI(baseUI, baseUI.m_Params);
            }

            //把栈集合的栈顶元素重新显示
            if (m_StackUI.Count > 0)
            {
                BaseUI topUIForm = m_StackUI.Peek();
                //栈顶元素作冻结处理
                ShowUI(topUIForm, topUIForm.m_Params);
            }
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
                if (ui.m_UIType == UIType.FariyGUI)
                {
                    ui.wnd.Hide();
                }
                else
                {
                    ui.GameObject.SetActive(false);
                }
                ui.OnDisable();
            }
        }


        /// <summary>
        /// 显示或者隐藏所有的UI，通过根节点
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
        /// 是否存在Window
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ExisWindow(string name)
        {
            if (m_AllUIDic.ContainsKey(name))
            {
                return true;
            }
            return false;
        }
    }
}
