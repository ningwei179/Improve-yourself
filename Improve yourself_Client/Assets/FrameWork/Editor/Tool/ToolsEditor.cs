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
    private static string DLLPATH = "Assets/GameData/Data/HotFix/HotFixProject.dll";
    private static string PDBPATH = "Assets/GameData/Data/HotFix/HotFixProject.pdb";

    [MenuItem("Tools/修改热更dll为txt")]
    public static void ChangeDllName()
    {
        if (File.Exists(DLLPATH))
        {
            string targetPath = DLLPATH + ".txt";
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
            File.Move(DLLPATH, targetPath);
        }

        if (File.Exists(PDBPATH))
        {
            string targetPath = PDBPATH + ".txt";
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
            File.Move(PDBPATH, targetPath);
        }
        AssetDatabase.Refresh();
    }
}