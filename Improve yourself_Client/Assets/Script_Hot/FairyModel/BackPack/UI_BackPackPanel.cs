/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace FairyGUI.BackPack
{
    public partial class UI_BackPackPanel : GComponent
    {
        public GList m_ItemList;
        public const string URL = "ui://1cnrr7lvg0910";

        public static UI_BackPackPanel CreateInstance()
        {
            return (UI_BackPackPanel)UIPackage.CreateObject("BackPack", "BackPackPanel");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_ItemList = (GList)GetChildAt(0);
        }
    }
}