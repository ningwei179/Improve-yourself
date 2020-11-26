/****************************************************     文件：ILRuntimeManager.cs 	作者：NingWei     日期：2020/10/13 15:49:48 	功能：ILRuntime热更管理类 *****************************************************/
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Improve
{
    public class ILRuntimeManager : Singleton<ILRuntimeManager>
    {
        private const string DLLPATH = "Assets/GameData/Data/ILRuntimeHotFix/HotFixProject.dll.bytes";
        private const string PDBPATH = "Assets/GameData/Data/ILRuntimeHotFix/HotFixProject.pdb.bytes";
        private AppDomain m_AppDomain;

        public AppDomain ILRunAppDomain
        {
            get { return m_AppDomain; }
        }

        internal IEnumerator LoadHotFixAssembly(GameObject obj, Transform transform)
        {
            MemoryStream dllMs = null;
            AsyncOperationHandle<TextAsset> dllHandle = Addressables.LoadAssetAsync<TextAsset>(DLLPATH);
            yield return dllHandle;
            if (dllHandle.Status == AsyncOperationStatus.Succeeded)
            {
                dllMs = new MemoryStream(dllHandle.Result.bytes);
            }

            MemoryStream pdbMs = null;
            AsyncOperationHandle<TextAsset> pdbHandle = Addressables.LoadAssetAsync<TextAsset>(PDBPATH);
            yield return pdbHandle;
            if (pdbHandle.Status == AsyncOperationStatus.Succeeded)
            {
                pdbMs = new MemoryStream(pdbHandle.Result.bytes);
            }

            if (dllMs != null && pdbMs != null)
            {
                //整个工程只有一个ILRuntime的AppDomain
                m_AppDomain = new AppDomain();

                m_AppDomain.LoadAssembly(dllMs);

                ILRuntimeRegister.Instance.InitializeILRuntime(m_AppDomain);

                OnHotFixLoaded(obj, transform);
            }
            yield return null;
        }

        void OnHotFixLoaded(GameObject obj, Transform transform)
        {
            //启动热更DLL的Main方法
            m_AppDomain.Invoke("HotFixProject.HotFixMain", "Main", null, obj, transform);
        }

        internal void OpenLoadingUI(string name)
        {
            m_AppDomain.Invoke("HotFixProject.HotFixMain", "OpenLoadingUI", null, name);
        }
    }
}