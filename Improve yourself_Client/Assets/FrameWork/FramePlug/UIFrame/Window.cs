/****************************************************
	文件：Window.cs
	作者：NingWei
	日期：2020/09/07 11:34   	
	功能：UI窗口基类
*****************************************************/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Window
{
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
    /// 是否是Resource加载的
    /// </summary>
    public bool Resource { get; set; } = false;

    /// <summary>
    /// 是否是热更的
    /// </summary>
    public bool IsHotFix { get; set; } = false;

    /// <summary>
    /// 热更的类名
    /// </summary>
    public string HotFixClassName { get; set; }

    //所有的Button
    protected List<Button> m_AllButton = new List<Button>();

    //所有的Toggle
    protected List<Toggle> m_AllToggle = new List<Toggle>();

    public virtual string PrefabName()
    {
        return "";
    }

    public virtual bool OnMessage(UIMsgID msgID, params object[] paralist)
    {
        return true;
    }

    public virtual void Awake(params object[] paramList)
    {

    }


    public virtual void OnShow(params object[] paramList)
    {

    }

    public virtual void OnDisable()
    {

    }

    public virtual void OnUpdate()
    {

    }

    public virtual void OnClose()
    {

    }

    /// <summary>
    /// 同步替换图片
    /// </summary>
    /// <param name="path"></param>
    /// <param name="image"></param>
    /// <param name="setNativeSize"></param>
    /// <returns></returns>
    public bool ChangeImageSprite(string path,Image image,bool setNativeSize = false)
    {
        if (image == null)
            return false;

        Sprite sp = ResourceManager.Instance.LoadResource<Sprite>(path);
        if (sp != null)
        {
            if (image.sprite != null)
                image.sprite = null;

            image.sprite = sp;

            if(setNativeSize)
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
    public void ChangeImageSpriteAsync(string path, Image image, bool setNativeSize = false)
    {
        if (image == null)
            return;

        ResourceManager.Instance.AsyncLoadResource(path, (string resPath, Object obj, object param1, object param2, object param3) =>
        {
            if (obj != null)
            {
                Sprite m_Sp = obj as Sprite;
                Image m_Imgae = param1 as Image;
                bool m_SetNativeSize = (bool)param2;

                if (m_Imgae.sprite != null)
                    m_Imgae.sprite = null;

                m_Imgae.sprite = m_Sp;

                if (m_SetNativeSize)
                    image.SetNativeSize();
            }
        },LoadResPriority.RES_MIDDLE,image,setNativeSize,true);
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
