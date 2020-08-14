using System;
using System.Collections.Generic;

/// <summary>
/// 对象池管理器
/// </summary>
public class ObjectManager : Singleton<ObjectManager>
{
    #region 类对象池的使用
    /// <summary>
    /// 所有对象池字典
    /// </summary>
    protected Dictionary<Type,object> m_ClassPoolDic = new Dictionary<Type, object>();

    /// <summary>
    /// 获取或者创建类对象池，创建完成后，外面可以保存ClassObjectPool<T>
    /// 然后调用Spawn和Recycle来创建和回收类对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maxcount"></param>
    /// <returns></returns>
    public ClassObjectPool<T> GetOrCreateClassPool<T>(int maxcount) where T : class, new()
    {
        Type type = typeof(T);

        object outObj = null;
        if (!m_ClassPoolDic.TryGetValue(type, out outObj) || outObj == null)
        {
            ClassObjectPool<T> newPool = new ClassObjectPool<T>(maxcount);
            m_ClassPoolDic.Add(type,newPool);
            return newPool;
        }

        return outObj as ClassObjectPool<T>;
    }
    #endregion
}
