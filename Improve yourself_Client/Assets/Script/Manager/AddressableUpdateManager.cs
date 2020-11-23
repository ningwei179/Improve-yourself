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
    private long m_LoadSumSize;

    public long LoadSumSize {
        get {
            return m_LoadSumSize;
        }
        set {
            m_LoadSumSize = value;
        }
    }

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
    /// 需要下载的资源的集合
    /// </summary>
    List<object> m_DownLoadList = new List<object>();

    /// <summary>
    /// 已经下载过的资源的key和它的大小
    /// </summary>
    Dictionary<object, long> m_AlreadyDownDic = new Dictionary<object, long>();

    /// <summary>
    /// 当前正在下载资源的大小
    /// </summary>
    float nowLoadSize =0;

    /// <summary>
    /// 已经下载资源的大小
    /// </summary>
    long alreadyLoadSize = 0;

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

    /// <summary>
    /// 下载的总进度
    /// </summary>
    /// <returns></returns>
    internal float GetProgress()
    {
        return GetLoadSize() / m_LoadSumSize;
    }

    /// <summary>
    /// 已经下载的大小,单位是字节
    /// </summary>
    /// <returns></returns>
    internal float GetLoadSize()
    {
        return (alreadyLoadSize + nowLoadSize);
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

        //这里的检查更新的目录应该有其他的扩展用法，但是我们这只用来把他当AB的Manifest用
        //所以我们其实只有一个目录，不考虑多个目录的用法
        AsyncOperationHandle<List<string>> catalogHandler = Addressables.CheckForCatalogUpdates(false);
        yield return catalogHandler;
        if (catalogHandler.Status != AsyncOperationStatus.Succeeded)
        {
            ServerInfoError();
            Addressables.Release(catalogHandler);
            yield break;
        }
        List<string> catalogs = (List<string>)catalogHandler.Result;

        List<IResourceLocator> locators;
        AsyncOperationHandle CatalogHandle;
        if (catalogs.Count > 0)
        {
            Debug.Log($"需要更新资源的目录");
            //下载新的目录
            //这个目录里面包含整个项目所有被Addressable管理的资源的地址
            //同一份资源有俩个地址，一个是设置的一个是它的hash值
            CatalogHandle = Addressables.UpdateCatalogs(catalogs, false);
        }
        else
        {
            //当成AB的manifest去获取一次资源目录
            Debug.Log($"不需要更新资源的目录");
            catalogs.Add("AddressablesMainContentCatalog");
            CatalogHandle = Addressables.UpdateCatalogs(catalogs, false);
        }
        yield return CatalogHandle;
        if (CatalogHandle.Status != AsyncOperationStatus.Succeeded)
        {
            ServerInfoError();
            yield break;
        }
        locators = (List<IResourceLocator>)CatalogHandle.Result;

        //检查资源更新
        m_Startmono.StartCoroutine(ComputeDownload(locators, hotCallBack));

        Addressables.Release(catalogHandler);
    }

    /// <summary>
    /// 计算下载的资源
    /// </summary>
    IEnumerator ComputeDownload(List<IResourceLocator> locators, Action<bool> hotCallBack = null)
    {
        m_DownLoadList.Clear();
        foreach (var locator in locators)
        {
            //计算每个目录的所有的要下载的资源的大小
            AsyncOperationHandle allSizeHandle = Addressables.GetDownloadSizeAsync(locator.Keys);
            yield return allSizeHandle;
            m_LoadSumSize += (long)allSizeHandle.Result;     //需要下载的资源的总大小
            
            if ((long)allSizeHandle.Result > 0)
            {
                Debug.Log($"这个资源目录要更新的资源的大小 : {(long)allSizeHandle.Result}");

                //这里的资源目录的keys里面给每个资源都保存了俩个key,一个是设置的名称，一个是hash值
                foreach (var key in locator.Keys)
                {
                    AsyncOperationHandle sizeHandle = Addressables.GetDownloadSizeAsync(key);
                    yield return sizeHandle;
                    long totalDownloadSize = (long)sizeHandle.Result;
                    //Debug.Log($"资源:{key}==所在的AB包的大小" + totalDownloadSize);

                    //这里好坑，一个AB包里更新一个资源，这个包里其他资源的key获取下载大小也会返回包的大小
                    //这样会导致记录了很多无用的需要下载的资源的key，可以用分开打AB来解决这个问题，
                    //但是资源设置的名称和hash就没办法了，他们指向同一份资源
                    if (totalDownloadSize > 0)
                    {
                        m_DownLoadList.Add(key);
                    }
                    Addressables.Release(sizeHandle);
                }
            }

            Addressables.Release(allSizeHandle);
        }

        hotCallBack(m_DownLoadList.Count>0);
    }

    /// <summary>
    /// 下载更新资源
    /// </summary>
    /// <returns></returns>
    internal IEnumerator StartDownLoadAB(Action startOnFinish, List<object> allDownLoad = null)
    {
        StartDownload = true;
        if (allDownLoad == null)
        {
            allDownLoad = m_DownLoadList;
        }
        for (int i = 0; i < allDownLoad.Count; ++i)
        {
            //获取这个m_DownLoadList[i]资源所在的AB包大小，可能多个m_DownLoadList[i]在同一个资源包里面
            //当这个m_DownLoadList[i]资源的AB包下载完成后，这个AB包里其他的m_DownLoadList[i]资源获取
            //GetDownloadSizeAsync的大小会是0,用这个0来判断是否开始下载m_DownLoadList[i]资源的AB包
            AsyncOperationHandle sizeHandle = Addressables.GetDownloadSizeAsync(m_DownLoadList[i]);
            yield return sizeHandle;
            long totalDownloadSize = (long)sizeHandle.Result;
            if (totalDownloadSize > 0)
            {
                Debug.Log($"资源:{m_DownLoadList[i]}==所在的AB包要更新的大小{totalDownloadSize}");

                //先卸载这个资源以前的包
                //一定要卸载，不然会越更新手机中的资源越多
                Addressables.ClearDependencyCacheAsync(m_DownLoadList[i]);

                //再重新下载这个资源包
                var downloadHandle = Addressables.DownloadDependenciesAsync(m_DownLoadList[i]);
                while (!downloadHandle.IsDone)
                {
                    nowLoadSize = downloadHandle.PercentComplete * totalDownloadSize;    //正在下载资源的大小
                    yield return new WaitForEndOfFrame();
                }
                Addressables.Release(downloadHandle);
                m_AlreadyDownDic.Add(m_DownLoadList[i], totalDownloadSize);

                nowLoadSize = 0;
                alreadyLoadSize += totalDownloadSize;       //已经下载资源的大小

                Debug.Log($"资源:{m_DownLoadList[i].ToString()}所在的AB包下载完成:{(int)totalDownloadSize}");
            }
            else
            {
                m_AlreadyDownDic.Add(m_DownLoadList[i], totalDownloadSize);
            }
            Addressables.Release(sizeHandle);
        }

        //下载失败后要重新下载的列表
        List<object> downLoadAgainList = new List<object>();
        for (int i = 0; i < m_DownLoadList.Count; ++i)
        {
            if (!m_AlreadyDownDic.ContainsKey(m_DownLoadList[i]))
            {
                downLoadAgainList.Add(m_DownLoadList[i]);
            }
        }

        //全部文件都是正确的
        if (downLoadAgainList.Count <= 0)
        {
            if (startOnFinish != null)
            {
                yield return oneSecond;
                StartDownload = false;
                startOnFinish();
            }
            if (LoadOver != null)
            {
                LoadOver();
            }
        }
        else
        {
            if (m_TryDownCount >= DOWNLOADCOUNT)
            {
                string allName = "";
                StartDownload = false;
                foreach (var item in downLoadAgainList)
                {
                    allName += item + ";";
                }
                Debug.LogError("资源重复下载多次次都失败，请检查资源" + allName);
                ItemError?.Invoke(allName);
            }
            else
            {
                m_TryDownCount++;
                //自动重新下载校验失败的文件
                m_Startmono.StartCoroutine(StartDownLoadAB(startOnFinish, downLoadAgainList));
            }
        }
    }

    #region 网上找的下载的代码，实际用的时候应该考虑断网资源下载不全的情况
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

    //private IEnumerator CheckDownload(object key)
    //{
    //    Debug.Log("CheckDownload:" + key.ToString());
    //    AsyncOperationHandle sizeHandle = Addressables.GetDownloadSizeAsync(key);
    //    yield return sizeHandle;
    //    long totalDownloadSize = (long)sizeHandle.Result;
    //    Debug.Log("CheckDownloadSize:" + totalDownloadSize);
    //    if (totalDownloadSize > 0)
    //    {
    //        var downloadHandle = Addressables.DownloadDependenciesAsync(key);
    //        while (!downloadHandle.IsDone)
    //        {
    //            float percent = downloadHandle.PercentComplete;
    //            Debug.Log($"{key.ToString()}已经下载：{(int)(totalDownloadSize * percent)}/{totalDownloadSize}");
    //            yield return new WaitForEndOfFrame();
    //        }
    //        Debug.Log($"{key.ToString()}下载完成:{(int)totalDownloadSize}");
    //    }
    //}
    #endregion
}
