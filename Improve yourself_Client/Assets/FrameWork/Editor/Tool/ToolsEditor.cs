/****************************************************
    文件：ToolsEditor.cs
	作者：NingWei
    日期：2020/10/14 19:30:27
	功能：Nothing
*****************************************************/

using UnityEngine;
using UnityEditor;
using System.IO;

public class ToolsEditor
{
    private static string DLLPATH = "Assets/GameData/Data/ILRuntimeHotFix/HotFixProject.dll";
    private static string PDBPATH = "Assets/GameData/Data/ILRuntimeHotFix/HotFixProject.pdb";

    [MenuItem("Tools/修改热更dll为bytes")]
    public static void ChangeDllName()
    {
        if (File.Exists(DLLPATH))
        {
            string targetPath = DLLPATH + ".bytes";
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
            File.Move(DLLPATH, targetPath);
        }

        if (File.Exists(PDBPATH))
        {
            string targetPath = PDBPATH + ".bytes";
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
            File.Move(PDBPATH, targetPath);
        }
        AssetDatabase.Refresh();
    }
}