/****************************************************
	文件：BackPackWindow
	作者：ningwei
	日期：2020/11/27 17:41:17
	功能：背包UI
*****************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Improve
{
	public class BackPackWindow:BaseUI
	{
        private LoadingPanel m_Panel;

        private string m_SceneName;

        public override void Init()
        {
            m_UIType = UIType.FariyGUI;
            m_UIRoot = UIRoot.Normal;
            m_ShowMode = UIShowMode.Normal;
            PrefabName = "LoadingPanel.prefab";
        }

        public override void Awake(params object[] paralist)
        {

        }

        public override void OnUpdate()
        {

        }
    }
}