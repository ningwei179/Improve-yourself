/****************************************************
	文件：UIBinder
	作者：ningwei
	日期：2020/11/27 17:56:49
	功能：xxx
*****************************************************/
using FairyGUI.BackPack;
using FairyGUI.Common;
using HotFixProject;

namespace Improve
{
    public class FairyGUIBinder : HotSingleton<FairyGUIBinder>
    {
        internal void BindAll()
        {
            CommonBinder.BindAll();
            BackPackBinder.BindAll();
        }
    }
}