/****************************************************
	文件：HotPatchManager.cs
	作者：NingWei
	日期：2020/09/22 11:20   	
	功能：热更新管理器
*****************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;
namespace Improve
{
    public class HotPatchManager : Singleton<HotPatchManager>
    {
        private MonoBehaviour m_Mono;
        //解压后的资源存放路径
        private string m_UnPackPath = Application.persistentDataPath + "/Origin";
        //从服务器下载的资源存放路径
        private string m_DownLoadPath = Application.persistentDataPath + "/DownLoad";
        //当前版本
        public string CurVersion;
        //当前包名
        public string m_CurPackName;
        #region 这俩个路径用来对比资源是否要热更
        //服务器下载的需要热更资源的配置
        private string m_ServerXmlPath = Application.persistentDataPath + "/ServerInfo.xml";
        //本地记录的需要热更资源的配置
        private string m_LocalXmlPath = Application.persistentDataPath + "/LocalInfo.xml";
        //服务器上的所有版本信息
        private ServerInfo m_ServerInfo;
        //本地的所有版本信息
        private ServerInfo m_LocalInfo;
        //当前版本需要信息
        private VersionInfo m_GameVersion;
        //当前版本需要热更的信息
        public Pathces CurrentPatches { get; private set; }
        #endregion
        //所有热更的东西
        private Dictionary<string, Patch> m_HotFixDic = new Dictionary<string, Patch>();
        //所有需要下载的东西
        private List<Patch> m_DownLoadList = new List<Patch>();
        //所有需要下载的东西的Dic
        private Dictionary<string, Patch> m_DownLoadDic = new Dictionary<string, Patch>();
        //服务器上的资源名对应的MD5，用于下载后MD5校验
        private Dictionary<string, string> m_DownLoadMD5Dic = new Dictionary<string, string>();
        //计算需要解压的文件
        private List<string> m_UnPackedList = new List<string>();
        //原包记录的MD5码
        private Dictionary<string, ABMD5Base> m_PackedMd5 = new Dictionary<string, ABMD5Base>();
        //服务器列表获取错误回调
        public Action ServerInfoError;
        //文件下载出错回调
        public Action<string> ItemError;
        //下载完成回调
        public Action LoadOver;
        //储存已经下载的资源
        public List<Patch> m_AlreadyDownList = new List<Patch>();
        //是否开始下载
        public bool StartDownload = false;
        //尝试重新下载次数
        private int m_TryDownCount = 0;
        private const int DOWNLOADCOUNT = 4;
        //当前正在下载的资源
        private DownLoadAssetBundle m_CurDownload = null;

        // 需要下载的资源总个数
        public int LoadFileCount { get; set; } = 0;
        // 需要下载资源的总大小 KB
        public float LoadSumSize { get; set; } = 0;
        //是否开始解压
        public bool StartUnPack = false;
        //解压文件总大小
        public float UnPackSumSize { get; set; } = 0;
        //已解压大小
        public float AlreadyUnPackSize { get; set; } = 0;

        private WaitForSeconds wait = new WaitForSeconds(3f);

        public void Init(MonoBehaviour mono)
        {
            m_Mono = mono;
            ReadMD5();
        }

        /// <summary>
        /// 读取本地资源MD5码
        /// </summary>
        void ReadMD5()
        {
            m_PackedMd5.Clear();
            TextAsset md5 = Resources.Load<TextAsset>("ABMD5");
            if (md5 == null)
            {
                Debug.LogError("未读取到本地MD5");
                return;
            }

            using (MemoryStream stream = new MemoryStream(md5.bytes))
            {
                BinaryFormatter bf = new BinaryFormatter();
                ABMD5 abmd5 = bf.Deserialize(stream) as ABMD5;
                foreach (ABMD5Base abmd5Base in abmd5.ABMD5List)
                {
                    m_PackedMd5.Add(abmd5Base.Name, abmd5Base);
                }
            }
        }

        /// <summary>
        /// Android平台不能直接通过FileStream从streamingAssetsPath加载文件，
        /// 所以将streamingAssetsPath路径下的文件拷贝到其他文件夹
        /// 计算需要解压的文件
        /// </summary>
        /// <returns></returns>
        public bool ComputeUnPackFile()
        {
#if UNITY_ANDROID
        if (!Directory.Exists(m_UnPackPath))
        {
            Directory.CreateDirectory(m_UnPackPath);
        }
        m_UnPackedList.Clear();
        foreach (string fileName in m_PackedMd5.Keys)
        {
            string filePath = m_UnPackPath + "/" + fileName;
            if (File.Exists(filePath))
            {
                string md5 = MD5Manager.Instance.BuildFileMd5(filePath);
                if (m_PackedMd5[fileName].Md5 != md5)
                {
                    m_UnPackedList.Add(fileName);
                }
            }
            else
            {
                m_UnPackedList.Add(fileName);
            }
        }

        foreach (string fileName in m_UnPackedList)
        {
            if (m_PackedMd5.ContainsKey(fileName))
            {
                UnPackSumSize += m_PackedMd5[fileName].Size;
            }
        }

        return m_UnPackedList.Count > 0;
#else
            return false;
#endif
        }

        /// <summary>
        /// 获取解压进度
        /// </summary>
        /// <returns></returns>
        public float GetUnpackProgress()
        {
            return AlreadyUnPackSize / UnPackSumSize;
        }

        /// <summary>
        /// 开始解压文件
        /// </summary>
        /// <param name="callBack"></param>
        public void StartUnackFile(Action callBack)
        {
            StartUnPack = true;
            m_Mono.StartCoroutine(UnPackToPersistentDataPath(callBack));
        }

        /// <summary>
        /// 将包里的原始资源解压到本地
        /// </summary>
        /// <param name="callBack"></param>
        /// <returns></returns>
        IEnumerator UnPackToPersistentDataPath(Action callBack)
        {
            foreach (string fileName in m_UnPackedList)
            {
                UnityWebRequest unityWebRequest = UnityWebRequest.Get(Application.streamingAssetsPath + "/" + fileName);
                unityWebRequest.timeout = 30;
                yield return unityWebRequest.SendWebRequest();
                if (unityWebRequest.isNetworkError)
                {
                    Debug.Log("UnPack Error" + unityWebRequest.error);
                }
                else
                {
                    byte[] bytes = unityWebRequest.downloadHandler.data;
                    FileTool.CreateFile(m_UnPackPath + "/" + fileName, bytes);
                }

                if (m_PackedMd5.ContainsKey(fileName))
                {
                    AlreadyUnPackSize += m_PackedMd5[fileName].Size;
                }

                unityWebRequest.Dispose();
            }

            yield return wait;
            callBack?.Invoke();

            StartUnPack = false;
        }

        /// <summary>
        /// 计算AB包路径
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string ComputeABPath(string name)
        {
            Patch patch = null;
            m_HotFixDic.TryGetValue(name, out patch);
            if (patch != null)
            {
                return m_DownLoadPath + "/" + name;
            }
            return "";
        }

        /// <summary>
        /// 检测版本是否热更
        /// </summary>
        /// <param name="hotCallBack"></param>
        public void CheckVersion(Action<bool> hotCallBack = null)
        {
            m_TryDownCount = 0;
            m_HotFixDic.Clear();

            m_Mono.StartCoroutine(ReadXml(() =>
            {
                if (m_ServerInfo == null)
                {
                    ServerInfoError?.Invoke();
                    return;
                }

                foreach (VersionInfo version in m_ServerInfo.GameVersion)
                {
                //从服务器所有版信息中找到游戏的当前版本信息
                if (version.Version == CurVersion)
                    {
                        m_GameVersion = version;
                        break;
                    }
                }

            //从当前版本信息中,获取所有热更包信息
            GetHotAB();

            //检查本地所有版本信息与服务器所有版本信息，看是否要更新本地的所有版本信息xml文件
            if (CheckLocalAndServerPatch())
                {
                //计算要下载的资源
                ComputeDownload();

                //更新本地的所有版本信息xml文件
                if (File.Exists(m_ServerXmlPath))
                    {
                        if (File.Exists(m_LocalXmlPath))
                        {
                            File.Delete(m_LocalXmlPath);
                        }
                        File.Move(m_ServerXmlPath, m_LocalXmlPath);
                    }
                }
                else
                {
                //计算要下载的资源
                ComputeDownload();
                }

                LoadFileCount = m_DownLoadList.Count;
                LoadSumSize = m_DownLoadList.Sum(x => x.Size);
                hotCallBack?.Invoke(m_DownLoadList.Count > 0);
            }));
        }

        /// <summary>
        /// 检查本地所有版本信息与服务器所有版本信息，看是否要更新本地的所有版本信息xml文件
        /// </summary>
        /// <returns></returns>
        bool CheckLocalAndServerPatch()
        {
            //没有本地所有版本信息xml文件，需要更新xml文件
            if (!File.Exists(m_LocalXmlPath))
                return true;

            //读取本地所有版本信息，xml转换成类信息
            m_LocalInfo = BinarySerializeOpt.XmlDeserialize(m_LocalXmlPath, typeof(ServerInfo)) as ServerInfo;

            VersionInfo localGameVesion = null;
            if (m_LocalInfo != null)
            {
                foreach (VersionInfo version in m_LocalInfo.GameVersion)
                {
                    //从本地所有版信息中找到游戏的当前版本信息
                    if (version.Version == CurVersion)
                    {
                        localGameVesion = version;
                        break;
                    }
                }
            }
            //本地当前版本信息和服务器不一致，需要更新本地所有版本信息xml文件
            if (localGameVesion != null && m_GameVersion.Pathces != null && localGameVesion.Pathces != null && m_GameVersion.Pathces.Length > 0 && m_GameVersion.Pathces[m_GameVersion.Pathces.Length - 1].Version != localGameVesion.Pathces[localGameVesion.Pathces.Length - 1].Version)
                return true;

            return false;
        }

        /// <summary>
        /// 从服务器下载游戏的所有版本信息
        /// </summary>
        /// <param name="callBack"></param>
        /// <returns></returns>
        IEnumerator ReadXml(Action callBack)
        {
            string xmlUrl = FrameConstr.m_ResServerIp + "ServerInfo.xml";

            UnityWebRequest webRequest = UnityWebRequest.Get(xmlUrl);
            webRequest.timeout = 30;
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log("Download Error" + webRequest.error);
            }
            else
            {
                //将从服务器下载的数据保存为热更资源配置xml
                FileTool.CreateFile(m_ServerXmlPath, webRequest.downloadHandler.data);
                if (File.Exists(m_ServerXmlPath))
                {
                    //将xml转换成服务器上的所有版本信息
                    m_ServerInfo = BinarySerializeOpt.XmlDeserialize(m_ServerXmlPath, typeof(ServerInfo)) as ServerInfo;
                }
                else
                {
                    Debug.LogError("热更配置读取错误！");
                }
            }

            if (callBack != null)
            {
                callBack();
            }
        }

        /// <summary>
        /// 获取所有热更包信息
        /// </summary>
        void GetHotAB()
        {
            if (m_GameVersion != null && m_GameVersion.Pathces != null && m_GameVersion.Pathces.Length > 0)
            {
                Pathces lastPatches = m_GameVersion.Pathces[m_GameVersion.Pathces.Length - 1];
                if (lastPatches != null && lastPatches.Files != null)
                {
                    foreach (Patch patch in lastPatches.Files)
                    {
                        m_HotFixDic.Add(patch.Name, patch);
                    }
                }
            }
        }

        /// <summary>
        /// 计算下载的资源
        /// </summary>
        void ComputeDownload()
        {
            m_DownLoadList.Clear();
            m_DownLoadDic.Clear();
            m_DownLoadMD5Dic.Clear();
            //当前版本信息存在，热更补丁存在，热更补丁长度大于0
            if (m_GameVersion != null && m_GameVersion.Pathces != null && m_GameVersion.Pathces.Length > 0)
            {
                //设定的当前版本最后一个热补丁信息，就是当前需要热更的补丁信息
                CurrentPatches = m_GameVersion.Pathces[m_GameVersion.Pathces.Length - 1];

                if (CurrentPatches.Files != null && CurrentPatches.Files.Count > 0)
                {
                    foreach (Patch patch in CurrentPatches.Files)
                    {
                        if ((Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) && patch.Platform.Contains("StandaloneWindows64"))
                        {
                            AddDownLoadList(patch);
                        }
                        else if ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsEditor) && patch.Platform.Contains("Android"))
                        {
                            AddDownLoadList(patch);
                        }
                        else if ((Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.WindowsEditor) && patch.Platform.Contains("IOS"))
                        {
                            AddDownLoadList(patch);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 添加到下载列表
        /// </summary>
        /// <param name="patch"></param>
        void AddDownLoadList(Patch patch)
        {
            string filePath = m_DownLoadPath + "/" + patch.Name;
            //存在这个文件时进行对比看是否与服务器MD5码一致，不一致放到下载队列，如果不存在直接放入下载队列
            if (File.Exists(filePath))
            {
                string md5 = MD5Manager.Instance.BuildFileMd5(filePath);
                if (patch.Md5 != md5)
                {
                    m_DownLoadList.Add(patch);
                    m_DownLoadDic.Add(patch.Name, patch);
                    m_DownLoadMD5Dic.Add(patch.Name, patch.Md5);
                }
            }
            else
            {
                m_DownLoadList.Add(patch);
                m_DownLoadDic.Add(patch.Name, patch);
                m_DownLoadMD5Dic.Add(patch.Name, patch.Md5);
            }
        }

        /// <summary>
        /// 获取下载进度
        /// </summary>
        /// <returns></returns>
        public float GetProgress()
        {
            return GetLoadSize() / LoadSumSize;
        }

        /// <summary>
        /// 获取已经下载总大小
        /// </summary>
        /// <returns></returns>
        public float GetLoadSize()
        {
            float alreadySize = m_AlreadyDownList.Sum(x => x.Size);
            float curAlreadySize = 0;
            if (m_CurDownload != null)
            {
                Patch patch = FindPatchByGamePath(m_CurDownload.FileName);
                if (patch != null && !m_AlreadyDownList.Contains(patch))
                {
                    curAlreadySize = m_CurDownload.GetProcess() * patch.Size;
                }
            }

            return alreadySize + curAlreadySize;
        }

        /// <summary>
        /// 开始下载AB包
        /// </summary>
        /// <param name="callBack"></param>
        /// <returns></returns>
        public IEnumerator StartDownLoadAB(Action callBack, List<Patch> allPatch = null)
        {
            m_AlreadyDownList.Clear();
            StartDownload = true;
            if (allPatch == null)
            {
                allPatch = m_DownLoadList;
            }
            if (!Directory.Exists(m_DownLoadPath))
            {
                Directory.CreateDirectory(m_DownLoadPath);
            }

            List<DownLoadAssetBundle> downLoadAssetBundles = new List<DownLoadAssetBundle>();
            foreach (Patch patch in allPatch)
            {
                downLoadAssetBundles.Add(new DownLoadAssetBundle(patch.Url, m_DownLoadPath));
            }

            foreach (DownLoadAssetBundle downLoad in downLoadAssetBundles)
            {
                m_CurDownload = downLoad;
                yield return m_Mono.StartCoroutine(downLoad.Download());
                Patch patch = FindPatchByGamePath(downLoad.FileName);
                if (patch != null)
                {
                    m_AlreadyDownList.Add(patch);
                }
                downLoad.Destory();
            }

            //MD5码校验,如果校验没通过，自动重新下载没通过的文件，重复下载计数，达到一定次数后，反馈某某文件下载失败

            //存储MD5码错误的补丁文件，这些要重新下载
            List<Patch> downLoadList = new List<Patch>();

            foreach (DownLoadAssetBundle downLoad in downLoadAssetBundles)
            {
                string md5 = "";
                if (m_DownLoadMD5Dic.TryGetValue(downLoad.FileName, out md5))
                {
                    Debug.Log("下载的文件的MD5:" + MD5Manager.Instance.BuildFileMd5(downLoad.SaveFilePath));
                    Debug.Log("服务器上该文件的MD5:" + md5);
                    if (MD5Manager.Instance.BuildFileMd5(downLoad.SaveFilePath) != md5)
                    {
                        Debug.Log(string.Format("此文件{0}MD5校验失败，即将重新下载", downLoad.FileName));
                        Patch patch = FindPatchByGamePath(downLoad.FileName);
                        if (patch != null)
                        {
                            downLoadList.Add(patch);
                        }
                    }
                }
            }

            //全部文件都是正确的
            if (downLoadList.Count <= 0)
            {
                m_DownLoadMD5Dic.Clear();
                if (callBack != null)
                {
                    yield return wait;
                    StartDownload = false;
                    callBack();
                }
                if (LoadOver != null)
                {
                    LoadOver();
                }
            }
            else
            {
                if (m_TryDownCount >= DOWNLOADCOUNT)
                {
                    string allName = "";
                    StartDownload = false;
                    foreach (Patch patch in downLoadList)
                    {
                        allName += patch.Name + ";";
                    }
                    Debug.LogError("资源重复下载4次MD5校验都失败，请检查资源" + allName);
                    ItemError?.Invoke(allName);
                }
                else
                {
                    m_TryDownCount++;
                    m_DownLoadMD5Dic.Clear();
                    foreach (Patch patch in downLoadList)
                    {
                        m_DownLoadMD5Dic.Add(patch.Name, patch.Md5);
                    }
                    //自动重新下载校验失败的文件
                    m_Mono.StartCoroutine(StartDownLoadAB(callBack, downLoadList));
                }
            }

        }

        /// <summary>
        /// Md5码校验
        /// </summary>
        /// <param name="downLoadAssets"></param>
        /// <param name="callBack"></param>
        void VerifyMD5(List<DownLoadAssetBundle> downLoadAssets, Action callBack)
        {
            //存储MD5码错误的补丁文件，这些要重新下载
            List<Patch> downLoadList = new List<Patch>();

            foreach (DownLoadAssetBundle downLoad in downLoadAssets)
            {
                string md5 = "";
                if (m_DownLoadMD5Dic.TryGetValue(downLoad.FileName, out md5))
                {
                    Debug.Log("下载的文件的MD5:" + MD5Manager.Instance.BuildFileMd5(downLoad.SaveFilePath));
                    Debug.Log("服务器上该文件的MD5:" + md5);
                    if (MD5Manager.Instance.BuildFileMd5(downLoad.SaveFilePath) != md5)
                    {
                        Debug.Log(string.Format("此文件{0}MD5校验失败，即将重新下载", downLoad.FileName));
                        Patch patch = FindPatchByGamePath(downLoad.FileName);
                        if (patch != null)
                        {
                            downLoadList.Add(patch);
                        }
                    }
                }
            }

            //全部文件都是正确的
            if (downLoadList.Count <= 0)
            {
                m_DownLoadMD5Dic.Clear();
                if (callBack != null)
                {
                    StartDownload = false;
                    callBack();
                }
                if (LoadOver != null)
                {
                    LoadOver();
                }
            }
            else
            {
                if (m_TryDownCount >= DOWNLOADCOUNT)
                {
                    string allName = "";
                    StartDownload = false;
                    foreach (Patch patch in downLoadList)
                    {
                        allName += patch.Name + ";";
                    }
                    Debug.LogError("资源重复下载4次MD5校验都失败，请检查资源" + allName);
                    ItemError?.Invoke(allName);
                }
                else
                {
                    m_TryDownCount++;
                    m_DownLoadMD5Dic.Clear();
                    foreach (Patch patch in downLoadList)
                    {
                        m_DownLoadMD5Dic.Add(patch.Name, patch.Md5);
                    }
                    //自动重新下载校验失败的文件
                    m_Mono.StartCoroutine(StartDownLoadAB(callBack, downLoadList));
                }
            }
        }

        /// <summary>
        /// 根据名字查找对象的热更Patch
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Patch FindPatchByGamePath(string name)
        {
            Patch patch = null;
            m_DownLoadDic.TryGetValue(name, out patch);
            return patch;
        }
    }


    public class FileTool
    {
        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="bytes"></param>
        public static void CreateFile(string filePath, byte[] bytes)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            FileInfo file = new FileInfo(filePath);
            Stream stream = file.Create();
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
            stream.Dispose();
        }
    }
}