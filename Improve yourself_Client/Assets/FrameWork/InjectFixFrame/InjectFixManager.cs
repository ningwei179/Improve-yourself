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
using Debug = UnityEngine.Debug;
namespace Improve
{
    public class InjectFixManager : Singleton<InjectFixManager>
    {
        WaitForEndOfFrame oneFrame = new WaitForEndOfFrame();

        string patchPath = "Assets/GameData/Data/InjectHotFix/Assembly-CSharp.patch.bytes";

        internal IEnumerator LoadHotFixPatch()
        {
            bool loadComplete = false;
            if (FrameConstr.UseAssetAddress == AssetAddress.Addressable)
            {
                AddressableManager.Instance.AsyncLoadResource<TextAsset>(patchPath, (TextAsset text) =>
                {
                    try
                    {
                        if (text != null)
                        {
                            Debug.Log("加载InjectFix热补丁文件 ...");
                            var sw = Stopwatch.StartNew();
                            PatchManager.Load(new MemoryStream(text.bytes));
                            Debug.Log("加载InjectFix热补丁文件成功, 用时: " + sw.ElapsedMilliseconds + " ms");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log("加载InjectFix热补丁文件失败,补丁不匹配" + e);
                    }
                    loadComplete = true;
                });
            }
            else
            {
                ResourceManager.Instance.AsyncLoadResource(patchPath, (string resourcePath, UnityEngine.Object obj, object[] paramArr) =>
                {
                    try
                    {
                        if (obj != null)
                        {
                            TextAsset text = obj as TextAsset;
                            Debug.Log("加载InjectFix热补丁文件 ...");
                            var sw = Stopwatch.StartNew();
                            PatchManager.Load(new MemoryStream(text.bytes));
                            Debug.Log("加载InjectFix热补丁文件成功, 用时: " + sw.ElapsedMilliseconds + " ms");

                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log("加载InjectFix热补丁文件失败,补丁不匹配" + e);
                    }
                    loadComplete = true;
                }, LoadResPriority.RES_MIDDLE, false);
            }

            while (!loadComplete)
            {
                yield return oneFrame;
            }
        }
    }
}