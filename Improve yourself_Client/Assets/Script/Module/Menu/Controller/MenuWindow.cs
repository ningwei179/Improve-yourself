/****************************************************
	文件：MenuWindow.cs
	作者：NingWei
	日期：2020/09/07 11:31   	
	功能：处理菜单界面的逻辑
*****************************************************/

using Protocal;
using UnityEngine;

public class MenuWindow : Window
{
    private MenuPanel m_MainPanel;

    public override string PrefabName()
    {
        return "MenuPanel";
    }

    public override void Awake(params object[] paralist)
    {
        m_MainPanel = GameObject.GetComponent<MenuPanel>();
        AddButtonClickListener(m_MainPanel.m_StartButton, OnClickStart);
        AddButtonClickListener(m_MainPanel.m_LoadButton, OnClickLoad);
        AddButtonClickListener(m_MainPanel.m_ExitButton, OnClickExit);
        ResourceManager.Instance.AsyncLoadResource("Assets/GameData/UISprite/image1.png", (string resourcePath, Object obj, object param1, object param2, object param3) =>
        {
            if (obj != null)
            {
                Sprite sp = obj as Sprite;
                if (sp != null)
                {
                    m_MainPanel.Test1.sprite = sp;
                    Debug.Log("图片1加载出来了");
                }

            }
        }, LoadResPriority.RES_MIDDLE,true);

        ResourceManager.Instance.AsyncLoadResource("Assets/GameData/UISprite/image2.png", (string resourcePath, Object obj, object param1, object param2, object param3) =>
        {
            if (obj != null)
            {
                Sprite sp = obj as Sprite;
                if (sp != null)
                {
                    m_MainPanel.Test2.sprite = sp;
                    Debug.Log("图片2加载出来了");
                }
            }
        }, LoadResPriority.RES_HIGHT, true);
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            ResourceManager.Instance.ReleaseResource("Assets/GameData/UISprite/image1.png", true);
            ResourceManager.Instance.ReleaseResource("Assets/GameData/UISprite/image2.png", true);
        }
    }

    void OnClickStart()
    {
        NetMsg msg = new NetMsg();
        msg.cmd = (int)CMD.Reqlogin;
        msg.reqLogin = new ReqLogin();
        msg.reqLogin.account = "10001";
        msg.reqLogin.pass = "10001";
        NetWorkManager.Instance.SendMsg(msg);
    }

    void OnClickLoad()
    {

    }

    void OnClickExit()
    {

    }
}
