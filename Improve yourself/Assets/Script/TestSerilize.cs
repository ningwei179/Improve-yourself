using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
/// <summary>
/// C#序列化
/// </summary>
[System.Serializable]
public class TestSerilize
{
    [XmlAttribute("Id")]
    public int Id { get; set; }
    [XmlAttribute("Name")]
    public string Name { get; set; }
    [XmlElement("List")]
    public List<int> List { get; set; }
}
