/****************************************************
    文件：HotFixPanel.cs
	作者：NingWei
    日期：2020/9/23 18:14:47
	功能：HotFixPanel
*****************************************************/

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace Improve
{
    public class HotFixPanel : MonoBehaviour
    {
        public Image m_HotFixProgress;

        public Text m_ProgressText;

        public Text m_LoadingText;

        public GameObject m_DownLoadText;

        public Text LoadTipText;

        public Text m_LoadSizeText;

        public Text SpeedTipText;

        public Text m_SpeedText;

        public GameObject m_InfoPanel;

        public Text m_HotContentText;

        private float m_SumTime = 0;

        public void Awake()
        {
            m_HotFixProgress.fillAmount = 0;
            m_ProgressText.text = "";
            m_LoadingText.text = "";
            m_DownLoadText.SetActive(false);
            SpeedTipText.text = "速度：";
            m_InfoPanel.SetActive(false);
            if (FrameConstr.UseAssetAddress == AssetAddress.Addressable)
            {
                AddressableUpdateManager.Instance.ServerInfoError += ServerInfoError;
                AddressableUpdateManager.Instance.ItemError += ItemError;
            }
            else
            {
                HotPatchManager.Instance.ServerInfoError += ServerInfoError;
                HotPatchManager.Instance.ItemError += ItemError;
            }
        }

        public void OpenUI()
        {
#if UNITY_EDITOR
            StartOnFinish();
#else
        if (FrameConstr.UseAssetAddress == AssetAddress.Addressable)
        {
            HotFix();
        }
        else
        {
            if (HotPatchManager.Instance.ComputeUnPackFile())
            {
                LoadTipText.text = "解压中...";
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
        }
#endif
        }

        /// <summary>
        /// 检查热更
        /// </summary>
        void HotFix()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    //提示玩家是否热更下载
                    //PopUpUtil.OpenPopUpX("提示", "请检查网络连接是否正常", () =>
                    //{
                    //    Application.Quit();
                    //}, () =>
                    //{
                    //    Application.Quit();
                    //});
                }
            }
            else
            {
                CheckVersion();
            }
        }

        void CheckVersion()
        {
            if (FrameConstr.UseAssetAddress != AssetAddress.Addressable)
            {
                HotPatchManager.Instance.CheckVersion((hot) =>
                {
                    if (hot)
                    {
                    //提示玩家是否热更下载
                    //PopUpUtil.OpenPopUpX("提示", string.Format("当前版本为{0},有{1:F}M大小热更包，是否确定下载？", HotPatchManager.Instance.CurVersion, HotPatchManager.Instance.LoadSumSize),
                    //    OnClickStartDownLoad,
                    //    OnClickCancleDownLoad);

                    //直接下载不提示了
                    OnClickStartDownLoad();
                    }
                    else
                    {
                        StartOnFinish();
                    }
                });
            }
            else
            {
                AddressableUpdateManager.Instance.CheckVersion((hot) =>
                {
                    if (hot)
                    {
                    //提示玩家是否热更下载
                    //PopUpUtil.OpenPopUpX("提示", string.Format("当前版本有{0:F}M大小热更包，是否确定下载？", AddressableUpdateManager.Instance.LoadSumSize / 1024f / 1024f),
                    //    OnClickStartDownLoad,
                    //    OnClickCancleDownLoad);

                    //不提示了，直接更新
                    OnClickStartDownLoad();
                    }
                    else
                    {
                        StartOnFinish();
                    }
                });
            }
        }

        void ServerInfoError()
        {
            //PopUpUtil.OpenPopUpX("服务器列表获取失败", "服务器列表获取失败，请检查网络链接是否正常？尝试重新下载！", CheckVersion, Application.Quit);
        }

        void ItemError(string error)
        {
            //PopUpUtil.OpenPopUpX("资源下载失败", string.Format("{0}等资源下载失败，请重新尝试下载！", error), AnewDownload, Application.Quit);
        }

        void AnewDownload()
        {
            if (FrameConstr.UseAssetAddress != AssetAddress.Addressable)
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
            else
            {
                AddressableUpdateManager.Instance.CheckVersion((hot) =>
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
        }

        void OnClickStartDownLoad()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
            {
                if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
                {
                    //PopUpUtil.OpenPopUpX("下载确认", "当前使用的是手机流量，是否继续下载？", StartDownLoad, OnClickCancleDownLoad);
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
            m_DownLoadText.SetActive(true);

            LoadTipText.text = "下载资源:";

            if (FrameConstr.UseAssetAddress == AssetAddress.Addressable)
            {
                m_InfoPanel.SetActive(false);
                GameStart.Instance.StartCoroutine(AddressableUpdateManager.Instance.StartDownLoadAB(StartOnFinish));
            }
            else
            {
                m_InfoPanel.SetActive(true);
                GameStart.Instance.StartCoroutine(HotPatchManager.Instance.StartDownLoadAB(StartOnFinish));
                m_HotContentText.text = HotPatchManager.Instance.CurrentPatches.Des;
            }
        }

        /// <summary>
        /// 下载完成回调，或者没有下载的东西直接进入游戏
        /// </summary>
        void StartOnFinish()
        {
            StartCoroutine(OnFinish());
        }

        IEnumerator OnFinish()
        {
            yield return GameStart.Instance.StartCoroutine(GameStart.Instance.StartGame(m_HotFixProgress, m_LoadingText, m_ProgressText));
            HotPatchManager.Instance.ServerInfoError -= ServerInfoError;
            HotPatchManager.Instance.ItemError -= ItemError;

            //加载场景
            GameMapManager.Instance.LoadScene(ConStr.MenuScene);
        }

        public void Update()
        {
            if (FrameConstr.UseAssetAddress != AssetAddress.Addressable)
            {
                if (HotPatchManager.Instance.StartUnPack)
                {
                    m_SumTime += Time.deltaTime;
                    m_HotFixProgress.fillAmount = HotPatchManager.Instance.GetUnpackProgress();

                    m_ProgressText.text = string.Format("{0}%", (int)(m_HotFixProgress.fillAmount * 100));

                    float speed = (HotPatchManager.Instance.AlreadyUnPackSize / 1024.0f) / m_SumTime;
                    if (m_HotFixProgress.fillAmount == 1)
                    {
                        LoadTipText.text = "解压完成";
                        m_SpeedText.text = "";
                    }
                    else
                    {
                        m_LoadSizeText.text = string.Format("{0}M/{1}M", HotPatchManager.Instance.AlreadyUnPackSize / 1024f, HotPatchManager.Instance.UnPackSumSize / 1024f);
                        m_SpeedText.text = string.Format("{0:F} M/S", speed);
                    }
                }

                if (HotPatchManager.Instance.StartDownload)
                {
                    m_SumTime += Time.deltaTime;
                    m_HotFixProgress.fillAmount = HotPatchManager.Instance.GetProgress();

                    m_ProgressText.text = string.Format("{0}%", (int)(m_HotFixProgress.fillAmount * 100));

                    float speed = (HotPatchManager.Instance.GetLoadSize() / 1024.0f) / m_SumTime;
                    if (m_HotFixProgress.fillAmount == 1)
                    {
                        LoadTipText.text = "下载完成";
                    }
                    else
                    {
                        m_LoadSizeText.text = string.Format("{0}M/{1}M", HotPatchManager.Instance.GetLoadSize() / 1024f, HotPatchManager.Instance.LoadSumSize / 1024f);
                        m_SpeedText.text = string.Format("{0:F} M/S", speed);
                    }
                }
            }
            else
            {
                if (AddressableUpdateManager.Instance.StartDownload)
                {
                    m_SumTime += Time.deltaTime;

                    if (m_HotFixProgress.fillAmount == 1)
                    {
                        LoadTipText.text = "下载完成";
                    }
                    else
                    {
                        m_HotFixProgress.fillAmount = AddressableUpdateManager.Instance.GetProgress();
                        m_ProgressText.text = string.Format("{0}%", (int)(m_HotFixProgress.fillAmount * 100));
                        float speed = (AddressableUpdateManager.Instance.GetLoadSize() / 1024f / 1024f) / m_SumTime;
                        m_LoadSizeText.text = string.Format("{0}M/{1}M", AddressableUpdateManager.Instance.GetLoadSize() / 1024f / 1024f, AddressableUpdateManager.Instance.LoadSumSize / 1024f / 1024f);
                        m_SpeedText.text = string.Format("{0:F} M/S", speed);
                    }
                }
            }
        }
    }
}