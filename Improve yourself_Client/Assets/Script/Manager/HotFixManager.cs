/********************************************************************
	文件:	HotFixManager.cs
	作者:	NingWei
	日期:	2020/11/18 10:39
	功能:	加载InjectFix热更资源并且执行它
*********************************************************************/
using IFix.Core;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Debug = UnityEngine.Debug;

public class HotFixManager : Singleton<HotFixManager>
{
    string patchPath = "Assets/GameData/Data/HotFix/Assembly-CSharp.patch.bytes";

    /// <summary>
    /// mono脚本
    /// </summary>
    protected MonoBehaviour m_Startmono;

    internal void Init(MonoBehaviour mono) {
        m_Startmono = mono;
        m_Startmono.StartCoroutine(LoadHotFixPatch());
    }

    internal IEnumerator LoadHotFixPatch()
    {
        Debug.Log("开始加载C#热补丁文件");
        AsyncOperationHandle<long> sizeHandle = Addressables.GetDownloadSizeAsync(patchPath);
        yield return sizeHandle;
        if (sizeHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("热补丁文件大小是：" + sizeHandle.Result);
        }
        Addressables.Release(sizeHandle);

        AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(patchPath);
        if (!handle.IsValid())
        {
            Debug.LogError("热更文件不存在");
            yield break;
        }
        yield return handle;
        if (handle.IsDone)
        {
            Debug.Log("loading Assembly-CSharp.patch ...");
            var sw = Stopwatch.StartNew();
            PatchManager.Load(new MemoryStream(handle.Result.bytes));
            Debug.Log("patch Assembly-CSharp.patch, using " + sw.ElapsedMilliseconds + " ms");
        }
    }
}
