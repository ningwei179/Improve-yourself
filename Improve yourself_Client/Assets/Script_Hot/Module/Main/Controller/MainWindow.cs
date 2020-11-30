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
            UIResourceName = "MainPanel.prefab";
        }

        public override void Awake(params object[] paramList)
        {
            base.Awake(paramList);
            m_Panel = GameObject.GetComponent<MainPanel>();
            if (m_Panel == null)
                m_Panel = GameObject.AddComponent<MainPanel>();
            AddButtonClickListener(m_Panel.m_BackBtn, OnClose);
            AddButtonClickListener(m_Panel.m_BackPackBtn, OnClickBackPack);
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

        void OnClickBackPack() {
            UIManager.Instance.OpenUI<BackPackWindow>(ConStr.BackPackPanel);
        }
    }
}