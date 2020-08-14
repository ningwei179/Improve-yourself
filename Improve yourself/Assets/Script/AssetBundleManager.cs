using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

/// <summary>
/// 记录AssetBundle资源块信息
/// </summary>
public class AssetBundleInfo
{
    /// <summary>
    /// 资源路径的Crc
    /// </summary>
    public uint m_Crc = 0;

    /// <summary>
    /// 该资源的名称
    /// </summary>
    public string m_AssetName = string.Empty;

    /// <summary>
    /// 该资源所在的AB包名字
    /// </summary>
    public string m_ABName = string.Empty;

    /// <summary>
    /// 该资源所依赖的其他AssetBundle资源
    /// </summary>
    public List<string> m_DependceAssetBundle = null;

    //--------------------------------------------上面是AssetBundle信息相关
    //--------------------------------------------下面是资源相关

    /// <summary>
    /// 资源的唯一ID
    /// </summary>
    public int m_Guid = 0;

    /// <summary>
    /// 真正AssetBundle资源
    /// </summary>
    public AssetBundle m_AssetBundle = null;

    /// <summary>
    /// 资源对象
    /// </summary>
    public Object m_Obj = null;

    /// <summary>
    /// 是否跳场景清掉资源
    /// </summary>
    public bool m_Clear = true;

    /// <summary>
    /// 资源最后所使用的时间
    /// </summary>
    public float m_LastUseTime = 0.0f;

    /// <summary>
    /// 引用计数
    /// </summary>
    public int m_RefCount = 0;
    public int RefCount
    {
        get { return m_RefCount; }
        set
        {
            m_RefCount = value;
            if(m_RefCount<0)
                Debug.LogError("refcount <0 "+m_RefCount+","+(m_Obj !=null ?m_Obj.name: "name is null"));
        }
    }
}

/// <summary>
/// 根据assetBundle路径转换的Crc加载AssetBundle的中间类
/// 主要做引用计数用的
/// </summary>
public class AssetBundleItem
{
    public AssetBundle assetBundle = null;
    public int RefCount;

    public void Rest()
    {
        assetBundle = null;
        RefCount = 0;
    }
}

public class AssetBundleManager : Singleton<AssetBundleManager>
{
    /// <summary>
    /// assetbundle资源信息和crc的映射表
    /// </summary>
    protected Dictionary<uint, AssetBundleInfo> m_AssetBundleInfoDic = new Dictionary<uint, AssetBundleInfo>();

    /// <summary>
    /// 储存已经加载的AB包，key 为crc
    /// </summary>
    protected Dictionary<uint, AssetBundleItem> m_AssetBundleItemDic = new Dictionary<uint, AssetBundleItem>();

    /// <summary>
    /// 创建一个AssetBundleItem的资源池，暂定AssetBundle资源上限是500个，不够可以再加
    /// </summary>
    protected ClassObjectPool<AssetBundleItem> m_AssetBundleItemPool = ObjectManager.Instance.GetOrCreateClassPool<AssetBundleItem>(500);

    /// <summary>
    /// 加载AB配置表
    /// </summary>
    /// <returns></returns>
    public bool LoadAssetBundleConfig()
    {
        string configPath = Application.streamingAssetsPath + "/assetbundleconfig";
        AssetBundle configAB = AssetBundle.LoadFromFile(configPath);
        TextAsset textAsset = configAB.LoadAsset<TextAsset>("assetbundleconfig");
        if (textAsset == null)
        {
            Debug.LogError("AssetBundleConfig is no exist!");
            return false;
        }
        //创建一个内存流
        MemoryStream stream = new MemoryStream(textAsset.bytes);
        //二进制序列化对象
        BinaryFormatter bf = new BinaryFormatter();

        AssetBundleConfig abConfig = (AssetBundleConfig)bf.Deserialize(stream);
        //关闭内存流
        stream.Close();

        for (int i = 0; i < abConfig.ABList.Count; ++i)
        {
            ABBase abBase = abConfig.ABList[i];

            AssetBundleInfo assetBundleInfo = new AssetBundleInfo();
            assetBundleInfo.m_Crc = abBase.Crc;
            assetBundleInfo.m_AssetName = abBase.AssetName;
            assetBundleInfo.m_ABName = abBase.ABName;
            assetBundleInfo.m_DependceAssetBundle = abBase.ABDependce;

            if (m_AssetBundleInfoDic.ContainsKey(assetBundleInfo.m_Crc))
            {
                Debug.LogError("重复的Crc : 资源名：" + assetBundleInfo.m_AssetName + " ab包名：" + assetBundleInfo.m_ABName);
            }
            else
            {
                m_AssetBundleInfoDic.Add(assetBundleInfo.m_Crc, assetBundleInfo);
            }
        }

        return true;
    }

    /// <summary>
    /// 加载AssetBundle，存储到AssetBundleInfo中,
    /// </summary>
    /// <param name="crc">AssetBundle的crc标记</param>
    /// <returns></returns>
    public AssetBundleInfo LoadAssetBundleInfo(uint crc)
    {
        AssetBundleInfo assetBundleInfo = null;

        if (!m_AssetBundleInfoDic.TryGetValue(crc, out assetBundleInfo) || assetBundleInfo == null)
        {
            Debug.LogError("bundle字典中没有找到item，或者找到了item是空，crc :" + crc);
            return assetBundleInfo;
        }

        //如果这个assetbundle资源块里面的资源不是空的
        if (assetBundleInfo.m_AssetBundle != null)
        {
            return assetBundleInfo;
        }

        //加载该资源块中的资源
        assetBundleInfo.m_AssetBundle = LoadAssetBundle(assetBundleInfo.m_ABName);

        //加载该资源块中的资源所依赖的资源
        if (assetBundleInfo.m_DependceAssetBundle != null)
        {
            for (int i = 0; i < assetBundleInfo.m_DependceAssetBundle.Count; i++)
            {
                LoadAssetBundle(assetBundleInfo.m_DependceAssetBundle[i]);
            }
        }

        return assetBundleInfo;
    }

    /// <summary>
    /// 加载单个AssetBundle,根据名称
    /// </summary>
    /// <param name="name">AssetBundle名称</param>
    /// <returns></returns>
    private AssetBundle LoadAssetBundle(string name)
    {
        AssetBundleItem item = null;
        uint crc = Crc32.GetCrc32(name);    //根据名称获取Crc值

        if (!m_AssetBundleItemDic.TryGetValue(crc, out item))
        {
            AssetBundle assetBundle = null;
            string fullPath = Application.streamingAssetsPath + "/" + name;

            //
            if (File.Exists(fullPath))
            {
                assetBundle = AssetBundle.LoadFromFile(fullPath);
            }

            if (assetBundle == null)
            {
                Debug.LogError("Load AssetBundle Error:" + fullPath);
            }

            item = m_AssetBundleItemPool.Spawn(true);
            item.assetBundle = assetBundle;
            item.RefCount++;
            m_AssetBundleItemDic.Add(crc,item);
        }
        else
        {
            item.RefCount++;
        }

        return item.assetBundle;
    }

    /// <summary>
    /// 释放一个资源块
    /// </summary>
    /// <param name="item"></param>
    public void ReleaseAssetBundle(AssetBundleInfo item)
    {
        if (item == null)
        {
            return;
        }

        //先卸载依赖资源
        if (item.m_DependceAssetBundle != null && item.m_DependceAssetBundle.Count > 0)
        {
            for (int i = 0; i < item.m_DependceAssetBundle.Count; i++)
            {
                UnloadAssetBundle(item.m_DependceAssetBundle[i]);
            }
        }

        //再卸载这个资源
        UnloadAssetBundle(item.m_ABName);
    }

    /// <summary>
    /// 卸载AssetBundle
    /// </summary>
    /// <param name="name"></param>
    private void UnloadAssetBundle(string name)
    {
        AssetBundleItem item = null;

        uint crc = Crc32.GetCrc32(name); //根据名称获取Crc值

        if (m_AssetBundleItemDic.TryGetValue(crc, out item) && item != null)
        {
            item.RefCount--;
            if (item.RefCount <= 0 && item.assetBundle!=null)
            {
                item.assetBundle.Unload(true);      //卸载掉AssetBundle
                item.Rest();                        //重置这个对象
                m_AssetBundleItemPool.Recycle(item);//回收到对象池子里
                m_AssetBundleItemDic.Remove(crc);   //从字典中删除这个资源
            }
        }
    }

    /// <summary>
    /// 根据Crc查找AssetBundleInfo
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    public AssetBundleInfo FindAssetBundleInfo(uint crc)
    {
        return m_AssetBundleInfoDic[crc];
    }
}




