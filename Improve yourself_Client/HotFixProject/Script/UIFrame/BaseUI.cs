/****************************************************
	文件：Window.cs
	作者：NingWei
	日期：2020/09/07 11:34   	
	功能：UI窗口基类
*****************************************************/
using FairyGUI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace Improve
{

    public class BaseUI
    {
        /// <summary>
        /// FairyGUI的窗口
        /// </summary>
        public Window wnd;

        /// <summary>
        /// 引用GameObject
        /// </summary>
        public GameObject GameObject { get; set; }

        /// <summary>
        /// 引用Transform
        /// </summary>
        public Transform Transform { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 资产的加载地址
        /// </summary>
        public AssetAddress Resource { get; set; } = AssetAddress.AssetBundle;


        //所有的Button
        protected List<Button> m_AllButton = new List<Button>();

        //所有的Toggle
        protected List<Toggle> m_AllToggle = new List<Toggle>();

        //UI的参数列表
        public object[] m_Params;

        //Prefab的名称
        public string PrefabName { get; set; }

        //是否是FairyGUI
        public UIType m_UIType = UIType.UGUI;

        //UI的挂载节点，默认是普通的节点
        public UIRoot m_UIRoot = UIRoot.Normal;

        //UI的显示模式，默认是普通
        public UIShowMode m_ShowMode = UIShowMode.Normal;

        public virtual void Init()
        {

        }

        public virtual bool OnMessage(UIMsgID msgID, params object[] paralist)
        {
            return true;
        }

        public virtual void Awake(params object[] paramList)
        {
            m_Params = paramList;
        }


        public virtual void OnShow(params object[] paramList)
        {
            m_Params = paramList;
        }

        public virtual void OnDisable()
        {

        }

        public virtual void OnUpdate()
        {

        }

        public virtual void OnClose()
        {
            UIManager.Instance.CloseUI(this);
        }

        /// <summary>
        /// 同步替换图片
        /// </summary>
        /// <param name="path"></param>
        /// <param name="image"></param>
        /// <param name="setNativeSize"></param>
        /// <returns></returns>
        public bool ChangeImageSprite(string path, UnityEngine.UI.Image image, bool setNativeSize = false)
        {
            if (image == null)
                return false;

            Sprite sp = ResourceManager.Instance.LoadResource<Sprite>(path);
            if (sp != null)
            {
                if (image.sprite != null)
                    image.sprite = null;

                image.sprite = sp;

                if (setNativeSize)
                    image.SetNativeSize();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 异步替换图片
        /// </summary>
        /// <param name="paht"></param>
        /// <param name="image"></param>
        /// <param name="setNativeSize"></param>
        public void ChangeImageSpriteAsync(string path, UnityEngine.UI.Image image, bool setNativeSize = false)
        {
            if (image == null)
                return;

            ResourceManager.Instance.AsyncLoadResource(path, (string resPath, Object obj, object[] paramArr) =>
            {
                if (obj != null)
                {
                    Sprite m_Sp = obj as Sprite;
                    UnityEngine.UI.Image m_Imgae = paramArr[0] as UnityEngine.UI.Image;
                    bool m_SetNativeSize = (bool)paramArr[1];

                    if (m_Imgae.sprite != null)
                        m_Imgae.sprite = null;

                    m_Imgae.sprite = m_Sp;

                    if (m_SetNativeSize)
                        image.SetNativeSize();
                }
            }, LoadResPriority.RES_MIDDLE, true, 0, image, setNativeSize);
        }

        /// <summary>
        /// 移除所有的btn事件
        /// </summary>
        public void RemoveAllBtnListener()
        {
            foreach (Button btn in m_AllButton)
            {
                btn.onClick.RemoveAllListeners();
            }
        }

        /// <summary>
        /// 移除所有的Toggle事件
        /// </summary>
        public void RemoveAllTogglenListener()
        {
            foreach (Toggle toggle in m_AllToggle)
            {
                toggle.onValueChanged.RemoveAllListeners();
            }
        }

        /// <summary>
        /// 添加button 事件监听
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="action"></param>
        public void AddButtonClickListener(Button btn, UnityAction action)
        {
            if (btn != null)
            {
                if (!m_AllButton.Contains(btn))
                {
                    m_AllButton.Add(btn);
                }
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(action);
                btn.onClick.AddListener(BtnPlaySound);
            }
        }

        /// <summary>
        /// 添加Toggle 事件监听
        /// </summary>
        public void AddToggleClickListener(Toggle toggle, UnityAction<bool> action)
        {
            if (toggle != null)
            {
                if (!m_AllToggle.Contains(toggle))
                {
                    m_AllToggle.Add(toggle);
                }
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener(action);
                toggle.onValueChanged.AddListener(TogglePlaySound);
            }
        }

        /// <summary>
        /// 播放Btn声音
        /// </summary>
        void BtnPlaySound()
        {

        }

        /// <summary>
        /// 播放Toggle声音
        /// </summary>
        /// <param name="isOn"></param>
        void TogglePlaySound(bool isOn)
        {

        }
    }
}
