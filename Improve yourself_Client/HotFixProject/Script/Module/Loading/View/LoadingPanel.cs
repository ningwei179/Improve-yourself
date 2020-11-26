/****************************************************
	文件：LoadingPanel.cs
	作者：NingWei
	日期：2020/09/07 11:29   	
	功能：加载界面
*****************************************************/

using UnityEngine;
using UnityEngine.UI;
namespace Improve
{

    public class LoadingPanel : MonoBehaviour
    {
        public Slider m_Slider;

        public Text m_Text;

        private void Awake()
        {
            Debug.Log("loading log Awake");
            m_Slider = this.transform.Find("Bg/Slider").GetComponent<Slider>();
            m_Text = this.transform.Find("Bg/Text").GetComponent<Text>();
            Debug.Log("loading log Awake Down");
        }

        private void Update()
        {
            Debug.Log("loading log Awake");
        }
    }
}