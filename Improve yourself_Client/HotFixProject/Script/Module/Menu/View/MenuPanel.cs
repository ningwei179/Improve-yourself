/****************************************************
	文件：MenuPanel.cs
	作者：NingWei
	日期：2020/09/07 11:29   	
	功能：游戏菜单界面
*****************************************************/

using UnityEngine;
using UnityEngine.UI;
namespace Improve
{
    public class MenuPanel : MonoBehaviour
    {
        public Button m_StartButton;
        public Button m_LoadButton;
        public Button m_ExitButton;

        public Image Test1;
        public Image Test2;

        private void Awake()
        {
            m_StartButton = this.transform.Find("Bg/StartButton").GetComponent<Button>();
            m_LoadButton = this.transform.Find("Bg/LoadButton").GetComponent<Button>();
            m_ExitButton = this.transform.Find("Bg/ExitButton").GetComponent<Button>();
            Test1 = this.transform.Find("Bg/Test1").GetComponent<Image>();
            Test2 = this.transform.Find("Bg/Test2").GetComponent<Image>();
        }
    }
}