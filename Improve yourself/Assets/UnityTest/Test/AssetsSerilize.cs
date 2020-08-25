using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// unity Assets序列化
/// </summary>
[CreateAssetMenu(fileName = "TestAssets",menuName = "CreateAssets"),DefaultExecutionOrder(0)]
public class AssetsSerilize : ScriptableObject
{
    public int Id;
    public string Name;
    public List<int> List;      //字典是不能序列化的，要改成俩个list
}
