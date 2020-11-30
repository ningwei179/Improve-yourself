using UnityEngine;
using UnityEngine.UI;
namespace Improve
{
    public class MainPanel : MonoBehaviour
    {
        public Button m_BackBtn;
        public Button m_BackPackBtn;

        private void Awake()
        {
            m_BackBtn = transform.Find("Bg/Btn-Back").GetComponent<Button>();
            m_BackPackBtn = transform.Find("Bg/Btn-BackPack").GetComponent<Button>();
        }
    }
}