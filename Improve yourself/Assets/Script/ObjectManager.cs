using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对象管理器
/// 管理需要实例化的资源
/// </summary>
public class ObjectManager : Singleton<ObjectManager>
{
    //对象池节点
    public Transform RecyclePoolTrs;
    //场景节点
    public Transform SceneTrs;

    /// <summary>
    /// ResourceObj对象池
    /// </summary>
    protected Dictionary<uint, List<ResourceObj>> m_ObjectPoolDic = new Dictionary<uint, List<ResourceObj>>();

    /// <summary>
    /// 暂存ResourceObj的Dic
    /// key为ResourceObj.m_CloneObj.GetInstanceID()
    /// </summary>
    protected Dictionary<int, ResourceObj> m_ResObjDic = new Dictionary<int, ResourceObj>();

    /// <summary>
    /// ResourceObj的类对象池
    /// </summary>
    protected ClassObjectPool<ResourceObj> m_ResObjClassPool = null;

    /// <summary>
    /// 根据异步的GUID储存ResourceObj,来判断是否正在异步加载
    /// </summary>
    protected Dictionary<long, ResourceObj> m_AsyncResObjs = new Dictionary<long, ResourceObj>();

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="recycleTrs">回收节点</param>
    /// <param name="sceneTrs">场景默认节点</param>
    public void Init(Transform recycleTrs, Transform sceneTrs)
    {
        m_ResObjClassPool = ObjectManager.Instance.GetOrCreateClassPool<ResourceObj>(1000);

        RecyclePoolTrs = recycleTrs;

        SceneTrs = sceneTrs;
    }

    /// <summary>
    /// 清空对象池
    /// </summary>
    public void ClearCache()
    {
        List<uint> tempList = new List<uint>();
        foreach (uint key in m_ObjectPoolDic.Keys)
        {
            List<ResourceObj> st = m_ObjectPoolDic[key];
            for (int i = st.Count - 1; i >= 0; i++)
            {
                ResourceObj resObj = st[i];
                if (!System.Object.ReferenceEquals(resObj.m_CloneObj, null) && resObj.m_bClear)
                {
                    GameObject.Destroy(resObj.m_CloneObj);
                    m_ResObjDic.Remove(resObj.m_CloneObj.GetInstanceID());
                    resObj.Reset();
                    m_ResObjClassPool.Recycle(resObj);
                }
            }

            if (st.Count <= 0)
            {
                tempList.Add(key);
            }
        }

        for (int i = 0; i < tempList.Count; i++)
        {
            if (m_ObjectPoolDic.ContainsKey(tempList[i]))
            {
                m_ObjectPoolDic.Remove(tempList[i]);
            }
        }
        tempList.Clear();
    }

    /// <summary>
    /// 清除某个资源在对象池中所有的对象
    /// </summary>
    /// <param name="crc"></param>
    public void ClearPoolObject(uint crc)
    {
        List<ResourceObj> st = null;
        if (!m_ObjectPoolDic.TryGetValue(crc, out st) || st == null)
        {
            return;
        }

        for (int i = st.Count - 1; i >= 0; i--)
        {
            ResourceObj resObj = st[i];
            if (resObj.m_bClear)
            {
                st.Remove(resObj);
                int tempID = resObj.m_CloneObj.GetInstanceID();
                GameObject.Destroy(resObj.m_CloneObj);
                resObj.Reset();
                m_ResObjDic.Remove(tempID);
                m_ResObjClassPool.Recycle(resObj);
            }
        }

        if (st.Count == 0)
        {
            m_ObjectPoolDic.Remove(crc);
        }
    }

    /// <summary>
    /// 从对象池取出一个对象
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    protected ResourceObj GetObjectFromPool(uint crc)
    {
        List<ResourceObj> st = null;
        if (m_ObjectPoolDic.TryGetValue(crc, out st) && st != null && st.Count != 0)
        {
            //增加引用计数
            ResourceManager.Instance.IncreaseResourceRef(crc);


            ResourceObj resObj = st[0];
            st.RemoveAt(0);
            GameObject obj = resObj.m_CloneObj;
            if (!System.Object.ReferenceEquals(obj, null))
            {
                resObj.m_Already = false;
#if UNITY_EDITOR
                if (obj.name.EndsWith("(Recycle)"))
                {
                    obj.name = obj.name.Replace("(Recycle)", "");
                }
#endif  
            }

            return resObj;
        }

        return null;
    }

    /// <summary>
    /// 取消异步加载
    /// </summary>
    /// <param name="guid"></param>
    public void CancleLoad(long guid)
    {
        ResourceObj resObj = null;
        //根据GUID，找到正在异步加载的资源，并且该资源没有在加载中，回调列表数量是0
        if (m_AsyncResObjs.TryGetValue(guid, out resObj) && ResourceManager.Instance.CancleLoad(resObj))
        {
            m_AsyncResObjs.Remove(guid);

            resObj.Reset();
            m_ResObjClassPool.Recycle(resObj);
        }
    }

    /// <summary>
    /// 是否正在异步加载
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    public bool IsingAsyncLoad(long guid)
    {
        return m_AsyncResObjs[guid] != null;
    }

    /// <summary>
    /// 该对象是否是ObjectManager创建的
    /// </summary>
    /// <returns></returns>
    public bool IsObjectManagerCreate(GameObject obj)
    {
        ResourceObj resObj = m_ResObjDic[obj.GetInstanceID()];
        return resObj == null ? false : true;
    }

    /// <summary>
    /// 预加载GameObject
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="count">预加载个数</param>
    /// <param name="clear">跳场景是否清楚</param>
    public void PreLoadGameObject(string path, int count = 1, bool clear = false)
    {
        List<GameObject> tempGameObjectList = new List<GameObject>();

        for (int i = 0; i < count; i++)
        {
            GameObject obj = InstantiateObject(path, false, clear);
            tempGameObjectList.Add(obj);
        }

        for (int i = 0; i < count; i++)
        {
            GameObject obj = tempGameObjectList[i];
            ReleaseObject(obj);
            obj = null;
        }
        tempGameObjectList.Clear();
    }

    /// <summary>
    /// 同步加载
    /// </summary>
    /// <param name="path"></param>
    /// <param name="setSceneObj">是否存到</param>
    /// <param name="bClear">跳场景是否清空</param>
    /// <returns></returns>
    public GameObject InstantiateObject(string path, bool setSceneObj = false, bool bClear = true)
    {
        uint crc = Crc32.GetCrc32(path);
        //从缓存池获取
        ResourceObj resObj = GetObjectFromPool(crc);

        if (resObj == null)
        {
            resObj = m_ResObjClassPool.Spawn(true);
            resObj.m_Crc = crc;
            resObj.m_bClear = bClear;

            //ResourceManager提供加载方法
            //给resObj.m_AssetBundleInfo 赋值
            //给resObj.m_AssetBundleInfo.m_Obj赋值
            resObj = ResourceManager.Instance.LoadResource(path, resObj);

            if (resObj.m_AssetBundleInfo.m_Obj != null)
            {
                resObj.m_CloneObj = GameObject.Instantiate(resObj.m_AssetBundleInfo.m_Obj) as GameObject;
            }
        }

        //setParent会造成性能消耗,所以加个布尔值控制下
        if (setSceneObj)
        {
            resObj.m_CloneObj.transform.SetParent(SceneTrs, false);
        }

        int tempID = resObj.m_CloneObj.GetInstanceID();
        if (!m_ResObjDic.ContainsKey(tempID))
        {
            m_ResObjDic.Add(tempID, resObj);
        }

        return resObj.m_CloneObj;
    }

    /// <summary>
    /// 异步加载
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fealFinish"></param>
    /// <param name="priority"></param>
    /// <param name="setSceneObj"></param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="param3"></param>
    /// <param name="bClear"></param>
    public long InstantiateObjectAsync(string path, OnAsyncFinish dealFinish, LoadResPriority priority, bool setSceneObj = false,
        object param1 = null, object param2 = null, object param3 = null, bool bClear = true)
    {
        if (string.IsNullOrEmpty(path))
            return 0;

        uint crc = Crc32.GetCrc32(path);
        //从缓存池获取
        ResourceObj resObj = GetObjectFromPool(crc);
        //从缓存中拿到了，直接返回
        if (resObj != null)
        {
            if (setSceneObj)
            {
                resObj.m_CloneObj.transform.SetParent(SceneTrs, false);
            }

            if (dealFinish != null)
            {
                dealFinish(path, resObj.m_CloneObj, param1, param2, param3);
            }

            return resObj.m_Guid;
        }

        //异步加载的GUID
        long guid = ResourceManager.Instance.CreateGuid();

        //缓存中没有，异步加载
        resObj = m_ResObjClassPool.Spawn(true); //new一个对象
        resObj.m_Crc = crc;
        resObj.m_SetSceneParent = setSceneObj;
        resObj.m_bClear = bClear;
        resObj.m_DealFinish = dealFinish;
        resObj.m_Param1 = param1;
        resObj.m_Param2 = param2;
        resObj.m_Param3 = param3;

        //调用ResourceManager的异步加载接口
        ResourceManager.Instance.AsyncLoadResource(path, resObj, OnLoadResObjFinish, priority);
        return guid;
    }

    /// <summary>
    /// 实例化资源加载完毕的回调
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="resObj">中间类</param>
    /// <param name="param1">参数1</param>
    /// <param name="param2">参数2</param>
    /// <param name="param3">参数3</param>
    void OnLoadResObjFinish(string path, ResourceObj resObj, object param1 = null, object param2 = null, object param3 = null)
    {
        if (resObj == null)
            return;

        if (resObj.m_AssetBundleInfo.m_Obj == null)
        {
#if UNITY_EDITOR
            Debug.LogError("异步资源加载的资源为空：" + path);
#endif
        }
        else
        {
            //给ResourceObj的实例化对象赋值
            resObj.m_CloneObj = GameObject.Instantiate(resObj.m_AssetBundleInfo.m_Obj) as GameObject;
        }

        //加载完成，就从正在加载的异步中移除
        if (m_AsyncResObjs.ContainsKey(resObj.m_Guid))
        {
            m_AsyncResObjs.Remove(resObj.m_Guid);
        }

        if (resObj.m_CloneObj != null && resObj.m_SetSceneParent)
        {
            resObj.m_CloneObj.transform.SetParent(SceneTrs, false);
        }

        if (resObj.m_DealFinish != null)
        {
            int tempID = resObj.m_CloneObj.GetInstanceID();
            if (!m_ResObjDic.ContainsKey(tempID))
            {
                m_ResObjDic.Add(tempID, resObj);
            }

            resObj.m_DealFinish(path, resObj.m_CloneObj, resObj.m_Param1, resObj.m_Param2, resObj.m_Param3);
        }
    }

    /// <summary>
    /// 回收资源
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="maxCacheCount"></param>
    /// <param name="destoryCache"></param>
    /// <param name="recycleParent">是否放回回收节点</param>
    public void ReleaseObject(GameObject obj, int maxCacheCount = -1, bool destoryCache = false, bool recycleParent = true)
    {
        if (obj == null)
            return;

        ResourceObj resObj = null;

        int temID = obj.GetInstanceID();

        if (!m_ResObjDic.TryGetValue(temID, out resObj))
        {
            Debug.Log(obj.name + "对象不是ObjectManager创建的!");
            return;
        }

        if (resObj == null)
        {
            Debug.LogError("缓存的ResourceObj为空");
        }

        if (resObj.m_Already)
        {
            Debug.LogError("该对象已经放回对象池了，检查自己是否清空引用");
            return;
        }

#if UNITY_EDITOR
        obj.name += "(Recycle)";
#endif
        List<ResourceObj> st = null;
        //不缓存
        if (maxCacheCount == 0)
        {
            m_ResObjDic.Remove(temID);

            ResourceManager.Instance.ReleaseResource(resObj, destoryCache);

            resObj.Reset();

            m_ResObjClassPool.Recycle(resObj);
        }
        else
        {   //回收到对象池
            if (!m_ObjectPoolDic.TryGetValue(resObj.m_Crc, out st) || st == null)
            {
                st = new List<ResourceObj>();
                m_ObjectPoolDic.Add(resObj.m_Crc, st);
            }

            if (resObj.m_CloneObj)
            {
                if (recycleParent)       //放到资源回收池节点下
                    resObj.m_CloneObj.transform.SetParent(RecyclePoolTrs);
                else
                {
                    resObj.m_CloneObj.SetActive(false);
                }
            }

            //不限制最大缓存数量，或者当前缓存数量小于最大缓存数量
            if (maxCacheCount < 0 || st.Count < maxCacheCount)
            {
                st.Add(resObj);

                resObj.m_Already = true;
                //ResourceManager做一个引用计数
                ResourceManager.Instance.DecreaseResourceRef(resObj);
            }
            else
            {
                m_ResObjDic.Remove(temID);
                ResourceManager.Instance.ReleaseResource(resObj, destoryCache);
                resObj.Reset();
                m_ResObjClassPool.Recycle(resObj);
            }
        }
    }

    #region 类对象池的使用
    /// <summary>
    /// 所有对象池字典
    /// </summary>
    protected Dictionary<Type, object> m_ClassPoolDic = new Dictionary<Type, object>();

    /// <summary>
    /// 获取或者创建类对象池，创建完成后，外面可以保存ClassObjectPool<T>
    /// 然后调用Spawn和Recycle来创建和回收类对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maxcount"></param>
    /// <returns></returns>
    public ClassObjectPool<T> GetOrCreateClassPool<T>(int maxcount) where T : class, new()
    {
        Type type = typeof(T);

        object outObj = null;
        if (!m_ClassPoolDic.TryGetValue(type, out outObj) || outObj == null)
        {
            ClassObjectPool<T> newPool = new ClassObjectPool<T>(maxcount);
            m_ClassPoolDic.Add(type, newPool);
            return newPool;
        }

        return outObj as ClassObjectPool<T>;
    }
    #endregion
}
