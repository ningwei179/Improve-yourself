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
    public float LoadSumSize;

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
    /// ��Ҫ���ص���Դ�Ķ�λ������Դ��С�ֵ�
    /// </summary>
    Dictionary<IResourceLocator, long> m_DownLoadDic = new Dictionary<IResourceLocator, long>();

    /// <summary>
    /// ��Ҫ���ص���Դ�ļ���
    /// </summary>
    List<object> m_DownLoadList = new List<object>();

    /// <summary>
    /// �Ѿ����ع�����Դ�Ķ�λ��
    /// </summary>
    List<object> m_AlreadyDownDic = new List<object>();

    /// <summary>
    /// �Ѿ����ع�����Դ
    /// </summary>
    public List<object> m_AlreadyDownList = new List<object>();

    /// <summary>
    /// �仯����Դ��Ŀ¼
    /// </summary>
    public List<string> Catalogs = new List<string>();
    /// <summary>
    /// ��ǰ����������Դ�Ľ���
    /// </summary>
    float nowPercent;

    /// <summary>
    /// ��ǰ����������Դ�Ķ�λ��
    /// </summary>
    IResourceLocator nowLocator;

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
    /// ������Ŀ¼
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
            //TODO:����ʧ��(�������¸��£�����ǿ�����ذ�װ��)
            ServerInfoError();
            yield break;
        }
        List<string> catalogs = (List<string>)handler.Result;
        Debug.Log($"��Ҫ���µ���Դ��Ŀ¼����:{catalogs.Count}");

        if (catalogs.Count > 0)
        {
            AsyncOperationHandle updateHandle = Addressables.UpdateCatalogs(catalogs, false);
            yield return updateHandle;
            List<IResourceLocator> locators = (List<IResourceLocator>)updateHandle.Result;
            //���Ŀ¼�������������Ŀ���б�Addressable�������Դ�ĵ�ַ
            //ͬһ����Դ��������ַ��һ�������õ�һ��������hashֵ
            foreach (var locator in locators)
            {
                Debug.Log($"Ҫ���µ���ԴĿ¼�Ķ�λ��ID : {locator.LocatorId}");

                AsyncOperationHandle allSizeHandle = Addressables.GetDownloadSizeAsync(locator.Keys);
                yield return allSizeHandle;

                LoadSumSize += (long)allSizeHandle.Result;     //��Ҫ���ص���Դ���ܴ�С

                if ((long)allSizeHandle.Result > 0)
                {
                    Debug.Log($"�����ԴĿ¼Ҫ���µ���Դ�Ĵ�С : {(long)allSizeHandle.Result}");

                    //�������ԴĿ¼��keys�����ÿ����Դ������������key,һ�������õ����ƣ�һ����hashֵ
                    foreach (var key in locator.Keys)
                    {
                        AsyncOperationHandle sizeHandle = Addressables.GetDownloadSizeAsync(key);
                        yield return sizeHandle;
                        long totalDownloadSize = (long)sizeHandle.Result;
                        Debug.Log($"��Դ:{key}==���ڵ�AB���Ĵ�С" + totalDownloadSize);

                        //����ÿӣ�һ��AB�������һ����Դ���������������Դ��key��ȡ���ش�СҲ�᷵�ذ��Ĵ�С
                        //�����ᵼ�¼�¼�˺ܶ����õ���Ҫ���ص���Դ��key�������÷ֿ���AB�����������⣬
                        //������Դ���õ����ƺ�hash��û�취�ˣ�����ָ��ͬһ����Դ
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
    /// ���ظ�����Դ
    /// </summary>
    /// <returns></returns>
    internal IEnumerator StartDownLoadAB(Action startOnFinish)
    {
        for (int i = 0; i < m_DownLoadList.Count; ++i)
        {
            AsyncOperationHandle sizeHandle = Addressables.GetDownloadSizeAsync(m_DownLoadList[i]);
            yield return sizeHandle;
            long totalDownloadSize = (long)sizeHandle.Result;
            Debug.Log($"��Դ:{m_DownLoadList[i]}==���ڵ�AB��Ҫ���µĴ�С{totalDownloadSize}");
            if (totalDownloadSize > 0)
            {
                var downloadHandle = Addressables.DownloadDependenciesAsync(m_DownLoadList[i]);
                while (!downloadHandle.IsDone)
                {
                    float percent = downloadHandle.PercentComplete;
                    Debug.Log($"��Դ:{m_DownLoadList[i].ToString()}���ڵ�AB���Ѿ����أ�{(int)(totalDownloadSize * percent)}/{totalDownloadSize}");
                    yield return new WaitForEndOfFrame();
                }
                m_AlreadyDownList.Add(m_DownLoadList[i]);
                Debug.Log($"��Դ:{m_DownLoadList[i].ToString()}���ڵ�AB���������:{(int)totalDownloadSize}");
            }
            else
            {
                m_AlreadyDownList.Add(m_DownLoadList[i]);
            }
        }
        startOnFinish();
    }

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
                Debug.Log($"{key.ToString()}�Ѿ����أ�{(int)(totalDownloadSize * percent)}/{totalDownloadSize}");
                yield return new WaitForEndOfFrame();
            }
            Debug.Log($"{key.ToString()}�������:{(int)totalDownloadSize}");
        }
    }
}
