/****************************************************
	文件：UIBinder
	作者：ningwei
	日期：2020/11/27 17:56:49
	功能：xxx
*****************************************************/
using FairyGUI;
using FairyGUI.BackPack;
using FairyGUI.Common;
using HotFixProject;
using System.Collections.Generic;
using UnityEngine;

namespace Improve
{
    public class FairyGUIManager : HotSingleton<FairyGUIManager>
    {

        //FairyGUI的所有包的字典key是Fairy的包名，value是打成bundle后的bundle名称
        public Dictionary<string, string> m_FairyGUIList = new Dictionary<string, string>()
        {
            { "Assets/GameData/FairyGUI/Common","common_"},
            { "Assets/GameData/FairyGUI/BackPack","backpack_"},
        };

        //FairyGUI预加载的包的字典key是Fairy的包名，value是打成bundle后的bundle名称
        public Dictionary<string, string> m_PreFairyGUIList = new Dictionary<string, string>()
        {
            { "Assets/GameData/FairyGUI/Common","common_"},
        };

        internal void BindAll()
        {
            CommonBinder.BindAll();
            BackPackBinder.BindAll();
        }

        internal void PreAddPackage()
        {
            foreach (var item in m_PreFairyGUIList)
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    UIPackage.AddPackage(item.Key);
                }
                else
                {
                    AssetBundle ab = AssetBundleManager.Instance.LoadAssetBundle(item.Value);
                    UIPackage.AddPackage(ab);
                }
            }
        }
    }
}