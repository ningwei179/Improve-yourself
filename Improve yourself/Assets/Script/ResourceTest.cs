using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Xml.Serialization;
using TMPro.EditorUtilities;

public class ResourceTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //AssetBundle assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/role");
        ////实例化角色
        //GameObject rolePrefab = assetBundle.LoadAsset<GameObject>("C0002");
        //Instantiate(rolePrefab);

        //编辑器加载，游戏运行中不会用到
        //GameObject rolePrefab = Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/C0002.prefab"));

        //XmlSerilizeTest();

        //DeXmlSerilizerTest();

        //BinarySerilizeTest();

        //DeBinarySerilizeTest();

        ReadTestAssets();
    }

    /// <summary>
    /// 读取unity Assets 序列化
    /// </summary>
    void ReadTestAssets()
    {
        AssetsSerilize assets =
            UnityEditor.AssetDatabase.LoadAssetAtPath<AssetsSerilize>("Assets/TestAssets.asset");
        Debug.Log(assets.Id);
        foreach (var item in assets.List)
        {
            Debug.Log(item);
        }
    }

    /// <summary>
    /// XML序列化测试
    /// </summary>
    void XmlSerilizeTest()
    {
        TestSerilize testSerilize = new TestSerilize();
        testSerilize.Id = 1;
        testSerilize.Name = "XML测试";
        testSerilize.List = new List<int>();
        testSerilize.List.Add(1);
        testSerilize.List.Add(2);
        testSerilize.List.Add(3);
        XmlSerilize(testSerilize);
    }

    /// <summary>
    /// XML反序列化测试
    /// </summary>
    void DeXmlSerilizerTest()
    {
        TestSerilize testSerilize =  XmlDeSerilize();
        foreach (var item in testSerilize.List)
        {
            Debug.Log(item);
        }
    }

    /// <summary>
    /// XML序列化
    /// </summary>
    /// <param name="testSerilize"></param>
    void XmlSerilize(TestSerilize testSerilize)
    {
        //创建一个文件流对象
        FileStream fileStream = new FileStream(Application.dataPath+"/test.xml",FileMode.Create,FileAccess.ReadWrite,FileShare.ReadWrite);
        //创建一个写入流对象
        StreamWriter sw = new StreamWriter(fileStream,System.Text.Encoding.UTF8);
        //创建xml序列化对象
        XmlSerializer xml = new XmlSerializer(testSerilize.GetType());
        //用xml序列化对象将testSerilize这个类写入到流中
        xml.Serialize(sw,testSerilize);
        //关闭写入流和文件流
        sw.Close();
        fileStream.Close();
    }

    /// <summary>
    /// XML反序列化
    /// </summary>
    TestSerilize XmlDeSerilize()
    {
        //创建一个文件流对象
        FileStream fileStream = new FileStream(Application.dataPath + "/test.xml", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        //创建xml序列化对象
        XmlSerializer xs = new XmlSerializer(typeof(TestSerilize));
        //反序列化xml返回一个类对象
        TestSerilize testSerilize = (TestSerilize)xs.Deserialize(fileStream);
        fileStream.Close();
        return testSerilize;
    }

    void BinarySerilizeTest()
    {
        TestSerilize testSerilize = new TestSerilize();
        testSerilize.Id = 1;
        testSerilize.Name = "二进制测试";
        testSerilize.List = new List<int>();
        testSerilize.List.Add(1);
        testSerilize.List.Add(2);
        testSerilize.List.Add(3);

        BinarySerilize(testSerilize);
    }

    void DeBinarySerilizeTest()
    {
        TestSerilize testSerilize = DeBinarySerilize();
        foreach (var item in testSerilize.List)
        {
            Debug.Log(item);
        }
    }

    /// <summary>
    /// 二进制序列化
    /// </summary>
    /// <param name="serilize"></param>
    void BinarySerilize(TestSerilize serilize)
    {
        //创建一个文件流对象
        FileStream fileStream = new FileStream(Application.dataPath + "/test.bytes", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        //二进制序列化对象
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fileStream,serilize);
        fileStream.Close();
    }

    /// <summary>
    /// 二进制反序列化
    /// </summary>
    /// <returns></returns>
    TestSerilize DeBinarySerilize()
    {
        //加载文件
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/test.bytes");
        //创建一个内存流
        MemoryStream stream = new MemoryStream(textAsset.bytes);
        //二进制序列化对象
        BinaryFormatter bf = new BinaryFormatter();

        TestSerilize testSerilize = (TestSerilize)bf.Deserialize(stream);
        //关闭内存流
        stream.Close();
        return testSerilize;
    }
}
