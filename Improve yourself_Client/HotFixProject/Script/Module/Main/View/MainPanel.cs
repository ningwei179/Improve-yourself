using UnityEngine;
using UnityEngine.UI;
namespace Improve
{
    public class MainPanel : MonoBehaviour
    {
        public Button m_BackBtn;

        private void Awake()
        {
            m_BackBtn = transform.Find("Btn-Back").GetComponent<Button>();
        }
    }
}