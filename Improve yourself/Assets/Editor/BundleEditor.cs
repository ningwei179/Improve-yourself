using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BundleEditor
{
    [MenuItem("Tools/打包")]
    public static void Build()
    {
        //打包bundle
        BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
        //编辑器刷新
        AssetDatabase.Refresh();    
    }
}
