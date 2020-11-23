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

public class InjectFixManager : Singleton<InjectFixManager>
{
    WaitForEndOfFrame oneFrame = new WaitForEndOfFrame();

    string patchPath = "Assets/GameData/Data/InjectHotFix/Assembly-CSharp.patch.bytes";

    internal IEnumerator LoadHotFixPatch()
    {
        bool loadComplete = false;
        AddressableManager.Instance.AsyncLoadResource<TextAsset>(patchPath, (TextAsset text) =>
        {
            try
            {
                if (text != null)
                {
                    Debug.Log("加载C#热补丁文件 ...");
                    var sw = Stopwatch.StartNew();
                    PatchManager.Load(new MemoryStream(text.bytes));
                    Debug.Log("加载C#热补丁文件成功, 用时: " + sw.ElapsedMilliseconds + " ms");
                }
            }
            catch (Exception e)
            {
                Debug.Log("加载C#热补丁文件失败,补丁不匹配");
            }
            loadComplete = true;
        });
        while (!loadComplete) {
            yield return oneFrame;
        }
    }
}
