/****************************************************
	文件：ABConfig.cs
	作者：NingWei
	日期：2020/09/07 11:35   	
	功能：ABConfig 序列化文件
*****************************************************/
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// unity Assets序列化
/// 添加这个可以创建unity 序列化文件
/// </summary>
[CreateAssetMenu(fileName = "ABConfig", menuName = "CreateABConfig"), DefaultExecutionOrder(0)]
public class ABConfig : ScriptableObject
{
    //单个文件所在文件夹路径，会遍历这个文件夹下面所有的Prefab,所有的Prefab的名字不能重复，必须保证名字的唯一性
    public List<string> m_AllPrefabPath = new List<string>();
    public List<FileDirAbName> m_AllFileDirAB = new List<FileDirAbName>();

    [System.Serializable]
    public struct FileDirAbName
    {
        public string ABName;
        public string Path;
    }
}
