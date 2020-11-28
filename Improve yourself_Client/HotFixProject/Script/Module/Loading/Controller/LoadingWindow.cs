/****************************************************
	文件：LoadingWindow.cs
	作者：NingWei
	日期：2020/09/07 11:30   	
	功能：处理加载界面的逻辑
*****************************************************/

using UnityEngine;
namespace Improve
{

    public class LoadingWindow : BaseUI
    {
        private LoadingPanel m_Panel;

        private string m_SceneName;

        public override void Init()
        {
            m_UIRoot = UIRoot.Normal;
            m_ShowMode = UIShowMode.Normal;
            PrefabName = "LoadingPanel.prefab";
        }

        public override void Awake(params object[] paralist)
        {
            Debug.Log("loading log LoadingWindow Awake");
            m_Panel = GameObject.GetComponent<LoadingPanel>();
            if (m_Panel == null)
                m_Panel = GameObject.AddComponent<LoadingPanel>();
            m_SceneName = paralist[0] as string;
        }

        public override void OnUpdate()
        {
            Debug.Log("loading log LoadingWindow OnUpdate");
            if (m_Panel == null)
                return;
            Debug.Log("loading log LoadingWindow OnUpdate2"+ GameMapManager.LoadingProgress);
            m_Panel.m_Slider.value = GameMapManager.LoadingProgress / 100.0f;
            m_Panel.m_Text.text = string.Format("{0}%", GameMapManager.LoadingProgress);

            if (GameMapManager.LoadingProgress >= 100)
            {
                LoadOherScene();
            }
        }

        public void LoadOherScene()
        {
            if (m_SceneName == ConStr.MenuScene)
            {
                UIManager.Instance.OpenUI<MenuWindow>(ConStr.MenuPanel);
            }

            //关闭界面
            base.OnClose();
        }
    }
}