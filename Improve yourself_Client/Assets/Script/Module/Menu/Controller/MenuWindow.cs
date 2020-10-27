/****************************************************
	文件：MenuWindow.cs
	作者：NingWei
	日期：2020/09/07 11:31   	
	功能：处理菜单界面的逻辑
*****************************************************/

using IYProtocal;
using UnityEngine;

public class MenuWindow : Window
{
    private MenuPanel m_Panel;

    public override string PrefabName()
    {
        return "MenuPanel.prefab";
    }

    public override void Awake(params object[] paralist)
    {
        m_Panel = GameObject.GetComponent<MenuPanel>();
        AddButtonClickListener(m_Panel.m_StartButton, OnClickStart);
        AddButtonClickListener(m_Panel.m_LoadButton, OnClickLoad);
        AddButtonClickListener(m_Panel.m_ExitButton, OnClickExit);
        ResourceManager.Instance.AsyncLoadResource("Assets/GameData/UISprite/image1.png", (string resourcePath, Object obj, object [] paramArr) =>
        {
            if (obj != null)
            {
                Sprite sp = obj as Sprite;
                if (sp != null)
                {
                    m_Panel.Test1.sprite = sp;
                    Debug.Log("图片1加载出来了");
                }

            }
        }, LoadResPriority.RES_MIDDLE,true);

        ResourceManager.Instance.AsyncLoadResource("Assets/GameData/UISprite/image2.png", (string resourcePath, Object obj, object[] paramArr) =>
        {
            if (obj != null)
            {
                Sprite sp = obj as Sprite;
                if (sp != null)
                {
                    m_Panel.Test2.sprite = sp;
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
