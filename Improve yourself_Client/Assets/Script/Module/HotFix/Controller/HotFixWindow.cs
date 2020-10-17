/****************************************************
    文件：HotFixWindow.cs
	作者：NingWei
    日期：2020/9/23 18:13:54
	功能：Nothing
*****************************************************/

using System.Collections;
using UnityEngine;
using static CommonEnum;

public class HotFixWindow : Window 
{
    private HotFixPanel m_Panel;
    private float m_SumTime = 0;

    public override string PrefabName()
    {
        return "HotFixPanel.prefab";
    }

    public override void Awake(params object[] paralist)
    {
        m_Panel = GameObject.GetComponent<HotFixPanel>();
        m_Panel.m_HotFixProgress.fillAmount = 0;
        m_Panel.m_SpeedText.text = string.Format("{0:F}M/S", 0);
        m_Panel.m_InfoPanel.SetActive(false);
        HotPatchManager.Instance.ServerInfoError += ServerInfoError;
        HotPatchManager.Instance.ItemError += ItemError;
#if UNITY_EDITOR
        StartOnFinish();
#else
        if (HotPatchManager.Instance.ComputeUnPackFile())
        {
            m_Panel.m_SliderTopText.text = "解压中...";
            HotPatchManager.Instance.StartUnackFile(() =>
            {
                m_SumTime = 0;
                HotFix();
            });
        }
        else
        {
            HotFix();
        }
#endif
    }

    /// <summary>
    /// 检查热更
    /// </summary>
    void HotFix() {
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                //提示玩家是否热更下载
                PopUpUtil.OpenPopUpX("提示", "请检查网络连接是否正常", () =>
                  {
                      Application.Quit();
                  }, () =>
                 {
                     Application.Quit();
                 });
            }
        }
        else
        {
            CheckVersion();
        }
    }

    void CheckVersion() {
        HotPatchManager.Instance.CheckVersion((hot) =>
        {
            if (hot)
            {
                //提示玩家是否热更下载
                PopUpUtil.OpenPopUpX("提示", string.Format("当前版本为{0},有{1:F}M大小热更包，是否确定下载？", HotPatchManager.Instance.CurVersion, HotPatchManager.Instance.LoadSumSize / 1024.0f), 
                    OnClickStartDownLoad,
                    OnClickCancleDownLoad);
            }
            else
            {
                StartOnFinish();
            }
        });
    }

    public override void OnClose()
    {
        base.OnClose();
        HotPatchManager.Instance.ServerInfoError -= ServerInfoError;
        HotPatchManager.Instance.ItemError -= ItemError;

        //加载场景
        GameMapManager.Instance.LoadScene(ConStr.MenuScene);
    }

    void ServerInfoError() {
        PopUpUtil.OpenPopUpX("服务器列表获取失败", "服务器列表获取失败，请检查网络链接是否正常？尝试重新下载！", CheckVersion, Application.Quit);
    }

    void ItemError(string error) {
        PopUpUtil.OpenPopUpX("资源下载失败", string.Format("{0}等资源下载失败，请重新尝试下载！", error), AnewDownload, Application.Quit);
    }

    void AnewDownload()
    {
        HotPatchManager.Instance.CheckVersion((hot) =>
        {
            if (hot)
            {
                StartDownLoad();
            }
            else
            {
                StartOnFinish();
            }
        });
    }

    void OnClickStartDownLoad()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
            if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            {
                PopUpUtil.OpenPopUpX("下载确认", "当前使用的是手机流量，是否继续下载？", StartDownLoad, OnClickCancleDownLoad);
            }
        }
        else
        {
            StartDownLoad();
        }
    }

    void OnClickCancleDownLoad()
    {
        Application.Quit();
    }

    /// <summary>
    /// 正式开始下载
    /// </summary>
    void StartDownLoad()
    {
        m_Panel.m_SliderTopText.text = "下载中...";
        m_Panel.m_InfoPanel.SetActive(true);
        m_Panel.m_HotContentText.text = HotPatchManager.Instance.CurrentPatches.Des;
        GameStart.Instance.StartCoroutine(HotPatchManager.Instance.StartDownLoadAB(StartOnFinish));
    }

    /// <summary>
    /// 下载完成回调，或者没有下载的东西直接进入游戏
    /// </summary>
    void StartOnFinish()
    {
        GameStart.Instance.StartCoroutine(OnFinish());
    }

    IEnumerator OnFinish()
    {
        yield return GameStart.Instance.StartCoroutine(GameStart.Instance.StartGame(m_Panel.m_HotFixProgress, m_Panel.m_SliderTopText));
        UIManager.Instance.CloseWindow(this);
    }

    public override void OnUpdate()
    {
        if (m_Panel == null)
            return;
        if (HotPatchManager.Instance.StartUnPack)
        {
            m_SumTime += Time.deltaTime;
            m_Panel.m_HotFixProgress.fillAmount = HotPatchManager.Instance.GetUnpackProgress();
            float speed = (HotPatchManager.Instance.AlreadyUnPackSize / 1024.0f) / m_SumTime;
            if (m_Panel.m_HotFixProgress.fillAmount == 1)
            {
                m_Panel.m_SliderTopText.text = "解压完成";
                m_Panel.m_SpeedText.text = "";
            }
            else {
                m_Panel.m_SpeedText.text = string.Format("{0:F} M/S", speed);
            }
        }

        if (HotPatchManager.Instance.StartDownload)
        {
            m_SumTime += Time.deltaTime;
            m_Panel.m_HotFixProgress.fillAmount = HotPatchManager.Instance.GetProgress();
            float speed = (HotPatchManager.Instance.GetLoadSize() / 1024.0f) / m_SumTime;
            if (m_Panel.m_HotFixProgress.fillAmount == 1)
            {
                m_Panel.m_SliderTopText.text = "下载完成";
                m_Panel.m_SpeedText.text = "";
            }
            else
            {
                m_Panel.m_SpeedText.text = string.Format("{0:F} M/S", speed);
            }
        }
    }
}