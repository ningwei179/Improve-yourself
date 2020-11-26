/****************************************************
	文件：ServerInfo.cs
	作者：NingWei
	日期：2020/09/19 15:34   	
	功能：版本信息文件
*****************************************************/
using System.Collections.Generic;
using System.Xml.Serialization;
namespace Improve
{
    [System.Serializable]
    public class ServerInfo
    {
        [XmlElement("GameVersion")]
        public VersionInfo[] GameVersion;
    }

    //当前游戏版本对应的所有补丁
    [System.Serializable]
    public class VersionInfo
    {
        [XmlAttribute]
        public string Version;
        [XmlElement]
        public Pathces[] Pathces;
    }

    //一个总补丁包
    [System.Serializable]
    public class Pathces
    {
        [XmlAttribute]
        public int Version;

        [XmlAttribute]
        public string Des;

        [XmlElement]
        public List<Patch> Files;
    }

    /// <summary>
    /// 单个补丁包
    /// </summary>
    [System.Serializable]
    public class Patch
    {
        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public string Url;

        [XmlAttribute]
        public string Platform;

        [XmlAttribute]
        public string Md5;

        [XmlAttribute]
        public float Size;
    }
}