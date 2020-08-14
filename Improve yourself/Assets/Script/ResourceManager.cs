using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 资源加载的优先级
/// </summary>
public enum LoadResPriority
{
    RES_HIGHT = 0,  //最高优先级
    RES_MIDDLE = 1, //一般优先级
    RES_SLOW = 2,   //低优先级
    RES_NUM = 3     //优先级数量
}

/// <summary>
/// 异步加载的基本单位
/// </summary>
public class AsyncLoadResUnit
{
    //多个地方申请加载同一份资源，我们给每个地方都返回资源，返回的是同一份资源，且这个资源只加载一次

    //异步加载的回调列表
    public List<AsynCallBack> m_CallBackList = new List<AsynCallBack>();

    //要加载的资源的Crc
    public uint m_Crc;

    //要加载的资源的路径
    public string m_Path;

    //是否是一张图片
    public bool m_Sprite = false;   //判断是否是Sprite,因为Unity Object不能转换成Sprite

    //要加载的资源的优先级
    public LoadResPriority m_Priority = LoadResPriority.RES_SLOW;

    //重置异步加载基本单位
    public void Reset()
    {
        m_CallBackList.Clear();
        m_Crc = 0;
        m_Path = "";
        m_Sprite = false;
        m_Priority = LoadResPriority.RES_SLOW;
    }
}

/// <summary>
/// 异步加载的回调
/// </summary>
public class AsynCallBack
{
    /// <summary>
    /// 加载完成的回调
    /// </summary>
    public OnAsyncObjFinish m_DealFinish = null;
    /// <summary>
    /// 回调参数
    /// </summary>
    public object m_Param1 = null;
    public object m_Param2 = null;
    public object m_Param3 = null;

    public void Reset()
    {
        m_DealFinish = null;
        m_Param1 = null;
        m_Param2 = null;
        m_Param3 = null;
    }
}

/// <summary>
/// 异步加载完成的回调
/// </summary>
/// <param name="path"></param>
/// <param name="obj"></param>
/// <param name="param1"></param>
/// <param name="param2"></param>
/// <param name="param3"></param>
public delegate void OnAsyncObjFinish(string path, Object obj, object param1, object param2, object param3);

/// <summary>
/// 资源管理器
/// </summary>
public class ResourceManager : Singleton<ResourceManager>
{
    public bool m_LoadFromAssetBundle = false;      //是否从AssetBundle加载

    /// <summary>
    /// 缓存已经使用的资源列表
    /// </summary>
    public Dictionary<uint, AssetBundleInfo> AssetDic { get; set; } = new Dictionary<uint, AssetBundleInfo>();

    /// <summary>
    /// 缓存引用计数为0的资源列表，达到缓存最大的时候释放这个列表里面最早没用的资源
    /// </summary>
    public CMapList<AssetBundleInfo> m_NoRefrenceAssetMapList = new CMapList<AssetBundleInfo>();

    /// <summary>
    /// 正在异步加载的资源列表
    /// 按优先级分别装到对应的list中
    /// 这个才是用来加载的
    /// </summary>
    protected List<AsyncLoadResUnit>[] m_loadingAssetList = new List<AsyncLoadResUnit>[(int)LoadResPriority.RES_NUM];

    /// <summary>
    /// 正在异步加载的资源dic，
    /// 这个主要放便做查询用的
    /// </summary>
    public Dictionary<uint, AsyncLoadResUnit> m_LoadingAssetDic = new Dictionary<uint, AsyncLoadResUnit>();

    /// <summary>
    /// 中间类，回调类的类对象池
    /// </summary>
    protected ClassObjectPool<AsyncLoadResUnit> m_AsyncLoadResParamPool = new ClassObjectPool<AsyncLoadResUnit>(50);

    protected ClassObjectPool<AsynCallBack> m_AsynCallBackPool = new ClassObjectPool<AsynCallBack>(100);

    //最长连续卡着加载资源的时间，单位微秒
    private const long MAXLOADRESTIME = 200000;
    /// <summary>
    /// mono脚本
    /// </summary>
    protected MonoBehaviour m_Startmono;

    public void Init(MonoBehaviour mono)
    {
        for (int i = 0; i < (int)LoadResPriority.RES_NUM; i++)
        {
            m_loadingAssetList[i] = new List<AsyncLoadResUnit>();
        }
        m_Startmono = mono;
        m_Startmono.StartCoroutine(AsyncLoadCor());
    }

    /// <summary>
    /// 清空缓存,例如跳场景的时候用的
    /// </summary>
    public void ClearCache()
    {
        while (m_NoRefrenceAssetMapList.Size()>0)
        {
            AssetBundleInfo assetBundleInfo = m_NoRefrenceAssetMapList.Back();

            DestoryResouceItem(assetBundleInfo, true);

            m_NoRefrenceAssetMapList.Pop();
        }
    }

    /// <summary>
    /// 预加载资源
    /// 
    /// </summary>
    /// <param name="path"></param>
    public void PreloadRes(string path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        uint crc = Crc32.GetCrc32(path);
        //根据crc从缓存中获取一个AssetBundleInfo
        AssetBundleInfo item = GetCacheAssetBundleInfo(crc,0);  
        if (item != null)
        {
            return;
        }

        Object obj = null;
#if UNITY_EDITOR
        if (!m_LoadFromAssetBundle)
        {
            item = AssetBundleManager.Instance.FindAssetBundleInfo(crc);
            if (item.m_Obj != null)
            {
                obj = item.m_Obj as Object;
            }
            else
            {
                obj = LoadAssetByEditor<Object>(path);
            }
        }
#endif
        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadAssetBundleInfo(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_Obj != null)
                {
                    obj = item.m_Obj as Object;
                }
                else
                {
                    obj = item.m_AssetBundle.LoadAsset<Object>(item.m_AssetName);
                }
            }
        }

        //缓存AssetBundle资源块
        CacheAssetBundleInfo(path, ref item, crc, obj);

        //设置跳场景不清空缓存
        item.m_Clear = false;

        //卸载资源设置成不销毁
        ReleaseResource(obj, false);

        //上面的步骤就是提前加载一次资源，然后缓存AssetBundle资源块，
        //再将资源块设置成跳场景不清空缓存，预加载资源后应该就是跳场景进入游戏了
        //最后释放掉资源设置成不销毁，存到双向链表里面
    }

    /// <summary>
    /// 同步资源加载，外部直接调用，仅加载不需要实例化的资源，例如Texture和音频之类的
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T LoadResource<T>(string path) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(path))
            return null;

        uint crc = Crc32.GetCrc32(path);
        //根据crc从缓存中获取一个AssetBundleInfo
        AssetBundleInfo item = GetCacheAssetBundleInfo(crc);
        if (item != null)
        {
            return item.m_Obj as T;
        }

        T obj = null;
#if UNITY_EDITOR
        if (!m_LoadFromAssetBundle)
        {
            item = AssetBundleManager.Instance.FindAssetBundleInfo(crc);
            if (item.m_Obj != null)
            {
                obj = item.m_Obj as T;
            }
            else
            {
                obj = LoadAssetByEditor<T>(path);
            }
        }
#endif
        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadAssetBundleInfo(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_Obj != null)
                {
                    obj = item.m_Obj as T;
                }
                else
                {
                    obj = item.m_AssetBundle.LoadAsset<T>(item.m_AssetName);
                }
            }
        }

        //缓存AssetBundle资源块
        CacheAssetBundleInfo(path, ref item, crc, obj);

        return obj;
    }

    /// <summary>
    /// 不需要实例化的资源卸载，例如Texture和音频之类的
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="destoryObj"></param>
    public bool ReleaseResource(Object obj, bool destoryObj = false)
    {
        if (obj == null)
        {
            return false;
        }

        AssetBundleInfo item = null;
        foreach (AssetBundleInfo assetBundleInfo in AssetDic.Values)
        {
            if (assetBundleInfo.m_Guid == obj.GetInstanceID())
            {
                item = assetBundleInfo;
            }
        }

        if (item == null)
        {
            Debug.LogError("AssetDic 里不存在该资源 ：" + obj.name + "  可能释放多次");
            return false;
        }

        item.RefCount--;

        DestoryResouceItem(item, destoryObj);
        return true;
    }

    /// <summary>
    /// 不需要实例化的资源的卸载
    /// </summary>
    /// <param name="path"></param>
    /// <param name="destoryObj"></param>
    /// <returns></returns>
    public bool ReleaseResource(string path, bool destoryObj = false)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        uint crc = Crc32.GetCrc32(path);

        AssetBundleInfo item = null;
        if(AssetDic.TryGetValue(crc,out item) || null == item){
            Debug.LogError("该路径："+path+"  的资源不存在！");
        }

        if (item == null)
        {
            Debug.LogError("AssetDic 里不存在该资源 ：" + item.m_AssetName + "  可能释放多次");
            return false;
        }

        item.RefCount--;

        DestoryResouceItem(item, destoryObj);
        return true;
    }

    /// <summary>
    /// 缓存AssetBundle资源块
    /// </summary>
    /// <param name="path"></param>
    /// <param name="assetBundleInfo"></param>
    /// <param name="crc"></param>
    /// <param name="obj"></param>
    /// <param name="addrefcount">添加引用数量</param>
    void CacheAssetBundleInfo(string path, ref AssetBundleInfo item, uint crc, Object obj, int addrefcount = 1)
    {
        //缓存太多，清除最早没有使用的资源
        WashOut();

        if (item == null)
            Debug.LogError("AssetBundleInfo is null " + path);

        if (obj == null)
            Debug.LogError("ResourceLoad Fail " + path);

        item.m_Obj = obj;
        item.m_Guid = obj.GetInstanceID();
        item.m_LastUseTime = Time.realtimeSinceStartup;
        item.RefCount += addrefcount;

        AssetBundleInfo oldItem = null;
        if (AssetDic.TryGetValue(item.m_Crc, out oldItem))
        {
            AssetDic[item.m_Crc] = item;
        }
        else
        {
            AssetDic.Add(item.m_Crc, item);
        }
    }

    /// <summary>
    /// 缓存太多，清除最早没有使用的资源
    /// </summary>
    protected void WashOut()
    {
        //当前内存使用大于80%，我们就进行清除最早没用的资源

        //if (m_NoRefrenceAssetMapList.Size() <= 0)
        //{
        //    break;
        //}
    }

    /// <summary>
    /// 回收一个资源
    /// </summary>
    /// <param name="item"></param>
    /// <param name="destroy"></param>
    protected void DestoryResouceItem(AssetBundleInfo item, bool destroyCache = false)
    {
        //资源是空的，或者正在被引用不能清除
        if (item == null || item.RefCount > 0)
        {
            return;
        }

        //AssetDic中删除这个资源
        if (!AssetDic.Remove(item.m_Crc))
        {
            return;
        }

        //设置的不清除缓存，就加入到双向链表里面
        if (!destroyCache)
        {
            m_NoRefrenceAssetMapList.InsetToHead(item);
            return;
        }


        //释放AssetBundle引用
        AssetBundleManager.Instance.ReleaseAssetBundle(item);

        if (item.m_Obj != null)
        {
            item.m_Obj = null;
        }

        //编辑器下的卸载要用这个方法才能从内存卸载
#if UNITY_EDITOR
        Resources.UnloadUnusedAssets();
#endif
    }

    /// <summary>
    /// 编辑器下测试资源加载用的
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
#if UNITY_EDITOR
    protected T LoadAssetByEditor<T>(string path) where T : UnityEngine.Object
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
    }
#endif

    /// <summary>
    /// 从缓存获取一个资源块
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    AssetBundleInfo GetCacheAssetBundleInfo(uint crc, int addrefcount = 1)
    {
        AssetBundleInfo item = null;
        if (AssetDic.TryGetValue(crc, out item) && item != null)
        {
            item.RefCount += addrefcount;
            item.m_LastUseTime = Time.realtimeSinceStartup;
        }

        return item;
    }

    /// <summary>
    /// 异步资源加载，外部直接调用，（仅仅加载不需要实例化的资源，例如Texture和音频之类的）
    /// </summary>
    public void AsyncLoadResource(string path, OnAsyncObjFinish dealFinish, LoadResPriority priority, object param1 = null, object param2 = null, object param3 = null, uint crc = 0)
    {
        if (crc == 0)
        {
            crc = Crc32.GetCrc32(path);
        }

        AssetBundleInfo item = GetCacheAssetBundleInfo(crc);
        if (item != null)
        {
            dealFinish?.Invoke(path, item.m_Obj, param1, param2, param3);
            return;
        }

        //判断是否在加载中
        AsyncLoadResUnit unit = null;

        //没有找到这个异步加载单位，或者这个异步加载单位是空
        if (!m_LoadingAssetDic.TryGetValue(crc, out unit) || unit == null)
        {
            unit = m_AsyncLoadResParamPool.Spawn(true);
            unit.m_Crc = crc;
            unit.m_Path = path;
            unit.m_Priority = priority;

            m_LoadingAssetDic.Add(crc, unit);   //添加到正在异步加载的资源dic中

            m_loadingAssetList[(int)priority].Add(unit);   //按照加载优先级，添加到对应的正在异步加载的资源列表中
        }

        //往回调列表里面添加回调
        AsynCallBack callBack = m_AsynCallBackPool.Spawn(true);
        callBack.m_DealFinish = dealFinish;
        callBack.m_Param1 = param1;
        callBack.m_Param2 = param2;
        callBack.m_Param3 = param3;

        //往这个异步加载单位的回调列表中添加一个回调
        //可能多个地方加载同一份资源，这样做只加载一次资源，
        //加载完了后，根据回调列表一次返回这份资源
        unit.m_CallBackList.Add(callBack);
    }

    /// <summary>
    /// 异步加载
    /// </summary>
    /// <returns></returns>
    IEnumerator AsyncLoadCor()
    {
        List<AsynCallBack> callBackList = null;
        while (true)
        {
            bool haveYield = false;     //是否已经等了一帧了
            //上次yield的时间
            long lastYiledTime = System.DateTime.Now.Ticks;

            //遍历优先级列表，从0开始，0表示最高
            for (int i = 0; i < (int)LoadResPriority.RES_NUM; i++)
            {
                List<AsyncLoadResUnit> loadingList = m_loadingAssetList[i];
                if (loadingList.Count <= 0)
                    continue;

                AsyncLoadResUnit loadintItem = loadingList[0];
                loadingList.RemoveAt(0);
                callBackList = loadintItem.m_CallBackList;

                Object obj = null;
                AssetBundleInfo item = null;
#if UNITY_EDITOR
                if (!m_LoadFromAssetBundle)
                {
                    obj = LoadAssetByEditor<Object>(loadintItem.m_Path);
                    //模拟异步加载
                    yield return new WaitForSeconds(0.5f);

                    item = AssetBundleManager.Instance.FindAssetBundleInfo(loadintItem.m_Crc);
                }
#endif
                if (obj == null)
                {
                    item = AssetBundleManager.Instance.LoadAssetBundleInfo(loadintItem.m_Crc);
                    if (item != null && item.m_AssetBundle != null)
                    {
                        AssetBundleRequest abRequest = null;
                        if (loadintItem.m_Sprite)   //判断是否是Sprite,因为Unity Object不能转换成Sprite
                        {
                            abRequest = item.m_AssetBundle.LoadAssetAsync<Sprite>(item.m_AssetName);
                        }
                        else
                        {
                            abRequest = item.m_AssetBundle.LoadAssetAsync(item.m_AssetName);
                        }
                        yield return abRequest;
                        if (abRequest.isDone)
                        {
                            obj = abRequest.asset;
                        }

                        lastYiledTime = System.DateTime.Now.Ticks;
                    }
                }

                CacheAssetBundleInfo(loadintItem.m_Path, ref item, loadintItem.m_Crc, obj, callBackList.Count);

                //处理回调
                for (int j = 0; j < callBackList.Count; j++)
                {
                    AsynCallBack callBack = callBackList[j];
                    if (callBack != null && callBack.m_DealFinish != null)
                    {
                        callBack.m_DealFinish(loadintItem.m_Path,obj,callBack.m_Param1, callBack.m_Param2, callBack.m_Param3);
                        callBack.m_DealFinish = null;
                    }
                    callBack.Reset();       //还原回调，并且回收
                    m_AsynCallBackPool.Recycle(callBack);
                }

                obj = null;
                callBackList.Clear();
                m_LoadingAssetDic.Remove(loadintItem.m_Crc);
                loadintItem.Reset();
                m_AsyncLoadResParamPool.Recycle(loadintItem);

                //加载一个资源时间过长，等一帧
                if (System.DateTime.Now.Ticks - lastYiledTime > MAXLOADRESTIME)
                {
                    yield return null;
                    lastYiledTime = System.DateTime.Now.Ticks;
                    haveYield = true;
                }
            }

            //内存循环加载很快，没有等一帧，但是加载整个优先级列表时间过长，等一帧
            if (!haveYield || System.DateTime.Now.Ticks - lastYiledTime > MAXLOADRESTIME)
            {
                lastYiledTime = System.DateTime.Now.Ticks;
                yield return null;
            }

        }
    }
}

