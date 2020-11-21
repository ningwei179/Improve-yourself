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
    /// 总下载大小，单位是字节
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
    /// 需要下载的资源的集合
    /// </summary>
    List<object> m_DownLoadList = new List<object>();

    /// <summary>
    /// 已经下载过的资源的定位器
    /// </summary>
    List<object> m_AlreadyDownDic = new List<object>();

    /// <summary>
    /// 已经下载过的资源
    /// </summary>
    public List<object> m_AlreadyDownList = new List<object>();

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
        //for (int i = 0; i < m_AlreadyDownList.Count; ++i)
        //{
        //    alreadySize += m_DownLoadDic[m_AlreadyDownList[i]];
        //}
        float curAlreadySize = 0;
        //if (nowLocator != null)
        //{
        //    curAlreadySize = nowPercent * m_DownLoadDic[nowLocator];
        //}
        return alreadySize + curAlreadySize;
    }

    internal void CheckVersion(Action<bool> hotCallBack = null)
    {
        m_Startmono.StartCoroutine(CheckCatalogUpdates(hotCallBack));
    }

    /// <summary>
    /// 检查更新目录
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckCatalogUpdates(Action<bool> hotCallBack = null)
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
        Debug.Log($"需要更新的资源的目录数量:{catalogs.Count}");

        if (catalogs.Count > 0)
        {
            AsyncOperationHandle updateHandle = Addressables.UpdateCatalogs(catalogs, false);
            yield return updateHandle;
            List<IResourceLocator> locators = (List<IResourceLocator>)updateHandle.Result;
            //这个目录里面包含整个项目所有被Addressable管理的资源的地址
            //同一份资源有俩个地址，一个是设置的一个是它的hash值
            foreach (var locator in locators)
            {
                Debug.Log($"要更新的资源目录的定位器ID : {locator.LocatorId}");

                AsyncOperationHandle allSizeHandle = Addressables.GetDownloadSizeAsync(locator.Keys);
                yield return allSizeHandle;

                LoadSumSize += (long)allSizeHandle.Result;     //需要下载的资源的总大小

                if ((long)allSizeHandle.Result > 0)
                {
                    Debug.Log($"这个资源目录要更新的资源的大小 : {(long)allSizeHandle.Result}");

                    //这里的资源目录的keys里面给每个资源都保存了俩个key,一个是设置的名称，一个是hash值
                    foreach (var key in locator.Keys)
                    {
                        AsyncOperationHandle sizeHandle = Addressables.GetDownloadSizeAsync(key);
                        yield return sizeHandle;
                        long totalDownloadSize = (long)sizeHandle.Result;
                        Debug.Log($"资源:{key}==所在的AB包的大小" + totalDownloadSize);

                        //这里好坑，一个AB包里更新一个资源，这个包里其他资源的key获取下载大小也会返回包的大小
                        //这样会导致记录了很多无用的需要下载的资源的key，可以用分开打AB来解决这个问题，
                        //但是资源设置的名称和hash就没办法了，他们指向同一份资源
                        if (totalDownloadSize > 0)
                        {
                            m_DownLoadList.Add(key);
                        }
                    }
                }
            }
            Addressables.Release(updateHandle);
        }
        hotCallBack(catalogs.Count > 0);
        Addressables.Release(handler);
    }

    /// <summary>
    /// 下载更新资源
    /// </summary>
    /// <returns></returns>
    internal IEnumerator StartDownLoadAB(Action startOnFinish)
    {
        for (int i = 0; i < m_DownLoadList.Count; ++i)
        {
            AsyncOperationHandle sizeHandle = Addressables.GetDownloadSizeAsync(m_DownLoadList[i]);
            yield return sizeHandle;
            long totalDownloadSize = (long)sizeHandle.Result;
            Debug.Log($"资源:{m_DownLoadList[i]}==所在的AB包要更新的大小{totalDownloadSize}");
            if (totalDownloadSize > 0)
            {
                var downloadHandle = Addressables.DownloadDependenciesAsync(m_DownLoadList[i]);
                while (!downloadHandle.IsDone)
                {
                    float percent = downloadHandle.PercentComplete;
                    Debug.Log($"资源:{m_DownLoadList[i].ToString()}所在的AB包已经下载：{(int)(totalDownloadSize * percent)}/{totalDownloadSize}");
                    yield return new WaitForEndOfFrame();
                }
                m_AlreadyDownList.Add(m_DownLoadList[i]);
                Debug.Log($"资源:{m_DownLoadList[i].ToString()}所在的AB包下载完成:{(int)totalDownloadSize}");
            }
            else
            {
                m_AlreadyDownList.Add(m_DownLoadList[i]);
            }
        }
        startOnFinish();
    }

    /// <summary>
    /// 下载更新资源
    /// </summary>
    /// <returns></returns>
    //internal IEnumerator StartDownLoadAB(Action startOnFinish)
    //{
    //    AsyncOperationHandle initHandle = Addressables.InitializeAsync();
    //    yield return initHandle;
    //    AsyncOperationHandle<List<string>> handler = Addressables.CheckForCatalogUpdates(false);
    //    yield return handler;
    //    if (handler.Status != AsyncOperationStatus.Succeeded)
    //    {
    //        //TODO:更新失败(弹窗重新更新，或者强制下载安装包)
    //        ServerInfoError();
    //        yield break;
    //    }
    //    List<string> catalogs = (List<string>)handler.Result;
    //    Debug.Log($"need update catalog:{catalogs.Count}");
    //    foreach (var catalog in catalogs)
    //    {
    //        Debug.Log(catalog);
    //    }

    //    if (catalogs.Count > 0)
    //    {
    //        AsyncOperationHandle updateHandle = Addressables.UpdateCatalogs(catalogs, false);
    //        yield return updateHandle;
    //        List<IResourceLocator> locators = (List<IResourceLocator>)updateHandle.Result;
    //        foreach (var locator in locators)
    //        {
    //            foreach (var key in locator.Keys)
    //            {
    //                Debug.Log($"update {key}");

    //                yield return CheckDownload(key);
    //            }
    //        }
    //    }
    //    Addressables.Release(handler);
    //    yield return oneSecond;
    //    startOnFinish();
    //}

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
