/****************************************************
	文件：AssetBundleManager.cs
	作者：NingWei
	日期：2020/09/19 15:37   	
	功能：AB管理器
*****************************************************/
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
namespace Improve
{
    /// <summary>
    /// 记录AssetBundle资源块信息
    /// </summary>
    public class AssetBundleInfo : CustomYieldInstruction
    {
        //--------------------------------------------基本信息
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


        //--------------------------------------------资源相关
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
                if (m_RefCount < 0)
                    Debug.LogError("refcount <0 " + m_RefCount + "," + (m_Obj != null ? m_Obj.name : "name is null"));
            }
        }

        //--------------------------------------------异步加载相关
        /// <summary>
        /// 异步加载的协程等待重写
        /// </summary>
        public override bool keepWaiting => !_CheckIsDone();

        private bool _isDone;
        /// <summary>
        /// 操作是否完成。
        /// </summary>
        /// <returns></returns>
        public bool isDone => _CheckIsDone();

        /// <summary>
        /// 重置下载完成
        /// </summary>
        public void ResetDone()
        {
            _isDone = false;
        }

        /// <summary>
        /// 异步加载Bundle的请求列表
        /// </summary>
        public List<AssetBundleCreateRequest> m_Requests;

        // 检查异步加载是否完成
        private bool _CheckIsDone()
        {
            if (_isDone)
            {
                return _isDone;
            }

            // 如果有任意一个请求未完成，则认为整体加载未完成
            if (m_Requests != null && m_Requests.Count != 0)
            {
                foreach (var req in m_Requests)
                {
                    if (req.isDone == false)
                    {
                        return _isDone;
                    }
                }
            }

            _isDone = true;

            // 收集加载到的AssetBundle
            if (m_Requests != null && m_Requests.Count != 0)
            {
                m_AssetBundle = m_Requests[0].assetBundle;  //第一个异步请求的是自己的资源

                AssetBundleManager.Instance.SaveAssetBundle(m_Requests[0].assetBundle);
                //剩下的是依赖的资源
                for (int i = 1; i < m_Requests.Count; i++)
                {
                    AssetBundleManager.Instance.SaveAssetBundle(m_Requests[i].assetBundle);
                }
            }

            //清空异步请求
            m_Requests.Clear();

            return _isDone;
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
        //AB包是否是加密过的,默认是加密过的,
        public bool Encrypt = false;

        protected string m_ABConfigABName = "assetbundleconfig";
        /// <summary>
        /// assetbundle资源信息和crc的映射表
        /// </summary>
        protected Dictionary<uint, AssetBundleInfo> m_AssetBundleInfoDic = new Dictionary<uint, AssetBundleInfo>();

        /// <summary>
        /// 储存已经加载的AB包，key 为crc
        /// </summary>
        internal Dictionary<uint, AssetBundleItem> m_AssetBundleItemDic = new Dictionary<uint, AssetBundleItem>();

        /// <summary>
        /// 创建一个AssetBundleItem的资源池，暂定AssetBundle资源上限是500个，不够可以再加
        /// </summary>
        internal ClassObjectPool<AssetBundleItem> m_AssetBundleItemPool = ObjectManager.Instance.GetOrCreateClassPool<AssetBundleItem>(500);

        /// <summary>
        /// AB包配置文件
        /// </summary>
        AssetBundle configAB;

        protected string ABLoadPath
        {
            get
            {
#if UNITY_ANDROID
            return Application.persistentDataPath + "/Origin/";
#else
                return Application.streamingAssetsPath + "/";
#endif
            }
        }

        /// <summary>
        /// 加载AB配置表
        /// </summary>
        /// <returns></returns>
        public bool LoadAssetBundleConfig(bool isInit = true)
        {
            //#if UNITY_EDITOR
            //        if (!ResourceManager.Instance.m_LoadFromAssetBundle)
            //            return false;
            //#endif
            //初始化时候的ab配置文件在streamingAssets文件夹中
            string configPath = Application.streamingAssetsPath + "/" + m_ABConfigABName;
            //非初始化，检查热更后进来的
            if (!isInit)
            {
                //热更完毕后检查配置文件是否热更了
                string hotABPath = HotPatchManager.Instance.ComputeABPath(m_ABConfigABName);
                if (string.IsNullOrEmpty(hotABPath))
                    return true;
                else
                {
                    //文件改变了，卸载本地的ab包，从热更路径下下载新的ab包
                    if (configAB != null)
                    {
                        configAB.Unload(true);
                        configPath = hotABPath;
                    }
                }
            }
            m_AssetBundleInfoDic.Clear();

            //加密后的ab包要先解密，才能加载
            if (Encrypt)
            {
                byte[] bytes = AES.AESFileByteDecrypt(configPath, FrameConstr.m_ABSecretKey);

                configAB = AssetBundle.LoadFromMemory(bytes);
            }
            else
            {
                configAB = AssetBundle.LoadFromFile(configPath);
            }

            TextAsset textAsset = configAB.LoadAsset<TextAsset>(m_ABConfigABName);
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

                AssetBundleInfo abInfo = new AssetBundleInfo();
                abInfo.m_Crc = abBase.Crc;
                abInfo.m_AssetName = abBase.AssetName;
                abInfo.m_ABName = abBase.ABName;
                abInfo.m_DependceAssetBundle = abBase.ABDependce;

                if (m_AssetBundleInfoDic.ContainsKey(abInfo.m_Crc))
                {
                    Debug.LogError("重复的Crc : 资源名：" + abInfo.m_AssetName + " ab包名：" + abInfo.m_ABName);
                }
                else
                {
                    m_AssetBundleInfoDic.Add(abInfo.m_Crc, abInfo);
                }
            }

            return true;
        }

        /// <summary>
        /// 加载AssetBundle，存储到AssetBundleInfo.m_AssetBundle中,
        /// </summary>
        /// <param name="crc">AssetBundle的crc标记</param>
        /// <returns></returns>
        public AssetBundleInfo LoadAssetBundleInfo(uint crc)
        {
            AssetBundleInfo abInfo = null;

            if (!m_AssetBundleInfoDic.TryGetValue(crc, out abInfo) || abInfo == null)
            {
                Debug.LogError("bundle字典中没有找到item，或者找到了item是空，crc :" + crc);
                return abInfo;
            }

            //加载该资源块中的资源
            abInfo.m_AssetBundle = LoadAssetBundle(abInfo.m_ABName);

            //加载该资源块中的资源所依赖的资源
            if (abInfo.m_DependceAssetBundle != null)
            {
                for (int i = 0; i < abInfo.m_DependceAssetBundle.Count; i++)
                {
                    LoadAssetBundle(abInfo.m_DependceAssetBundle[i]);
                }
            }

            return abInfo;
        }

        /// <summary>
        /// 加载单个AssetBundle,根据名称
        /// </summary>
        /// <param name="name">AssetBundle名称</param>
        /// <returns></returns>
        private AssetBundle LoadAssetBundle(string name)
        {
            AssetBundleItem abItem = null;
            uint crc = Crc32.GetCrc32(name);    //根据名称获取Crc值

            if (!m_AssetBundleItemDic.TryGetValue(crc, out abItem))
            {
                AssetBundle assetBundle = null;

                //热更下来的AB包放在Application.persistentDataPath + "/DownLoad" 目录下

                //打包到游戏包里的AB包放在Application.streamingAssetsPath 目录下

                //所以要根据资源文件名称来判断是否是热更的AB包

                string hotAbPath = HotPatchManager.Instance.ComputeABPath(name);

                string fullPath = string.IsNullOrEmpty(hotAbPath) ? ABLoadPath + name : hotAbPath;

                //加密后的ab包要先解密，才能加载
                if (Encrypt)
                {
                    byte[] bytes = AES.AESFileByteDecrypt(fullPath, FrameConstr.m_ABSecretKey);

                    assetBundle = AssetBundle.LoadFromMemory(bytes);
                }
                else
                {
                    assetBundle = AssetBundle.LoadFromFile(fullPath);
                }

                if (assetBundle == null)
                {
                    Debug.LogError("Load AssetBundle Error:" + fullPath);
                }

                abItem = m_AssetBundleItemPool.Spawn(true);
                abItem.assetBundle = assetBundle;
                abItem.RefCount++;
                m_AssetBundleItemDic.Add(crc, abItem);
            }
            else
            {
                abItem.RefCount++;
            }

            return abItem.assetBundle;
        }

        public AssetBundleInfo LoadAssetBundleInfoAsync(uint crc)
        {
            AssetBundleInfo abInfo = null;

            if (!m_AssetBundleInfoDic.TryGetValue(crc, out abInfo) || abInfo == null)
            {
                Debug.LogError("bundle字典中没有找到item，或者找到了item是空，crc :" + crc);
                return abInfo;
            }

            //如果这个assetbundle资源块里面的资源不是空的
            if (abInfo.m_AssetBundle != null)
            {
                return abInfo;
            }

            //初始化异步加载请求数组
            if (abInfo.m_Requests == null)
                abInfo.m_Requests = new List<AssetBundleCreateRequest>();

            //收集，加载自己的异步请求
            AssetBundleCreateRequest abRequest = null;

            AssetBundle ab = null;

            if (GetAssetBundleRequest(abInfo.m_ABName, out abRequest, out ab))
            {
                abInfo.m_AssetBundle = ab;
            }
            else
            {
                abInfo.m_Requests.Add(abRequest);
            }

            //收集，该资源块所依赖资源的异步加载请求
            if (abInfo.m_DependceAssetBundle != null)
            {
                for (int i = 0; i < abInfo.m_DependceAssetBundle.Count; i++)
                {
                    if (!GetAssetBundleRequest(abInfo.m_DependceAssetBundle[i], out abRequest, out ab))
                    {
                        abInfo.m_Requests.Add(abRequest);
                    }
                }
            }

            //异步请求数量不为0，重置下载完成条件
            if (abInfo.m_Requests.Count != 0)
                abInfo.ResetDone();

            return abInfo;
        }

        /// <summary>
        /// 收集异步请求
        /// </summary>
        /// <returns></returns>
        private bool GetAssetBundleRequest(string name, out AssetBundleCreateRequest abRequest, out AssetBundle ab)
        {
            abRequest = null;
            ab = null;

            AssetBundleItem abItem = null;
            uint crc = Crc32.GetCrc32(name);    //根据名称获取Crc值
                                                //没有加载过这个AB包，创建异步加载请求
            if (!m_AssetBundleItemDic.TryGetValue(crc, out abItem))
            {
                //热更下来的AB包放在Application.persistentDataPath + "/DownLoad" 目录下

                //打包到游戏包里的AB包放在Application.streamingAssetsPath 目录下

                //所以要根据资源文件名称来判断是否是热更的AB包
                string hotAbPath = HotPatchManager.Instance.ComputeABPath(name);

                string fullPath = string.IsNullOrEmpty(hotAbPath) ? ABLoadPath + name : hotAbPath;

                //加密后的ab包要先解密，才能加载，但是异步的时候感觉就没有异步的效果了额
                if (Encrypt)
                {
                    byte[] bytes = AES.AESFileByteDecrypt(fullPath, FrameConstr.m_ABSecretKey);

                    abRequest = AssetBundle.LoadFromMemoryAsync(bytes);
                }
                else
                {
                    abRequest = AssetBundle.LoadFromFileAsync(fullPath);
                }

                abItem = m_AssetBundleItemPool.Spawn(true);
                abItem.RefCount++;
                m_AssetBundleItemDic.Add(crc, abItem);
                return false;
            }
            else
            {
                ab = abItem.assetBundle;
                abItem.RefCount++;
                return true;
            }
        }

        /// <summary>
        /// 保存异步加载获取的AssetBundle
        /// </summary>
        /// <param name="name">AssetBundle名称</param>
        /// <returns></returns>
        public void SaveAssetBundle(AssetBundle assetBundle)
        {
            AssetBundleItem abItem = null;

            uint crc = Crc32.GetCrc32(assetBundle.name);    //根据名称获取Crc值

            if (m_AssetBundleItemDic.TryGetValue(crc, out abItem) && abItem != null)
            {
                abItem.assetBundle = assetBundle;
            }
            else
            {
                Debug.LogError("异步加载资源后，保存资源到字典时没有找到对应Key：" + assetBundle.name);
            }
        }

        /// <summary>
        /// 释放一个资源块
        /// </summary>
        /// <param name="abInfo"></param>
        public void ReleaseAssetBundle(AssetBundleInfo abInfo)
        {
            if (abInfo == null)
            {
                return;
            }

            //先卸载依赖资源
            if (abInfo.m_DependceAssetBundle != null && abInfo.m_DependceAssetBundle.Count > 0)
            {
                for (int i = 0; i < abInfo.m_DependceAssetBundle.Count; i++)
                {
                    UnloadAssetBundle(abInfo.m_DependceAssetBundle[i]);
                }
            }

            //再卸载这个资源
            UnloadAssetBundle(abInfo.m_ABName);
        }

        /// <summary>
        /// 卸载AssetBundle
        /// </summary>
        /// <param name="name"></param>
        private void UnloadAssetBundle(string name)
        {
            AssetBundleItem abItem = null;

            uint crc = Crc32.GetCrc32(name); //根据名称获取Crc值

            if (m_AssetBundleItemDic.TryGetValue(crc, out abItem) && abItem != null)
            {
                abItem.RefCount--;
                if (abItem.RefCount <= 0 && abItem.assetBundle != null)
                {
                    abItem.assetBundle.Unload(true);      //卸载掉AssetBundle
                    abItem.Rest();                        //重置这个对象
                    m_AssetBundleItemPool.Recycle(abItem);//回收到对象池子里
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
            AssetBundleInfo abInfo = null;
            m_AssetBundleInfoDic.TryGetValue(crc, out abInfo);
            return abInfo;
        }
    }
}



