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

        /// <summary>
        /// 预加载FairyGUI的包
        /// </summary>
        internal void PreAddPackage()
        {
            foreach (var item in m_PreFairyGUIList)
            {
                if (FrameConstr.UseAssetAddress == AssetAddress.Addressable)
                {
                    //加载FairyGUI Package
                    //string desname = item.Key + "_fui.bytes";
                    //AddressableManager.Instance.AsyncLoadResource<TextAsset>(desname, (TextAsset text) =>
                    //{
                    //    Debug.Log("desLoadSuc");
                    //    UIPackage.AddPackage(
                    //        text.bytes,
                    //        "Common",
                    //        async (string fairyname, string extension, Type type, PackageItem ite) =>
                    //        {
                    //            Debug.Log($"{fairyname}, {extension}, {type.ToString()}, {ite.ToString()}");

                    //            string texturePath = "Assets/GameData/FairyGUI/" + fairyname + extension;

                    //            if (type == typeof(Texture))
                    //            {
                    //                AddressableManager.Instance.AsyncLoadResource<Texture>(fairyname, (Texture tex) =>
                    //                {
                    //                    ite.owner.SetItemAsset(ite, tex, DestroyMethod.Custom);
                    //                });
                    //            }
                    //        });
                    //});
                }
                else if (FrameConstr.UseAssetAddress == AssetAddress.AssetBundle)
                {
                    AssetBundle ab = AssetBundleManager.Instance.LoadAssetBundle(item.Value);
                    UIPackage.AddPackage(ab);
                }
                else
                {
                    UIPackage.AddPackage(item.Key);
                }
            }
        }
    }
}
