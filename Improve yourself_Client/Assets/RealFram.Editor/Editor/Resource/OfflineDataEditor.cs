/****************************************************
	文件：OfflineDataEditor.cs
	作者：NingWei
	日期：2020/09/07 13:06   	
	功能：离线数据编辑
*****************************************************/
using UnityEditor;
using UnityEngine;

public class OfflineDataEditor
{
    [MenuItem("Assets/生成离线数据")]
    public static void AssetCreateOfflineData()
    {
        GameObject[] objects = Selection.gameObjects;
        for (int i = 0; i < objects.Length; i++)
        {
            string prefabPath = AssetDatabase.GetAssetPath(objects[i]);
            EditorUtility.DisplayProgressBar("添加离线数据","正在修改：" + objects[i]+".....",1.0f/objects.Length);
            CreateOfflineData(prefabPath);
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Assets/生成UI离线数据")]
    public static void AssetCreateUIOfflineData()
    {
        GameObject[] objects = Selection.gameObjects;
        for (int i = 0; i < objects.Length; i++)
        {
            string prefabPath = AssetDatabase.GetAssetPath(objects[i]);
            EditorUtility.DisplayProgressBar("添加UI离线数据", "正在修改：" + objects[i] + ".....", 1.0f / objects.Length);
            CreateUIOfflineData(prefabPath);
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("离线数据/生成所有UI离线数据")]
    public static void AllCreateUIOfflineData()
    {
        //获取路径下的所有的Prefab
        string[] allStr = AssetDatabase.FindAssets("t:Prefab", new string[]{ "Assets/GameData/Prefabs/UGUI" });

        for (int i = 0; i < allStr.Length; i++)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(allStr[i]); //将获得的资源GUID转换成路径
            EditorUtility.DisplayProgressBar("添加UI离线数据", "正在扫描路径：" + prefabPath + ".....", 1.0f / allStr.Length *i);
            CreateUIOfflineData(prefabPath);
        }
        Debug.Log("UI离线数据全部生成完毕");
        EditorUtility.ClearProgressBar();
    }


    [MenuItem("Assets/生成特效离线数据")]
    public static void AssetCreateEffectOfflineData()
    {
        GameObject[] objects = Selection.gameObjects;
        for (int i = 0; i < objects.Length; i++)
        {
            string prefabPath = AssetDatabase.GetAssetPath(objects[i]);
            EditorUtility.DisplayProgressBar("添加特效离线数据", "正在修改：" + objects[i] + ".....", 1.0f / objects.Length);
            CreateEffectOfflineData(prefabPath);
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("离线数据/生成所有特效离线数据")]
    public static void AllCreateEffectOfflineData()
    {
        //获取路径下的所有的Prefab
        string[] allStr = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/GameData/Prefabs/Effect" });

        for (int i = 0; i < allStr.Length; i++)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(allStr[i]); //将获得的资源GUID转换成路径
            EditorUtility.DisplayProgressBar("添加特效离线数据", "正在扫描路径：" + prefabPath + ".....", 1.0f / allStr.Length * i);
            CreateEffectOfflineData(prefabPath);
        }
        Debug.Log("特效离线数据全部生成完毕");
        EditorUtility.ClearProgressBar();
    }


    public static void CreateOfflineData(string prefabPath)
    {
        GameObject go = PrefabUtility.LoadPrefabContents(prefabPath);
        OfflineData offlineData = go.GetComponent<OfflineData>();
        if(offlineData == null)
        {
            offlineData = go.AddComponent<OfflineData>();
        }
        offlineData.BindData();

        Debug.Log("修改了"+ go.name+" prefab!");

        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        PrefabUtility.UnloadPrefabContents(go);
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }


    public static void CreateUIOfflineData(string prefabPath)
    {
        GameObject go = PrefabUtility.LoadPrefabContents(prefabPath);
        //将UI的层级改成UI
        go.layer = LayerMask.NameToLayer("UI");

        UIOfflineData uiOfflineData = go.GetComponent<UIOfflineData>();
        if (uiOfflineData == null)
        {
            uiOfflineData = go.AddComponent<UIOfflineData>();
        }
        uiOfflineData.BindData();

        Debug.Log("修改了" + go.name + " UI prefab!");

        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        PrefabUtility.UnloadPrefabContents(go);
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }

    public static void CreateEffectOfflineData(string prefabPath)
    {
        GameObject go = PrefabUtility.LoadPrefabContents(prefabPath);

        EffectOfflineData effectOfflineData = go.GetComponent<EffectOfflineData>();
        if (effectOfflineData == null)
        {
            effectOfflineData = go.AddComponent<EffectOfflineData>();
        }
        effectOfflineData.BindData();

        Debug.Log("修改了" + go.name + " 特效 prefab!");

        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        PrefabUtility.UnloadPrefabContents(go);
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }
}
