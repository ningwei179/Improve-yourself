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
    /// �����ش�С
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
    /// ��Ҫ���ص���Դ�Ķ�λ���б�
    /// </summary>
    List<IResourceLocator> m_DownLoadList = new List<IResourceLocator>();

    /// <summary>
    /// �Ѿ����ع�����Դ�Ķ�λ��
    /// </summary>
    Dictionary<IResourceLocator, long> m_AlreadyDownDic = new Dictionary<IResourceLocator, long>();

    /// <summary>
    /// �Ѿ����ع�����Դ
    /// </summary>
    public List<IResourceLocator> m_AlreadyDownList = new List<IResourceLocator>();

    /// <summary>
    /// ��ǰ����������Դ�Ľ���
    /// </summary>
    float nowPercent;

    /// <summary>
    /// ��ǰ����������Դ�Ķ�λ��
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
            //TODO:����ʧ��(�������¸��£�����ǿ�����ذ�װ��)
            ServerInfoError();
            yield break;
        }
        hotCallBack( handler.Result.Count > 0);
        //����Դ��Ҫ���£��������ܹ�Ҫ���µ���Դ��С
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
            //������Դ�ɹ�
            if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                m_AlreadyDownList.Add(locator);
                m_AlreadyDownDic.Add(locator,0);
                Addressables.Release(downloadHandle);
            }
        }

        //��Ҫ�������ص���Դ�б�
        List<IResourceLocator> m_DownLoadAgainList = new List<IResourceLocator>();
        for (int i = 0; i < m_DownLoadList.Count; ++i) {
            //��Ҫ���ص���Դ���Ѿ����ع�����Դ�б������Ҳ������ͼ������������б�
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
                Debug.LogError("��Դ�ظ�����4�ζ�ʧ�ܣ�������Դ" + allName);
                ItemError?.Invoke(allName);
            }
            else
            {
                m_TryDownCount++;
                //�Զ���������ʧ�ܵ��ļ�
                StartDownLoadAB(callBack, m_DownLoadAgainList);
            }
        }
    }
}
