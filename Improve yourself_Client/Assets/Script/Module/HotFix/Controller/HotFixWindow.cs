/****************************************************
    文件：HotFixWindow.cs
	作者：NingWei
    日期：2020/9/23 18:13:54
	功能：Nothing
*****************************************************/

using UnityEngine;

public class HotFixWindow : Window 
{
    private HotFixPanel m_HotFixPanel;

    private string m_SceneName;

    public override string PrefabName()
    {
        return "HotFixPanel";
    }

    public override void Awake(params object[] paralist)
    {
        m_HotFixPanel = GameObject.GetComponent<HotFixPanel>();
        m_SceneName = paralist[0] as string;
    }

    public override void OnUpdate()
    {
        if (m_HotFixPanel == null)
            return;
    }
}