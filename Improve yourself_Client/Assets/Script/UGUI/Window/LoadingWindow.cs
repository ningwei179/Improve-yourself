using UnityEngine;

public class LoadingWindow : Window
{
    private LoadingPanel m_LoadingPanel;

    private string m_SceneName;

    public override string PrefabPath()
    {
        return "Assets/GameData/Prefabs/UGUI/Panel/LoadingPanel.prefab";
    }

    public override void Awake(params object[] paralist)
    {
        m_LoadingPanel = GameObject.GetComponent<LoadingPanel>();
        m_SceneName = paralist[0] as string;
    }

    public override void OnUpdate()
    {
        if (m_LoadingPanel == null)
            return;

        m_LoadingPanel.m_Slider.value = GameMapManager.LoadingProgress / 100.0f;
        m_LoadingPanel.m_Text.text = string.Format("{0}%", GameMapManager.LoadingProgress);

        if (GameMapManager.LoadingProgress >= 100)
        {
            LoadOherScene();
        }
    }

    public void LoadOherScene()
    {
        if (m_SceneName == ConStr.MenuScene)
        {
            UIManager.Instance.PopUpWindow(ConStr.MenuPanel);
        }

        //loading结束了，关闭loadingUI
        UIManager.Instance.CloseWindow(ConStr.LoadingPanel);
    }
}
