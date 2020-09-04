using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.UI;

public class MenuWindow : Window
{
    private MenuPanel m_MainPanel;

    public override string PrefabPath()
    {
        return "Assets/GameData/Prefabs/UGUI/Panel/MenuPanel.prefab";
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

    }

    void OnClickLoad()
    {

    }

    void OnClickExit()
    {

    }
}