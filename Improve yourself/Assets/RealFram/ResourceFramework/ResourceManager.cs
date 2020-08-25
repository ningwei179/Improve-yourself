using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 包含实例化资源的资源块
/// </summary>
public class ResourceObj
{
    //路径对应的crc
    public uint m_Crc = 0;

    /// <summary>
    /// ResourceObj中引用的AssetBundle资源块信息
    /// </summary>
    public AssetBundleInfo m_AssetBundleInfo = null;

    /// <summary>
    /// 实例化出来的GameObject
    /// </summary>
    public GameObject m_CloneObj = null;

    /// <summary>
    /// 跳场景是否清楚
    /// </summary>
    public bool m_bClear = false;

    //储存GUID，异步加载队列的GUID
    public long m_Guid = 0;

    /// <summary>
    /// 是否已经放回对象池
    /// </summary>
    public bool m_Already = false;

    //----------------------下面是异步加载需要的部分参数
    //是否放到场景节点下面
    public bool m_SetSceneParent = false;

    //实例化资源加载完成后的回调
    public OnAsyncFinish m_DealFinish = null;

    /// <summary>
    /// 回调参数
    /// </summary>
    public object m_Param1 = null;
    public object m_Param2 = null;
    public object m_Param3 = null;

    //离线数据
    public OfflineData m_offlineData = null;

    //重置
    public void Reset()
    {
        m_Crc = 0;
        m_CloneObj = null;
        m_bClear = false;
        m_Guid = 0;
        m_Already = false;
        m_SetSceneParent = false;
        m_DealFinish = null;
        m_Param1 = null;
        m_Param2 = null;
        m_Param3 = null;
        m_offlineData = null;
    }
}

#region 异步加载相关

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
    /// 实例化对象加载完成的回调（针对ObjectManager）
    /// </summary>
    public OnAsyncResObjFinish m_DealResObjFinish = null;

    /// <summary>
    /// 异步加载的实例化资源块
    /// </summary>
    public ResourceObj m_ResObj = null;
//--------------------------------------------------------------
    /// <summary>
    /// 非实例化对象加载完成的回调
    /// </summary>
    public OnAsyncFinish m_DealFinish = null;

    /// <summary>
    /// 回调参数
    /// </summary>
    public object m_Param1 = null;
    public object m_Param2 = null;
    public object m_Param3 = null;

    public void Reset()
    {
        m_DealFinish = null;
        m_DealResObjFinish = null;
        m_Param1 = null;
        m_Param2 = null;
        m_Param3 = null;
        m_ResObj = null;
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
public delegate void OnAsyncFinish(string path, Object obj, object param1, object param2, object param3);

/// <summary>
/// 实例化对象异步加载完成的回调
/// </summary>
/// <param name="path"></param>
/// <param name="obj"></param>
/// <param name="param1"></param>
/// <param name="param2"></param>
/// <param name="param3"></param>
public delegate void OnAsyncResObjFinish(string path, ResourceObj obj, object param1, object param2, object param3);
#endregion


/// <summary>
/// 资源管理器
/// 管理不需要实例化的资源
/// </summary>
public class ResourceManager : Singleton<ResourceManager>
{
    protected long m_Guid = 0;
    public bool m_LoadFromAssetBundle = true;      //是否从AssetBundle加载

    /// <summary>
    /// 缓存已经使用的资源列表
    /// </summary>
    public Dictionary<uint, AssetBundleInfo> AssetDic { get; set; } = new Dictionary<uint, AssetBundleInfo>();

    /// <summary>
    /// 缓存引用计数为0的资源列表，达到缓存最大的时候释放这个列表里面最早没用的资源
    /// </summary>
    public CMapList<AssetBundleInfo> m_NoRefrenceAssetMapList = new CMapList<AssetBundleInfo>();

    #region 异步加载相关
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

    //最大缓存个数
    private const int MAXCACHECOUNT = 500;
#endregion

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
        for (int i = 0; i < (int)LoadResPriority.RES_NUM; i++)
        {
            m_loadingAssetList[i] = new List<AsyncLoadResUnit>();
        }
        m_Startmono = mono;
        m_Startmono.StartCoroutine(AsyncLoadCor());
    }

    /// <summary>
    /// 创建唯一的GUID
    /// </summary>
    /// <returns></returns>
    public long CreateGuid()
    {
        return m_Guid++;
    }

    /// <summary>
    /// 清空缓存,例如跳场景的时候用的
    /// </summary>
    public void ClearCache()
    {
        //收集跳场景需要清除的资源
        List<AssetBundleInfo> tempList = new List<AssetBundleInfo>();
        foreach (AssetBundleInfo abInfo in AssetDic.Values)
        {
            if (abInfo.m_Clear)
            {
                tempList.Add(abInfo);
            }
        }

        //清除收集到的资源
        foreach (AssetBundleInfo abInfo in tempList)
        {
            DestoryResouceItem(abInfo, true);
        }
        tempList.Clear();
    }

    /// <summary>
    /// 取消异步加载资源
    /// </summary>
    /// <returns></returns>
    public bool CancleLoad(ResourceObj res)
    {
        AsyncLoadResUnit unit = null;
        //这俩个条件主要来判断该资源还没有进入到异步加载流程
        //多个人请求同一份资源，要多个人都取消才能取消这个异步加载单位
        //其中一个人取消异步加载，只是把应该给他的回调取消掉
        if (m_LoadingAssetDic.TryGetValue(res.m_Crc, out unit) &&
            m_loadingAssetList[(int) unit.m_Priority].Contains(unit))
        {
            for (int i = unit.m_CallBackList.Count-1; i >=0; i--)
            {
                //取消对应的回调列表
                AsynCallBack tempCallBack = unit.m_CallBackList[i];
                if (tempCallBack != null && res == tempCallBack.m_ResObj)
                {
                    tempCallBack.Reset();
                    m_AsynCallBackPool.Recycle(tempCallBack);
                    unit.m_CallBackList.Remove(tempCallBack);
                }
            }
            //异步加载单位的回调列表为0，表示所有人都取消了这个资源加载，回收他
            if (unit.m_CallBackList.Count <= 0)
            {
                unit.Reset();
                m_loadingAssetList[(int) unit.m_Priority].Remove(unit);
                m_AsyncLoadResParamPool.Recycle(unit);
                m_LoadingAssetDic.Remove(res.m_Crc);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 预加载资源,仅仅加载不实例化
    /// </summary>
    /// <param name="path">资源路径</param>
    public void PreloadRes(string path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        uint crc = Crc32.GetCrc32(path);
        //根据crc从缓存中获取一个AssetBundleInfo
        AssetBundleInfo abInfo = GetCacheAssetBundleInfo(crc,0);  
        if (abInfo != null)
        {
            return;
        }

        Object obj = null;
#if UNITY_EDITOR
        if (!m_LoadFromAssetBundle)
        {
            abInfo = AssetBundleManager.Instance.FindAssetBundleInfo(crc);
            if (abInfo.m_Obj != null)
            {
                obj = abInfo.m_Obj as Object;
            }
            else
            {
                if (abInfo == null)
                {
                    abInfo = new AssetBundleInfo();
                    abInfo.m_Crc = crc;
                }
                obj = LoadAssetByEditor<Object>(path);
            }
        }
#endif
        if (obj == null)
        {
            abInfo = AssetBundleManager.Instance.LoadAssetBundleInfo(crc);
            if (abInfo != null && abInfo.m_AssetBundle != null)
            {
                if (abInfo.m_Obj != null)
                {
                    obj = abInfo.m_Obj as Object;
                }
                else
                {
                    obj = abInfo.m_AssetBundle.LoadAsset<Object>(abInfo.m_AssetName);
                }
            }
        }

        //缓存AssetBundle资源块
        CacheAssetBundleInfo(path, ref abInfo, crc, obj);

        //设置跳场景不清空缓存
        abInfo.m_Clear = false;

        //卸载资源设置成不销毁
        ReleaseResource(path, false);

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
        AssetBundleInfo abInfo = GetCacheAssetBundleInfo(crc);
        if (abInfo != null)
        {
            return abInfo.m_Obj as T;
        }

        T obj = null;
#if UNITY_EDITOR
        if (!m_LoadFromAssetBundle)
        {
            abInfo = AssetBundleManager.Instance.FindAssetBundleInfo(crc);
            if (abInfo.m_Obj != null)
            {
                obj = abInfo.m_Obj as T;
            }
            else
            {
                if (abInfo == null)
                {
                    abInfo = new AssetBundleInfo();
                    abInfo.m_Crc = crc;
                }
                obj = LoadAssetByEditor<T>(path);
            }
        }
#endif
        if (obj == null)
        {
            abInfo = AssetBundleManager.Instance.LoadAssetBundleInfo(crc);
            if (abInfo != null && abInfo.m_AssetBundle != null)
            {
                if (abInfo.m_Obj != null)
                {
                    obj = abInfo.m_Obj as T;
                }
                else
                {
                    obj = abInfo.m_AssetBundle.LoadAsset<T>(abInfo.m_AssetName);
                }
            }
        }

        //缓存AssetBundle资源块
        CacheAssetBundleInfo(path, ref abInfo, crc, obj);

        return obj;
    }

    /// <summary>
    /// 不需要实例化的资源卸载，例如Texture和音频之类的
    /// 根据Object卸载
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="destoryObj"></param>
    public bool ReleaseResource(Object obj, bool destoryObj = false)
    {
        if (obj == null)
        {
            return false;
        }

        AssetBundleInfo abInfo = null;
        foreach (AssetBundleInfo assetBundleInfo in AssetDic.Values)
        {
            if (assetBundleInfo.m_Guid == obj.GetInstanceID())
            {
                abInfo = assetBundleInfo;
            }
        }

        if (abInfo == null)
        {
            Debug.LogError("AssetDic 里不存在该资源 ：" + obj.name + "  可能释放多次");
            return false;
        }

        abInfo.RefCount--;

        DestoryResouceItem(abInfo, destoryObj);
        return true;
    }

    /// <summary>
    /// 不需要实例化的资源的卸载,例如Texture和音频之类的
    /// 根据路径卸载
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
        //没找到资源或者找到的资源是空的
        if(!AssetDic.TryGetValue(crc,out item) || null == item){
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

    #region 针对给ObjectManager的接口
    /// <summary>
    /// 同步加载资源，针对给ObjectManager的接口
    /// 给ResourceObj.m_AssetBundleInfo赋值用的
    /// </summary>
    /// <param name="path"></param>
    /// <param name="resObj">传入的resourceObj，</param>
    /// <returns></returns>
    public ResourceObj LoadResource(string path, ResourceObj resObj)
    {
        if (resObj == null)
        {
            return null;
        }

        //获取crc
        uint crc = resObj.m_Crc == 0 ? Crc32.GetCrc32(path) : resObj.m_Crc;

        //根据crc从缓存中获取 AssetBundle资源块信息
        AssetBundleInfo abInfo = GetCacheAssetBundleInfo(crc);

        if (abInfo != null)
        {
            resObj.m_AssetBundleInfo = abInfo;
            return resObj;
        }

        Object obj = null;
#if UNITY_EDITOR
        if (!m_LoadFromAssetBundle)
        {
            abInfo = AssetBundleManager.Instance.FindAssetBundleInfo(crc);
            if (abInfo != null && abInfo.m_Obj != null)
            {
                obj = abInfo.m_Obj;
            }
            else
            {
                if (abInfo == null)
                {
                    abInfo = new AssetBundleInfo();
                    abInfo.m_Crc = crc;
                }

                obj = LoadAssetByEditor<Object>(path);
            }
        }
#endif
        //缓存中没有AssetBundle资源块信息，加载它
        if (obj == null)
        {
            abInfo = AssetBundleManager.Instance.LoadAssetBundleInfo(crc);

            if (abInfo != null && abInfo.m_AssetBundle != null)
            {
                if (abInfo.m_Obj != null)
                {
                    obj = abInfo.m_Obj;
                }
                else
                {
                    obj = abInfo.m_AssetBundle.LoadAsset<Object>(abInfo.m_AssetName);
                }
            }
        }

        //将加载的资源放入缓存中
        CacheAssetBundleInfo(path, ref abInfo, crc, obj);

        resObj.m_AssetBundleInfo = abInfo;

        abInfo.m_Clear = resObj.m_bClear;

        return resObj;
    }

    /// <summary>
    /// 根据ResourceObj 卸载资源
    /// </summary>
    /// <param name="resObj"></param>
    /// <param name="destoryObj"></param>
    /// <returns></returns>
    public bool ReleaseResource(ResourceObj resObj, bool destoryObj = false)
    {
        if (resObj == null)
            return false;

        uint crc = resObj.m_Crc;

        AssetBundleInfo abInfo = null;
        if (!AssetDic.TryGetValue(resObj.m_Crc, out abInfo) || null == abInfo)
        {
            Debug.LogError("该路径：" + resObj.m_CloneObj.name + "  的资源不存在！");
        }

        GameObject.Destroy(resObj.m_CloneObj);

        abInfo.RefCount--;

        DestoryResouceItem(abInfo, destoryObj);

        return true;

        //return ReleaseResource(resourceObj.m_AssetBundleInfo.m_Obj, destoryObj);
    }

    /// <summary>
    /// 根据Resobj增加引用计数
    /// </summary>
    /// <returns></returns>
    public int IncreageResourceRef(ResourceObj resObj, int count = 1)
    {
        return resObj != null ? IncreaseResourceRef(resObj.m_Crc, count) : 0;
    }

    /// <summary>
    /// 根据Resobj减少引用计数
    /// </summary>
    /// <returns></returns>
    public int DecreaseResourceRef(ResourceObj resObj, int count = 1)
    {
        return resObj != null ? DecreaseResourceRef(resObj.m_Crc, count) : 0;

    }
    #endregion

    /// <summary>
    /// 根据路径增加引用计数
    /// </summary>
    /// <returns></returns>
    public int IncreaseResourceRef(uint crc = 0, int count = 1)
    {
        AssetBundleInfo abInfo = null;
        if (!AssetDic.TryGetValue(crc, out abInfo) || abInfo == null)
            return 0;

        abInfo.RefCount += count;
        abInfo.m_LastUseTime = Time.realtimeSinceStartup;

        return abInfo.RefCount;
    }

    /// <summary>
    /// 根据路径减少引用计数
    /// </summary>
    /// <returns></returns>
    public int DecreaseResourceRef(uint crc = 0, int count = 1)
    {
        AssetBundleInfo abInfo = null;
        if (!AssetDic.TryGetValue(crc, out abInfo) || abInfo == null)
            return 0;

        abInfo.RefCount -= count;

        return abInfo.RefCount;
    }

    /// <summary>
    /// 缓存AssetBundleInfo到AssetDic中
    /// 同时给AssetBundleInfo.m_Obj，AssetBundleInfo.m_Guid，AssetBundleInfo.RefCount等属性赋值
    /// </summary>
    /// <param name="path"></param>
    /// <param name="assetBundleInfo"></param>
    /// <param name="crc"></param>
    /// <param name="obj"></param>
    /// <param name="addrefcount">添加引用数量</param>
    void CacheAssetBundleInfo(string path, ref AssetBundleInfo abInfo, uint crc, Object obj, int addrefcount = 1)
    {
        //缓存太多，清除最早没有使用的资源
        WashOut();

        if (abInfo == null)
            Debug.LogError("AssetBundleInfo is null " + path);

        if (obj == null)
            Debug.LogError("ResourceLoad Fail " + path);

        abInfo.m_Obj = obj;
        abInfo.m_Guid = obj.GetInstanceID();
        abInfo.m_LastUseTime = Time.realtimeSinceStartup;
        abInfo.RefCount += addrefcount;

        AssetBundleInfo oldItem = null;
        if (AssetDic.TryGetValue(abInfo.m_Crc, out oldItem))
        {
            AssetDic[abInfo.m_Crc] = abInfo;
        }
        else
        {
            AssetDic.Add(abInfo.m_Crc, abInfo);
        }
    }

    /// <summary>
    /// 缓存太多，清除最早没有使用的资源
    /// </summary>
    protected void WashOut()
    {
        //当大于缓存个数时，进行一般释放
        while (m_NoRefrenceAssetMapList.Size()>=MAXCACHECOUNT)
        {
            for (int i = 0; i < MAXCACHECOUNT/2; i++)
            {
                AssetBundleInfo abInfo = m_NoRefrenceAssetMapList.Back();

                DestoryResouceItem(abInfo,true);
            }
        }


    }

    /// <summary>
    /// 回收一个资源
    /// </summary>
    /// <param name="abInfo"></param>
    /// <param name="destroy"></param>
    protected void DestoryResouceItem(AssetBundleInfo abInfo, bool destroyCache = false)
    {
        //资源是空的，或者正在被引用不能清除
        if (abInfo == null || abInfo.RefCount > 0)
        {
            return;
        }

        //设置的不清除缓存，就加入到双向链表里面
        if (!destroyCache)
        {
            m_NoRefrenceAssetMapList.InsetToHead(abInfo);
            return;
        }

        //AssetDic中删除这个资源
        if (!AssetDic.Remove(abInfo.m_Crc))
        {
            return;
        }

        //从双向链表中清除这个资源
        m_NoRefrenceAssetMapList.Remvoe(abInfo);

        //释放AssetBundle引用
        AssetBundleManager.Instance.ReleaseAssetBundle(abInfo);

        //清空资源对应的对象池
        ObjectManager.Instance.ClearPoolObject(abInfo.m_Crc);

        if (abInfo.m_Obj != null)
        {
            abInfo.m_Obj = null;
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
    /// 从资源池获取一个资源块
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    AssetBundleInfo GetCacheAssetBundleInfo(uint crc, int addrefcount = 1)
    {
        AssetBundleInfo abInfo = null;
        if (AssetDic.TryGetValue(crc, out abInfo) && abInfo != null)
        {
            abInfo.RefCount += addrefcount;
            abInfo.m_LastUseTime = Time.realtimeSinceStartup;
        }

        return abInfo;
    }

    #region 异步加载相关
    /// <summary>
    /// 异步资源加载，外部直接调用，（仅仅加载不需要实例化的资源，例如Texture和音频之类的）
    /// </summary>
    public void AsyncLoadResource(string path, OnAsyncFinish dealFinish, LoadResPriority priority, object param1 = null, object param2 = null, object param3 = null, uint crc = 0)
    {
        if (crc == 0)
        {
            crc = Crc32.GetCrc32(path);
        }

        AssetBundleInfo abInfo = GetCacheAssetBundleInfo(crc);
        if (abInfo != null)
        {
            dealFinish?.Invoke(path, abInfo.m_Obj, param1, param2, param3);
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
    /// 异步资源加载，针对ObjectManager的，（需要实例化对象的异步加载）
    /// </summary>
    public void AsyncLoadResource(string path, ResourceObj resObj, OnAsyncResObjFinish dealFinish, LoadResPriority priority, object param1 = null, object param2 = null, object param3 = null, uint crc = 0)
    {
        AssetBundleInfo abInfo = GetCacheAssetBundleInfo(resObj.m_Crc);
        if (abInfo != null)
        {
            resObj.m_AssetBundleInfo = abInfo;
            if (dealFinish != null)
            {
                dealFinish(path, resObj, param1, param2, param3);
            }

            return;
        }
        //判断是否在加载中
        AsyncLoadResUnit unit = null;

        //没有找到这个异步加载单位，或者这个异步加载单位是空
        if (!m_LoadingAssetDic.TryGetValue(resObj.m_Crc, out unit) || unit == null)
        {
            unit = m_AsyncLoadResParamPool.Spawn(true);
            unit.m_Crc = resObj.m_Crc;
            unit.m_Path = path;
            unit.m_Priority = priority;

            m_LoadingAssetDic.Add(resObj.m_Crc, unit);   //添加到正在异步加载的资源dic中

            m_loadingAssetList[(int)priority].Add(unit);   //按照加载优先级，添加到对应的正在异步加载的资源列表中
        }

        //往回调列表里面添加回调
        AsynCallBack callBack = m_AsynCallBackPool.Spawn(true);
        callBack.m_Param1 = param1;
        callBack.m_Param2 = param2;
        callBack.m_Param3 = param3;

        callBack.m_DealResObjFinish = dealFinish;
        callBack.m_ResObj = resObj;
        //往这个异步加载单位的回调列表中添加一个回调
        //可能多个地方加载同一份资源，这样做只加载一次资源，
        //加载完了后，根据回调列表依次返回这份资源
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
                AssetBundleInfo abInfo = null;
#if UNITY_EDITOR
                if (!m_LoadFromAssetBundle)
                {
                    if (loadintItem.m_Sprite) //判断是否是Sprite,因为Unity Object不能转换成Sprite
                    {
                        obj = LoadAssetByEditor<Sprite>(loadintItem.m_Path);
                    }
                    else
                    {
                        obj = LoadAssetByEditor<Object>(loadintItem.m_Path);
                    }

                    //模拟异步加载
                    yield return new WaitForSeconds(0.5f);

                    abInfo = AssetBundleManager.Instance.FindAssetBundleInfo(loadintItem.m_Crc);

                    if (abInfo == null)
                    {
                        abInfo = new AssetBundleInfo();
                        abInfo.m_Crc = loadintItem.m_Crc;
                    }
                }
#endif
                if (obj == null)
                {
                    abInfo = AssetBundleManager.Instance.LoadAssetBundleInfo(loadintItem.m_Crc);
                    if (abInfo != null && abInfo.m_AssetBundle != null)
                    {
                        AssetBundleRequest abRequest = null;
                        if (loadintItem.m_Sprite)   //判断是否是Sprite,因为Unity Object不能转换成Sprite
                        {
                            abRequest = abInfo.m_AssetBundle.LoadAssetAsync<Sprite>(abInfo.m_AssetName);
                        }
                        else
                        {
                            abRequest = abInfo.m_AssetBundle.LoadAssetAsync(abInfo.m_AssetName);
                        }
                        yield return abRequest;
                        if (abRequest.isDone)
                        {
                            obj = abRequest.asset;
                        }

                        lastYiledTime = System.DateTime.Now.Ticks;
                    }
                }

                CacheAssetBundleInfo(loadintItem.m_Path, ref abInfo, loadintItem.m_Crc, obj, callBackList.Count);

                //处理回调
                for (int j = 0; j < callBackList.Count; j++)
                {
                    AsynCallBack callBack = callBackList[j];

                    //实例化对象的回调
                    if (callBack != null && callBack.m_DealResObjFinish != null && callBack.m_ResObj != null)
                    {
                        ResourceObj tempResObj = callBack.m_ResObj;

                        tempResObj.m_AssetBundleInfo = abInfo;

                        callBack.m_DealResObjFinish(loadintItem.m_Path, tempResObj, callBack.m_Param1, callBack.m_Param2, callBack.m_Param3);

                        callBack.m_DealResObjFinish = null;

                        tempResObj = null;
                    }

                    //非实例化对象的回调
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
    #endregion

}

