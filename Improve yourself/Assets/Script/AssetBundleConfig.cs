using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

[System.Serializable]
public class AssetBundleConfig
{
    [XmlElement("ABList")]
    public List<ABBase> ABList { get; set; }
}

[System.Serializable]
public class ABBase
{
    [XmlAttribute("Path")]
    public string Path { get; set; }    //资源文件路径
    [XmlAttribute("Crc")]
    public uint Crc { get; set; }       //将资源文件路径转换成Crc
    [XmlAttribute("ABName")]
    public string ABName;               //AB包名
    [XmlAttribute("AssetName")]
    public string AssetName;            //资源名称
    [XmlElement("ABDependce")]
    public List<string> ABDependce { get; set; }    //这个资源依赖的其他资源包名列表
}
