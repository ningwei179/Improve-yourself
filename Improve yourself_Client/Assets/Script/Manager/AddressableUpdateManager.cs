using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableUpdateManager : Singleton<AddressableUpdateManager>
{
    /// <summary>
    /// 开始下载
    /// </summary>
    public bool StartDownload = false;

    /// <summary>
    /// 尝试重新下载次数
    /// </summary>
    private int m_TryDownCount = 0;

    /// <summary>
    /// 尝试重新下载最大次数
    /// </summary>
    private const int DOWNLOADCOUNT = 4;

    /// <summary>
    /// 开始解压
    /// </summary>
    public bool StartUnPack;

    /// <summary>
    /// 当前版本
    /// </summary>
    public string CurVersion;

    /// <summary>
    /// 当前包名
    /// </summary>
    public string m_CurPackName;

    /// <summary>
    /// 总下载大小
    /// </summary>
    public float LoadSumSize;

    /// <summary>
    /// 更新列表没有获取到
    /// </summary>
    public Action ServerInfoError;

    /// <summary>
    /// 文件下载失败
    /// </summary>
    public Action<string> ItemError;

    /// <summary>
    /// 全部要更新的资源下载完成回调
    /// </summary>
    public Action LoadOver;

    /// <summary>
    /// 需要下载的资源的定位器和资源大小字典
    /// </summary>
    Dictionary<IResourceLocator, long> m_DownLoadDic = new Dictionary<IResourceLocator, long>();

    /// <summary>
    /// 需要下载的资源的定位器列表
    /// </summary>
    List<IResourceLocator> m_DownLoadList = new List<IResourceLocator>();

    /// <summary>
    /// 已经下载过的资源的定位器
    /// </summary>
    Dictionary<IResourceLocator, long> m_AlreadyDownDic = new Dictionary<IResourceLocator, long>();

    /// <summary>
    /// 已经下载过的资源
    /// </summary>
    public List<IResourceLocator> m_AlreadyDownList = new List<IResourceLocator>();

    /// <summary>
    /// 变化的资源的目录
    /// </summary>
    public List<string> Catalogs = new List<string>();
    /// <summary>
    /// 当前正在下载资源的进度
    /// </summary>
    float nowPercent;

    /// <summary>
    /// 当前正在下载资源的定位器
    /// </summary>
    IResourceLocator nowLocator;

    WaitForSeconds oneSecond = new WaitForSeconds(1.0f);

    /// <summary>
    /// mono脚本
    /// </summary>
    protected MonoBehaviour m_Startmono;

    /// <summary>
    /// 初始化资源管理器
    /// </summary>
    /// <param name="mono"></param>
    public void Init(MonoBehaviour mono)
    {
        m_Startmono = mono;
    }

    internal float GetProgress()
    {
        return GetLoadSize() / LoadSumSize;
    }

    internal float GetLoadSize()
    {
        float alreadySize = 0;
        for (int i = 0; i < m_AlreadyDownList.Count; ++i)
        {
            alreadySize += m_DownLoadDic[m_AlreadyDownList[i]];
        }
        float curAlreadySize = 0;
        if (nowLocator != null)
        {
            curAlreadySize = nowPercent * m_DownLoadDic[nowLocator];
        }
        return alreadySize + curAlreadySize;
    }

    internal void CheckVersion(Action<bool> hotCallBack = null)
    {
        hotCallBack(true);
    }

    internal void StartDownLoad(Action startOnFinish)
    {
        m_Startmono.StartCoroutine(StartDownLoadAB(startOnFinish));
    }

    /// <summary>
    /// 下载更新资源
    /// </summary>
    /// <returns></returns>
    internal IEnumerator StartDownLoadAB(Action startOnFinish)
    {
        AsyncOperationHandle initHandle = Addressables.InitializeAsync();
        yield return initHandle;
        AsyncOperationHandle<List<string>> handler = Addressables.CheckForCatalogUpdates(false);
        yield return handler;
        if (handler.Status != AsyncOperationStatus.Succeeded)
        {
            //TODO:更新失败(弹窗重新更新，或者强制下载安装包)
            ServerInfoError();
            yield break;
        }
        List<string> catalogs = (List<string>)handler.Result;
        Debug.Log($"need update catalog:{catalogs.Count}");
        foreach (var catalog in catalogs)
        {
            Debug.Log(catalog);
        }

        if (catalogs.Count > 0)
        {
            AsyncOperationHandle updateHandle = Addressables.UpdateCatalogs(catalogs, false);
            yield return updateHandle;
            List<IResourceLocator> locators = (List<IResourceLocator>)updateHandle.Result;
            foreach (var locator in locators)
            {
                foreach (var key in locator.Keys)
                {
                    Debug.Log($"update {key}");

                    yield return CheckDownload(key);
                }
            }
        }
        Addressables.Release(handler);
        yield return oneSecond;
        startOnFinish();
    }

    private IEnumerator CheckDownload(object key)
    {
        Debug.Log("CheckDownload:" + key.ToString());
        AsyncOperationHandle sizeHandle = Addressables.GetDownloadSizeAsync(key);
        yield return sizeHandle;
        long totalDownloadSize = (long)sizeHandle.Result;
        Debug.Log("CheckDownloadSize:" + totalDownloadSize);
        if (totalDownloadSize > 0)
        {
            var downloadHandle = Addressables.DownloadDependenciesAsync(key);
            while (!downloadHandle.IsDone)
            {
                float percent = downloadHandle.PercentComplete;
                Debug.Log($"{key.ToString()}已经下载：{(int)(totalDownloadSize * percent)}/{totalDownloadSize}");
                yield return new WaitForEndOfFrame();
            }
            Debug.Log($"{key.ToString()}下载完成:{(int)totalDownloadSize}");
        }
    }
}
