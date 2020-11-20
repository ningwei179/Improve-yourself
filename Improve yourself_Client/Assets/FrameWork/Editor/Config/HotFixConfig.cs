/********************************************************************
	�ļ�:	HotFixConfig.cs
	����:	NingWei
	����:	2020/11/18 9:56
	����:	C#������Ȳ�����������
*********************************************************************/

using IFix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
//1������������[Configure]��ǩ
//2�������EditorĿ¼
/// </summary>
[Configure]
public class HotFixConfig
{
    //[IFix]
    //static IEnumerable<Type> hotfix
    //{
    //    get
    //    {
    //        return (from type in Assembly.Load("Assembly-CSharp").GetTypes()
    //                where type.Namespace == "Improve"
    //                select type).ToList();
    //    }
    //}

    [IFix]
    static IEnumerable<Type> hotfix
    {
        get
        {
            return new List<Type>()
            {
                typeof(MenuWindow),
            };
        }
    }
}
