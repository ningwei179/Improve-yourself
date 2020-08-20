using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public string Name { get;set }

    //所有的Button
    protected List<Button> m_AllButton = new List<Button>();

    //所有的Toggle
    protected List<Toggle> m_AllTogglen = new List<Toggle>();

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
}
