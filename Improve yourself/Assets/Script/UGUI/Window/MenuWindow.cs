using UnityEngine;

public class MenuWindow : Window
{
    private MenuPanel m_MainPanel;

    private AudioClip m_Clip;

    public override string PrefabPath()
    {
        return "Assets/GameData/Prefabs/UGUI/Panel/MenuPanel.prefab";
    }

    public override void Awake(params object[] paralist)
    {
        m_MainPanel = GameObject.GetComponent<MenuPanel>();
        AddButtonClickListener(m_MainPanel.m_StartButton, OnClickStart);
        AddButtonClickListener(m_MainPanel.m_LoadButton, OnClickLoad);
        AddButtonClickListener(m_MainPanel.m_ExitButton, OnClickExit);
    }

    public override void OnUpdate()
    {

    }

    void OnClickStart()
    {

    }

    void OnClickLoad()
    {

    }

    void OnClickExit()
    {

    }
}
