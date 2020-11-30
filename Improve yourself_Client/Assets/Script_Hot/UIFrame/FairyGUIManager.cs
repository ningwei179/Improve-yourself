/****************************************************
	文件：UIBinder
	作者：ningwei
	日期：2020/11/27 17:56:49
	功能：xxx
*****************************************************/
using FairyGUI.BackPack;
using FairyGUI.Common;
using HotFixProject;
using System;
using System.Collections.Generic;

namespace Improve
{
    public class FairyGUIManager : HotSingleton<FairyGUIManager>
    {
        public string FairyGUIPath = "Assets/GameData/FairyGUI";

        //FairyGUI所有的包列表
        public List<string> m_FairyGuiList = new List<string>() {
            "Common",
            "BackPack",
        };

        //FairyGUI预加载的包的列表
        public List<string> m_PreFairyGuiList = new List<string> {
            "Common_fui",
        };

        internal void BindAll()
        {
            CommonBinder.BindAll();
            BackPackBinder.BindAll();
        }

        internal void PreAddPackage()
        {
            for (int i = 0; i < m_PreFairyGuiList.Count; ++i) { 
                
            }
        }
    }
}