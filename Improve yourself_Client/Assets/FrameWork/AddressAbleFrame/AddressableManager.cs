using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace Improve
{
    /// <summary>
    /// Addressable加载资源或者实例化资源要导入命名空间
    /// 再不同模块直接调用不方便，写个Manager方便使用，
    /// 不用再导入命名空间了
    /// </summary>
    public class AddressableManager : Singleton<AddressableManager>
    {
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
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        public void AsyncLoadResource<T>(string name, Action<T> callback)
        {
            m_Startmono.StartCoroutine(LoadAssetAsync(name, callback));
        }

        private IEnumerator LoadAssetAsync<T>(string name, Action<T> callback)
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(name);
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                callback(handle.Result);
            }
        }

        /// <summary>
        /// 异步实例化对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        public void AsyncInstantiate(string name, Action<GameObject> callback)
        {
            m_Startmono.StartCoroutine(InstantiateAsync(name, callback));
        }

        private IEnumerator InstantiateAsync(string name, Action<GameObject> callback)
        {
            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(name);
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                callback(handle.Result);
            }
        }

        public void Release(object obj)
        {
            Addressables.Release(obj);
        }
    }
}