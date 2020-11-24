using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainWindow : Window
{
    private MainPanel m_Panel;
    public override string PrefabName()
    {
        return "MainPanel.prefab";
    }

    public override void Awake(params object[] paramList)
    {
        base.Awake(paramList);
        m_Panel = GameObject.AddComponent<MainPanel>();
        m_Panel.m_BackBtn = Transform.Find("Btn-Back").GetComponent<Button>();

        AddButtonClickListener(m_Panel.m_BackBtn, OnClose);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
    }

    public override void OnClose()
    {
        base.OnClose();
        UIManager.Instance.PopUpWindow(ConStr.MenuPanel);
    }
}
