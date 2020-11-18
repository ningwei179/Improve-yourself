using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
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
    /// 当前正在下载资源的进度
    /// </summary>
    float nowPercent;

    /// <summary>
    /// 当前正在下载资源的定位器
    /// </summary>
    IResourceLocator nowLocator;

    WaitForSeconds oneSecond = new WaitForSeconds(1.0f);

    public void Init()
    {
        GameStart.Instance.StartCoroutine(Initialize());
    }

    IEnumerator Initialize() {
        AsyncOperationHandle initHandle = Addressables.InitializeAsync();
        yield return initHandle;
    }

    internal float GetProgress()
    {
        return GetLoadSize() / LoadSumSize;
    }

    internal float GetLoadSize()
    {
        float alreadySize = 0;
        for (int i = 0; i < m_AlreadyDownList.Count; ++i) {
            alreadySize +=m_DownLoadDic[m_AlreadyDownList[i]];
        }
        float curAlreadySize = 0;
        if (nowLocator != null)
        {
            curAlreadySize = nowPercent * m_DownLoadDic[nowLocator];
        }
        return alreadySize + curAlreadySize;
    }

    internal IEnumerator CheckVersion(Action<bool> hotCallBack)
    {
        AsyncOperationHandle<List<string>> handler = Addressables.CheckForCatalogUpdates(false);
        yield return handler;
        if (handler.Status != AsyncOperationStatus.Succeeded)
        {
            //TODO:更新失败(弹窗重新更新，或者强制下载安装包)
            ServerInfoError();
            yield break;
        }
        hotCallBack( handler.Result.Count > 0);
        //有资源需要更新，计算下总共要更新的资源大小
        if (handler.Result.Count > 0) { 
            AsyncOperationHandle<List<IResourceLocator>> updateHandle = Addressables.UpdateCatalogs(handler.Result, false);
            yield return updateHandle;
            
            m_DownLoadList = updateHandle.Result;

            foreach (var locator in m_DownLoadList)
            {
                AsyncOperationHandle sizeHandle = Addressables.GetDownloadSizeAsync(locator.Keys);
                yield return sizeHandle;
                LoadSumSize +=(long)sizeHandle.Result;
                m_DownLoadDic.Add(locator, (long)sizeHandle.Result);
            }
        }
        Addressables.Release(handler);
    }

    internal IEnumerator StartDownLoadAB(Action callBack, List<IResourceLocator> allDownLoadList = null)
    {
        if (allDownLoadList == null)
        {
            allDownLoadList = m_DownLoadList;
        }

        foreach (var locator in allDownLoadList)
        {
            AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(locator.Keys);
            while (!downloadHandle.IsDone)
            {
                nowPercent = downloadHandle.PercentComplete;
                nowLocator = locator;
                yield return null;
            }
            yield return downloadHandle;
            //下载资源成功
            if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                m_AlreadyDownList.Add(locator);
                m_AlreadyDownDic.Add(locator,0);
                Addressables.Release(downloadHandle);
            }
        }

        //需要重新下载的资源列表
        List<IResourceLocator> m_DownLoadAgainList = new List<IResourceLocator>();
        for (int i = 0; i < m_DownLoadList.Count; ++i) {
            //需要下载的资源在已经下载过的资源列表里面找不到，就加入重新下载列表
            if (!m_AlreadyDownDic.ContainsKey(m_DownLoadList[i]))
                m_DownLoadAgainList.Add(m_DownLoadList[i]);
        }

        if (m_DownLoadAgainList.Count <= 0)
        {
            if (callBack != null)
            {
                yield return oneSecond;
                StartDownload = false;
                callBack();
            }
            if (LoadOver != null)
            {
                LoadOver();
            }
        }
        else {
            if (m_TryDownCount >= DOWNLOADCOUNT)
            {
                string allName = "";
                StartDownload = false;
                foreach (IResourceLocator locator in m_DownLoadAgainList)
                {
                    allName += locator.LocatorId + ";";
                }
                Debug.LogError("资源重复下载4次都失败，请检查资源" + allName);
                ItemError?.Invoke(allName);
            }
            else
            {
                m_TryDownCount++;
                //自动重新下载失败的文件
                StartDownLoadAB(callBack, m_DownLoadAgainList);
            }
        }
    }
}
