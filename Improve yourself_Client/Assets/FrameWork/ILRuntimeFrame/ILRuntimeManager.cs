/****************************************************     文件：ILRuntimeManager.cs 	作者：NingWei     日期：2020/10/13 15:49:48 	功能：ILRuntime热更管理类 *****************************************************/
using ILRuntime.Runtime.Enviorment;
using System.Collections;
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
            if (FrameConstr.ILState == ILRuntimeState.Open)
            {
                MemoryStream dllMs = null;
                MemoryStream pdbMs = null;
                if (FrameConstr.UseAssetAddress == AssetAddress.Addressable)
                {
                    AsyncOperationHandle<TextAsset> dllHandle = Addressables.LoadAssetAsync<TextAsset>(DLLPATH);
                    yield return dllHandle;
                    if (dllHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        dllMs = new MemoryStream(dllHandle.Result.bytes);
                    }
                    AsyncOperationHandle<TextAsset> pdbHandle = Addressables.LoadAssetAsync<TextAsset>(PDBPATH);
                    yield return pdbHandle;
                    if (pdbHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        pdbMs = new MemoryStream(pdbHandle.Result.bytes);
                    }
                }
                else {
                    
                    TextAsset dllText = ResourceManager.Instance.LoadResource<TextAsset>(DLLPATH);
                    dllMs = new MemoryStream(dllText.bytes);
                    TextAsset padText = ResourceManager.Instance.LoadResource<TextAsset>(PDBPATH);
                    pdbMs = new MemoryStream(padText.bytes);
                }

                if (dllMs != null && pdbMs != null)
                {
                    Debug.Log("加载ILRuntime热补丁！");
                    //整个工程只有一个ILRuntime的AppDomain
                    m_AppDomain = new AppDomain();

                    m_AppDomain.LoadAssembly(dllMs, pdbMs, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());

                    ILRuntimeRegister.Instance.InitializeILRuntime(m_AppDomain);

                    OnHotFixLoaded(obj, transform);
                }
            }
            else {
                OnHotFixLoaded(obj, transform);
            }
            yield return null;
        }

        void OnHotFixLoaded(GameObject obj, Transform transform)
        {
            if (FrameConstr.ILState == ILRuntimeState.Open)
            {
                //启动热更DLL的Main方法
                Debug.Log("启动热更DLL的Main方法");
                m_AppDomain.Invoke("HotFixProject.HotFixMain", "Main", null, obj, transform);
            }
            else {
                Debug.Log("启动本地模拟的Main方法");
                HotFixMain.Main(obj, transform);
            }
        }

        internal void OpenLoadingUI(string name)
        {
            if (FrameConstr.ILState == ILRuntimeState.Open)
            {
                Debug.Log("启动热更DLL的Loading界面");
                m_AppDomain.Invoke("HotFixProject.HotFixMain", "OpenLoadingUI", null, name);
            }
            else {
                Debug.Log("启动本地模拟的Loading界面");
                HotFixMain.OpenLoadingUI(name);
            }
        }
    }
}