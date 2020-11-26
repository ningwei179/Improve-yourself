using UnityEngine;
using UnityEngine.UI;
namespace Improve
{
    public class MainWindow : BaseUI
    {
        private MainPanel m_Panel;
        public override void Init()
        {
            m_UIRoot = UIRoot.Normal;
            m_ShowMode = UIShowMode.Normal;
            PrefabName = "MainPanel.prefab";
        }

        public override void Awake(params object[] paramList)
        {
            base.Awake(paramList);
            m_Panel = GameObject.GetComponent<MainPanel>();
            if (m_Panel == null)
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
            UIManager.Instance.OpenUI<MenuWindow>(ConStr.MenuPanel);
        }
    }
}