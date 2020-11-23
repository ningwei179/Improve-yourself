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
    /// ��ʼ����
    /// </summary>
    public bool StartDownload = false;

    /// <summary>
    /// �����������ش���
    /// </summary>
    private int m_TryDownCount = 0;

    /// <summary>
    /// ������������������
    /// </summary>
    private const int DOWNLOADCOUNT = 4;

    /// <summary>
    /// ��ʼ��ѹ
    /// </summary>
    public bool StartUnPack;

    /// <summary>
    /// ��ǰ�汾
    /// </summary>
    public string CurVersion;

    /// <summary>
    /// ��ǰ����
    /// </summary>
    public string m_CurPackName;

    /// <summary>
    /// �����ش�С����λ���ֽ�
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
    /// �����б�û�л�ȡ��
    /// </summary>
    public Action ServerInfoError;

    /// <summary>
    /// �ļ�����ʧ��
    /// </summary>
    public Action<string> ItemError;

    /// <summary>
    /// ȫ��Ҫ���µ���Դ������ɻص�
    /// </summary>
    public Action LoadOver;

    /// <summary>
    /// ��Ҫ���ص���Դ�ļ���
    /// </summary>
    List<object> m_DownLoadList = new List<object>();

    /// <summary>
    /// �Ѿ����ع�����Դ��key�����Ĵ�С
    /// </summary>
    Dictionary<object, long> m_AlreadyDownDic = new Dictionary<object, long>();

    /// <summary>
    /// ��ǰ����������Դ�Ĵ�С
    /// </summary>
    float nowLoadSize =0;

    /// <summary>
    /// �Ѿ�������Դ�Ĵ�С
    /// </summary>
    long alreadyLoadSize = 0;

    WaitForSeconds oneSecond = new WaitForSeconds(1.0f);

    /// <summary>
    /// mono�ű�
    /// </summary>
    protected MonoBehaviour m_Startmono;

    /// <summary>
    /// ��ʼ����Դ������
    /// </summary>
    /// <param name="mono"></param>
    public void Init(MonoBehaviour mono)
    {
        m_Startmono = mono;
    }

    /// <summary>
    /// ���ص��ܽ���
    /// </summary>
    /// <returns></returns>
    internal float GetProgress()
    {
        return GetLoadSize() / m_LoadSumSize;
    }

    /// <summary>
    /// �Ѿ����صĴ�С,��λ���ֽ�
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
    /// ������Ŀ¼
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckCatalogUpdates(Action<bool> hotCallBack = null)
    {
        AsyncOperationHandle initHandle = Addressables.InitializeAsync();
        yield return initHandle;

        //����ļ����µ�Ŀ¼Ӧ������������չ�÷�������������ֻ����������AB��Manifest��
        //����������ʵֻ��һ��Ŀ¼�������Ƕ��Ŀ¼���÷�
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
            Debug.Log($"��Ҫ������Դ��Ŀ¼");
            //�����µ�Ŀ¼
            //���Ŀ¼�������������Ŀ���б�Addressable�������Դ�ĵ�ַ
            //ͬһ����Դ��������ַ��һ�������õ�һ��������hashֵ
            CatalogHandle = Addressables.UpdateCatalogs(catalogs, false);
        }
        else
        {
            //����AB��manifestȥ��ȡһ����ԴĿ¼
            Debug.Log($"����Ҫ������Դ��Ŀ¼");
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

        //�����Դ����
        m_Startmono.StartCoroutine(ComputeDownload(locators, hotCallBack));

        Addressables.Release(catalogHandler);
    }

    /// <summary>
    /// �������ص���Դ
    /// </summary>
    IEnumerator ComputeDownload(List<IResourceLocator> locators, Action<bool> hotCallBack = null)
    {
        m_DownLoadList.Clear();
        foreach (var locator in locators)
        {
            //����ÿ��Ŀ¼�����е�Ҫ���ص���Դ�Ĵ�С
            AsyncOperationHandle allSizeHandle = Addressables.GetDownloadSizeAsync(locator.Keys);
            yield return allSizeHandle;
            m_LoadSumSize += (long)allSizeHandle.Result;     //��Ҫ���ص���Դ���ܴ�С
            
            if ((long)allSizeHandle.Result > 0)
            {
                Debug.Log($"�����ԴĿ¼Ҫ���µ���Դ�Ĵ�С : {(long)allSizeHandle.Result}");

                //�������ԴĿ¼��keys�����ÿ����Դ������������key,һ�������õ����ƣ�һ����hashֵ
                foreach (var key in locator.Keys)
                {
                    AsyncOperationHandle sizeHandle = Addressables.GetDownloadSizeAsync(key);
                    yield return sizeHandle;
                    long totalDownloadSize = (long)sizeHandle.Result;
                    //Debug.Log($"��Դ:{key}==���ڵ�AB���Ĵ�С" + totalDownloadSize);

                    //����ÿӣ�һ��AB�������һ����Դ���������������Դ��key��ȡ���ش�СҲ�᷵�ذ��Ĵ�С
                    //�����ᵼ�¼�¼�˺ܶ����õ���Ҫ���ص���Դ��key�������÷ֿ���AB�����������⣬
                    //������Դ���õ����ƺ�hash��û�취�ˣ�����ָ��ͬһ����Դ
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
    /// ���ظ�����Դ
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
            //��ȡ���m_DownLoadList[i]��Դ���ڵ�AB����С�����ܶ��m_DownLoadList[i]��ͬһ����Դ������
            //�����m_DownLoadList[i]��Դ��AB��������ɺ����AB����������m_DownLoadList[i]��Դ��ȡ
            //GetDownloadSizeAsync�Ĵ�С����0,�����0���ж��Ƿ�ʼ����m_DownLoadList[i]��Դ��AB��
            AsyncOperationHandle sizeHandle = Addressables.GetDownloadSizeAsync(m_DownLoadList[i]);
            yield return sizeHandle;
            long totalDownloadSize = (long)sizeHandle.Result;
            if (totalDownloadSize > 0)
            {
                Debug.Log($"��Դ:{m_DownLoadList[i]}==���ڵ�AB��Ҫ���µĴ�С{totalDownloadSize}");

                //��ж�������Դ��ǰ�İ�
                //һ��Ҫж�أ���Ȼ��Խ�����ֻ��е���ԴԽ��
                Addressables.ClearDependencyCacheAsync(m_DownLoadList[i]);

                //���������������Դ��
                var downloadHandle = Addressables.DownloadDependenciesAsync(m_DownLoadList[i]);
                while (!downloadHandle.IsDone)
                {
                    nowLoadSize = downloadHandle.PercentComplete * totalDownloadSize;    //����������Դ�Ĵ�С
                    yield return new WaitForEndOfFrame();
                }
                Addressables.Release(downloadHandle);
                m_AlreadyDownDic.Add(m_DownLoadList[i], totalDownloadSize);

                nowLoadSize = 0;
                alreadyLoadSize += totalDownloadSize;       //�Ѿ�������Դ�Ĵ�С

                Debug.Log($"��Դ:{m_DownLoadList[i].ToString()}���ڵ�AB���������:{(int)totalDownloadSize}");
            }
            else
            {
                m_AlreadyDownDic.Add(m_DownLoadList[i], totalDownloadSize);
            }
            Addressables.Release(sizeHandle);
        }

        //����ʧ�ܺ�Ҫ�������ص��б�
        List<object> downLoadAgainList = new List<object>();
        for (int i = 0; i < m_DownLoadList.Count; ++i)
        {
            if (!m_AlreadyDownDic.ContainsKey(m_DownLoadList[i]))
            {
                downLoadAgainList.Add(m_DownLoadList[i]);
            }
        }

        //ȫ���ļ�������ȷ��
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
                Debug.LogError("��Դ�ظ����ض�δζ�ʧ�ܣ�������Դ" + allName);
                ItemError?.Invoke(allName);
            }
            else
            {
                m_TryDownCount++;
                //�Զ���������У��ʧ�ܵ��ļ�
                m_Startmono.StartCoroutine(StartDownLoadAB(startOnFinish, downLoadAgainList));
            }
        }
    }

    #region �����ҵ����صĴ��룬ʵ���õ�ʱ��Ӧ�ÿ��Ƕ�����Դ���ز�ȫ�����
    /// <summary>
    /// ���ظ�����Դ
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
    //        //TODO:����ʧ��(�������¸��£�����ǿ�����ذ�װ��)
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
    //            Debug.Log($"{key.ToString()}�Ѿ����أ�{(int)(totalDownloadSize * percent)}/{totalDownloadSize}");
    //            yield return new WaitForEndOfFrame();
    //        }
    //        Debug.Log($"{key.ToString()}�������:{(int)totalDownloadSize}");
    //    }
    //}
    #endregion
}
