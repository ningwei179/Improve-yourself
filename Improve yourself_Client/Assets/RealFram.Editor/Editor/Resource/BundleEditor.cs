/****************************************************
	文件：BundleEditor.cs
	作者：NingWei
	日期：2020/09/07 11:36   	
	功能：打AB包
*****************************************************/
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

public class BundleEditor
{
    private static string m_BundleTargetPath = Application.streamingAssetsPath;  //

    private static string ABCONFIGPATH = "Assets/RealFram.Editor/Editor/Resource/ABConfig.asset";

    private static string ABBYTEPATH = "Assets/GameData/Data/ABData/AssetBundleConfig.bytes";

    #region 这三个list，主要是给ABConfig.asset中配置的文件夹和Prefabs设置AssetBundle的打包属性
    //过滤的list，主要是单个prefab可能依赖其他资源，其依赖的资源可能已经在所有文件夹ab包dic里面了
    //过滤掉这种路径，避免接下来给所有文件夹和单个prefab设置AssetBundleName的时候出现重复设置
    public static List<string> m_AllFileAB = new List<string>();

    //key是ab包名，value是路径，所有文件夹ab包dic
    public static Dictionary<string, string> m_AllFileDir = new Dictionary<string, string>();

    //单个prefab的ab包dic,单个prefab可能依赖很多其他资源，记录他自己的路径以及他依赖的路径，过滤掉依赖路径中
    //已经在m_AllFileDir中添加过的路径
    public static Dictionary<string, List<string>> m_AllPrefabDir = new Dictionary<string, List<string>>();
    #endregion

    //储存所有的有效路径（ab包的路径）,生成配置文件用的
    private static List<string> m_ConfigFil = new List<string>();

    [MenuItem("Tools/打包")]
    public static void Build()
    {
        //清除所有字典或者list
        m_AllFileAB.Clear();
        m_AllFileDir.Clear();
        m_AllPrefabDir.Clear();
        m_ConfigFil.Clear();

        //加载ABConfig文件，该文件记录了所有需要打包的Prefab路径和文件夹路径
        ABConfig abConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(ABCONFIGPATH);

        //处理ABConfig中记录的需要打包的文件夹的打包
        foreach (ABConfig.FileDirAbName fileDir in abConfig.m_AllFileDirAB)
        {
            Debug.Log("abConfig.m_AllFileDirAB:" + fileDir.Path);
            if (m_AllFileDir.ContainsKey(fileDir.ABName))
            {
                Debug.Log("AB包配置名字重复，请检查");
            }
            else
            {
                //添加到文件夹ab包列表
                m_AllFileDir.Add(fileDir.ABName, fileDir.Path);
                //添加到过滤文件列表
                m_AllFileAB.Add(fileDir.Path);
                //添加到有效路径列表
                m_ConfigFil.Add(fileDir.Path);
            }
        }

        //处理ABConfig中记录的需要单个打包的prefab文件
        //找到abConfig.m_AllPrefabPath下的所有的Prefab文件
        string[] allStr = AssetDatabase.FindAssets("t:Prefab", abConfig.m_AllPrefabPath.ToArray());

        for (int i = 0; i < allStr.Length; ++i)
        {
            string path = AssetDatabase.GUIDToAssetPath(allStr[i]); //将获得的资源GUID转换成路径
            Debug.Log("Prefab:" + path);
            EditorUtility.DisplayCancelableProgressBar("查找Prefab", "Prefab:" + path, i * 0.1f / allStr.Length);

            //添加到有效路径列表
            m_ConfigFil.Add(path);

            //检查是否过滤，过滤掉依赖路径中已经在m_AllFileDir中添加过的路径
            if (!ContainAllFileAB(path))
            {
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                //注意！！！！获取这个prefab的所有依赖项的路径，这个数组包含了这个prefab本身的路径
                //所以，下面将allDependPath放入单个prefab的ab包字典的时候，天然就会包含这个prefab本身的路径
                string[] allDepend = AssetDatabase.GetDependencies(path);

                List<string> allDependPath = new List<string>();        //所有依赖文件的路径列表

                for (int j = 0; j < allDepend.Length; j++)
                {
                    Debug.Log("这个文件:" + obj.name + "==包括自己在内的所有依赖文件路径" + allDepend[j]);
                    //过滤掉依赖路径中已经在m_AllFileDir中添加过的路径 以及cs脚本
                    if (!ContainAllFileAB(allDepend[j]) && !allDepend[j].EndsWith(".cs"))
                    {
                        m_AllFileAB.Add(allDepend[j]);  //添加到过滤列表

                        allDependPath.Add(allDepend[j]);    //将依赖文件路径添加到所有依赖文件的路径列表中
                    }
                }

                if (m_AllPrefabDir.ContainsKey(obj.name))
                {
                    Debug.LogError("存在相同名字的prefab! 名字：" + obj.name);
                }
                else
                {
                    //将这个prefab的路径和他所依赖的文件的路径，存入单个prefab的ab包字典中
                    m_AllPrefabDir.Add(obj.name, allDependPath);
                }
            }
        }

        //unity打AssetBundle包，会将标记了assetBundleName的所有资源打包
        //给需要打成AssetBundle的所有文件设置assetBundleName，标记为要打AB包的文件
        foreach (string name in m_AllFileDir.Keys)
        {
            SetABName(name, m_AllFileDir[name]);
        }

        //给需要打成AssetBundle的单个Prefab资源设置assetBundleName，标记为要打AB包的文件
        foreach (string name in m_AllPrefabDir.Keys)
        {
            SetABName(name, m_AllPrefabDir[name]);
        }

        //执行打包代码
        BuildAssetBundle();

        //打完AB包后，把设置的AssetBundle的名字强制删除，避免meta文件被更改了
        string[] oldABNames = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < oldABNames.Length; i++)
        {
            AssetDatabase.RemoveAssetBundleName(oldABNames[i], true);
            EditorUtility.DisplayCancelableProgressBar("清楚AB包名称", "名字:" + oldABNames[i], i * 0.1f / oldABNames.Length);
        }
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 设置成AB包
    /// </summary>
    static void SetABName(string name, string path)
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath(path);
        if (assetImporter == null)
        {
            Debug.LogError("不存在此路径文件:" + path);
        }
        else
        {
            assetImporter.assetBundleName = name;
        }
    }

    /// <summary>
    /// 设置成AB包
    /// </summary>
    static void SetABName(string name, List<string> paths)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            SetABName(name, paths[i]);
        }
    }

    /// <summary>
    /// 打包assetBundle
    /// </summary>
    static void BuildAssetBundle()
    {
        //删除冗余的ab文件
        DeleteAB();

        //生成自己的配置表
        WriteData();

        //打包bundle
        BuildPipeline.BuildAssetBundles(m_BundleTargetPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
        //编辑器刷新
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 写入配置文件
    /// </summary>
    static void WriteData()
    {
        //记录所有即将打包的资源文件路径和它的包名，用来生成配置文件用的
        //key是资源文件路径，value是资源文件包名，属于哪一个ab包
        Dictionary<string, string> resPathDic = new Dictionary<string, string>();

        //获取所有的被标记为要打AB包的资源文件
        string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < allBundles.Length; ++i)
        {
            //根据assetBundle的名字获取assetBundle的全路径
            string[] allBundlePath = AssetDatabase.GetAssetPathsFromAssetBundle(allBundles[i]);
            for (int j = 0; j < allBundlePath.Length; j++)
            {
                //过滤assetBundle文件中的脚本文件
                if (allBundlePath[j].EndsWith(".cs"))
                    continue;
                //判断是否是有效路径
                if (ValidPath(allBundlePath[j]))
                    resPathDic.Add(allBundlePath[j], allBundles[i]);
                Debug.Log("此AB包：" + allBundles[i] + "==下面包含的资源文件路径" + allBundlePath[j]);
            }
        }


        //创建一个AB配置文件
        AssetBundleConfig config = new AssetBundleConfig();
        //记录AB文件列表
        config.ABList = new List<ABBase>();

        //遍历所有的资源文件字典，
        foreach (var path in resPathDic.Keys)
        {
            ABBase abBase = new ABBase();
            abBase.Path = path;
            abBase.Crc = Crc32.GetCrc32(path);  //Crc加密
            abBase.ABName = resPathDic[path];   //ab包名
            abBase.AssetName = path.Remove(0, path.LastIndexOf("/") + 1);   //截掉最后一个/，得到资源文件名称
            Debug.Log("abBase.AssetName"+ abBase.AssetName);
            abBase.ABDependce = new List<string>();
            //获取这个资源文件的所有依赖项
            string[] resDependce = AssetDatabase.GetDependencies(path);
            for (int i = 0; i < resDependce.Length; i++)
            {
                string resDependcePath = resDependce[i];    //依赖资源的路径
                //去掉这个资源文件自己和cs文件
                if(resDependcePath == path || path.EndsWith(".cs"))
                    continue;

                string abName = string.Empty;
                //通过依赖资源的路径获取这个依赖资源的包名
                if (resPathDic.TryGetValue(resDependcePath, out abName))
                {
                    //依赖资源的包名和自己本身资源在同一个包不管
                    if(abName == resPathDic[path])
                        continue;
                    //将这个包名添加到这个资源的依赖列表中，
                    //可能这个资源的多个依赖资源在同一个包中
                    //所以只添加一次就够了
                    if (!abBase.ABDependce.Contains(abName))
                    {
                        abBase.ABDependce.Add(abName);
                    }
                }
            }
            config.ABList.Add(abBase);
        }

        //写入xml
        string xmlPath = Application.dataPath + "/AssetBundleConfig.xml";
        if(File.Exists(xmlPath))
            File.Delete(xmlPath);
        FileStream fileStream = new FileStream(xmlPath,FileMode.Create,FileAccess.ReadWrite,FileShare.ReadWrite);
        StreamWriter sw = new StreamWriter(fileStream,System.Text.Encoding.UTF8);
        XmlSerializer xs = new XmlSerializer(config.GetType());
        xs.Serialize(sw,config);
        sw.Close();
        fileStream.Close();

        //写入二进制
        foreach (ABBase abBase in config.ABList)
        {
            abBase.Path = "";
        }

        //提前在ABBYTEPATH这个路径下创建一个2进制文件，将前面收集到的要打包的文件信息写入这个2进制文件
        //创建一个文件流对象
        FileStream fs = new FileStream(ABBYTEPATH, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        fs.Seek(0, SeekOrigin.Begin);
        fs.SetLength(0);
        
        //二进制序列化对象
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs, config);
        fs.Close();

        //刷新
        AssetDatabase.Refresh();

        //设置这个2进制文件也打包
        SetABName("assetbundleconfig", ABBYTEPATH);
    }

    /// <summary>
    /// 删除没用的AB包
    /// </summary>
    static void DeleteAB()
    {
        string[] allBundlesName = AssetDatabase.GetAllAssetBundleNames();   //获取所有的AssetBundle的名字
        DirectoryInfo direction = new DirectoryInfo(m_BundleTargetPath);    //获取该路径的文件夹
        FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);  //获取该文件夹下的所有文件
        for (int i = 0; i < files.Length; ++i)
        {
            //此文件存在于所有的AssetBundle文件数组中，并且他不是个meta文件，就不处理他
            if (ConationABName(files[i].Name, allBundlesName) || files[i].Name.EndsWith(".meta") || files[i].Name.EndsWith("manifest") ||
                files[i].Name.Equals("StreamingAssets") || files[i].Name.Equals("assetbundleconfig"))
            {
                continue;
            }
            else
            {
                Debug.Log("此AB包已经被删或者改名了："+files[i].Name);
                if (File.Exists(files[i].FullName))
                {
                    File.Delete(files[i].FullName);
                }
            }
        }
    }

    /// <summary>
    /// 检查name文件是否存在所有bundle文件数组中
    /// </summary>
    /// <param name="name"></param>
    /// <param name="strs"></param>
    /// <returns></returns>
    static bool ConationABName(string name,string[] strs)
    {
        for (int i = 0; i < strs.Length; i++)
        {
            if (name.Equals(strs[i]))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 检查是否包含在已经有的AB包里，用来做AB包冗余剔除
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    static bool ContainAllFileAB(string path)
    {
        for (int i = 0; i < m_AllFileAB.Count; i++)
        {
            //后面的剔除要加判断是因为某些情况会多余剔除
            //例：Assets/GameData/Test        
            //例：Assets/GameData/TestTT/a.prefab
            //解决文件夹相似造成的错误删除
            if (path == m_AllFileAB[i] || (path.Contains(m_AllFileAB[i]) && (path.Replace(m_AllFileAB[i],"")[0] == '/')))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 是否是有效路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    static bool ValidPath(string path)
    {
        for (int i = 0; i < m_ConfigFil.Count; ++i)
        {
            if (path.Contains(m_ConfigFil[i]))
            {
                return true;
            }

        }
        return false;
    }
}
