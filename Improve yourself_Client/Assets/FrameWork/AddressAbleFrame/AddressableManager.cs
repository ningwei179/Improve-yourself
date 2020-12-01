using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace Improve
{
    /// <summary>
    /// Addressable������Դ����ʵ������ԴҪ���������ռ�
    /// �ٲ�ͬģ��ֱ�ӵ��ò����㣬д��Manager����ʹ�ã�
    /// �����ٵ��������ռ���
    /// </summary>
    public class AddressableManager : Singleton<AddressableManager>
    {
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
        /// �첽������Դ
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
        /// �첽ʵ��������
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