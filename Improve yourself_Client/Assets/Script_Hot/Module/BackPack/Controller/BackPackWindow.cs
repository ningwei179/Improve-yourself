/****************************************************
	文件：BackPackWindow
	作者：ningwei
	日期：2020/11/27 17:41:17
	功能：背包UI
*****************************************************/
using FairyGUI.BackPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Improve
{
	public class BackPackWindow:BaseUI
	{
        private UI_BackPackPanel m_Panel;

        private string m_SceneName;

        public override void Init()
        {
            m_UIType = UIType.FariyGUI;
            m_UIRoot = UIRoot.Normal;
            m_ShowMode = UIShowMode.ReverseChange;
            UIResourceName = "Assets/GameData/FairyGUI/BackPack";
        }

        public override void Awake(params object[] paralist)
        {
            m_Panel = UI_BackPackPanel.CreateInstance();
            wnd.contentPane = m_Panel;
        }

        public override void OnShow(params object[] paramList)
        {
            base.OnShow(paramList);
        }

        public override void OnUpdate()
        {

        }
    }
}