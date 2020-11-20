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
    /// ���ظ�����Դ
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
            //TODO:����ʧ��(�������¸��£�����ǿ�����ذ�װ��)
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
                Debug.Log($"{key.ToString()}�Ѿ����أ�{(int)(totalDownloadSize * percent)}/{totalDownloadSize}");
                yield return new WaitForEndOfFrame();
            }
            Debug.Log($"{key.ToString()}�������:{(int)totalDownloadSize}");
        }
    }
}
