using System.Collections.Generic;
namespace Improve
{
    /// <summary>
    /// 类对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ClassObjectPool<T> where T : class, new()
    {
        /// <summary>
        /// 类对象池
        /// </summary>
        protected Stack<T> m_Pool = new Stack<T>();

        /// <summary>
        /// 最大对象个数，
        /// 小于等于 0 表示不限个数
        /// </summary>
        protected int m_MaxCount = 0;

        /// <summary>
        /// 没有回收的个数
        /// </summary>
        protected int m_NoRecycleCount = 0;

        /// <summary>
        /// 创建这么多数量的类
        /// </summary>
        /// <param name="maxcount"></param>
        public ClassObjectPool(int maxcount)
        {
            m_MaxCount = maxcount;
            for (int i = 0; i < m_MaxCount; i++)
            {
                m_Pool.Push(new T());
            }
        }

        /// <summary>
        /// 从池子里面取出类对象
        /// </summary>
        /// <param name="createIfPoolEmpty">如果是空的是否new一个</param>
        /// <returns></returns>
        public T Spawn(bool createIfPoolEmpty)
        {
            if (m_Pool.Count > 0)   //池子里有对象
            {
                T rtn = m_Pool.Pop();   //取出一个对象
                if (rtn == null)        //对象是空的？
                {
                    if (createIfPoolEmpty) //如果是空的是否new一个
                    {
                        rtn = new T();  //创建一个对象
                    }
                }
                m_NoRecycleCount++;     //没有被回收的对象数量++
                return rtn;
            }
            else
            {
                if (createIfPoolEmpty)
                {
                    T rtn = new T();  //创建一个对象
                    m_NoRecycleCount++; //没有被回收的对象数量++
                    return rtn;
                }
            }

            return null;
        }

        /// <summary>
        /// 回收对象
        /// 对象是空的，或者池子饱和了都会返回回收失败
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Recycle(T obj)
        {
            if (obj == null)
                return false;

            m_NoRecycleCount--;

            //池子里面的对象饱和了，直接释放这个对象
            if (m_Pool.Count >= m_MaxCount && m_MaxCount > 0)
            {
                obj = null;
                return false;
            }

            m_Pool.Push(obj);
            return true;
        }
    }
}