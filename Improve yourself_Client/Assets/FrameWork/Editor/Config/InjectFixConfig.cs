using IFix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
[Configure]
public class InjectFixConfig
{
    /// <summary>
    /// 通过反射将一些条件下的脚本都标记为可热更
    /// </summary>
    [IFix]
    static IEnumerable<Type> hotfix
    {
        get
        {
            return (from type in Assembly.Load("Assembly-CSharp").GetTypes()
                    where type.Namespace == "Improve"
                    select type).ToList();
        }
    }
    
    /// <summary>
    /// 有些脚本不好统一查找，可以在这里写死
    /// </summary>
    [IFix]
    static IEnumerable<Type> hotfixAdd
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
