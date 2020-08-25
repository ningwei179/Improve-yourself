using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 双向链表结构节点
/// </summary>
/// <typeparam name="T"></typeparam>
public class DoubleLinkedListNode<T> where T : class, new()
{
    //前一个节点
    public DoubleLinkedListNode<T> prev = null;
    //后一个节点
    public DoubleLinkedListNode<T> next = null;
    //当前节点
    public T t = null;
}

/// <summary>
/// 双向链表结构
/// </summary>
/// <typeparam name="T"></typeparam>
public class DoubleLinkedList<T> where T : class, new()
{
    //表头
    public DoubleLinkedListNode<T> Head = null;
    //表尾
    public DoubleLinkedListNode<T> Tail = null;
    //双向链表结构类对象池
    protected ClassObjectPool<DoubleLinkedListNode<T>> m_DoubleLinkNodePool = ObjectManager.Instance.GetOrCreateClassPool<DoubleLinkedListNode<T>>(500);
    //个数
    protected int m_Count = 0;
    public int Count
    {
        get { return m_Count; }
    }

    /// <summary>
    /// 添加一个节点到表头
    /// </summary>
    /// <param name="t"></param>
    public DoubleLinkedListNode<T> AddToHeader(T t)
    {
        DoubleLinkedListNode<T> pList = m_DoubleLinkNodePool.Spawn(true);
        pList.next = null;
        pList.prev = null;
        pList.t = t;
        return AddToHeader(pList);
    }

    /// <summary>
    /// 添加一个节点到表头
    /// </summary>
    /// <param name="pNode"></param>
    public DoubleLinkedListNode<T> AddToHeader(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null)
            return null;
        pNode.prev = null;          //该节点的前一个节点设置为空
        if (Head == null)           //双向链表,表头不存在
        {
            Head = Tail = pNode;    //该节点是双向链表的第一个节点， 双向链表的头，尾都是这个节点
        }
        else
        {                           //双向链表头存在
            pNode.next = Head;      //该节点下一个节点是双向链表当前的头节点，它要插到表头
            pNode.prev = null;      //该节点前一个节点没有，他要当表头，没有前一个
            Head = pNode;           //双向链表的头节点设置成改节点
        }

        m_Count++;                  //双向链表的节点总数++
        return Head;
    }

    /// <summary>
    /// 添加一个节点到表尾
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public DoubleLinkedListNode<T> AddToTail(T t)
    {
        DoubleLinkedListNode<T> pList = m_DoubleLinkNodePool.Spawn(true);
        pList.next = null;
        pList.prev = null;
        pList.t = t;
        return AddToTail(pList);
    }

    /// <summary>
    /// 添加一个节点到表尾
    /// </summary>
    /// <param name="pNode"></param>
    public DoubleLinkedListNode<T> AddToTail(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null)
            return null;
        pNode.next = null;              //该节点的后一个节点设置为空
        if (Tail == null)               //双向链表,表尾不存在
        {
            Head = Tail = pNode;        //该节点是双向链表的第一个节点， 双向链表的头，尾都是这个节点
        }
        else
        {                               //双向链表尾存在
            pNode.prev = Tail;          //该节点前一个节点是双向链表当前的尾节点，它要插到表尾
            pNode.next = null;          //该节点下一个节点没有，他要当表尾，没有下一个
            Head = pNode;               //双向链表的尾节点设置成改节点
        }

        m_Count++;                      //双向链表的节点总数++
        return Head;
    }

    /// <summary>
    /// 移除某个节点
    /// </summary>
    /// <param name="pNode"></param>
    public void RemoveNode(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null)
            return;

        if (pNode == Head)          //当前节点是头
            Head = pNode.next;      //把双向链表的头节点设置成它的下一个节点

        if (pNode == Tail)          //当前节点是尾
            Tail = pNode.prev;      //把双向链表的尾节点设置成它的上一个节点

        //设置前面节点和后面节点链接
        if (pNode.prev != null)                 //当前节点前面节点不是空
            pNode.prev.next = pNode.next;       //把当前节点前面节点的下一个节点设置成当前节点的下一个节点 a->b->c 拿掉b,b的前一个a,a的下一个就是b的下一个c了

        //设置后面节点和前面节点链接
        if (pNode.next != null)                 //当前节点后面节点不是空
            pNode.next.prev = pNode.prev;       //把当前节点后面节点的前一个节点设置成当前节点的前一个节点 a->b->c 拿掉b,b的下一个c,c的前一个就是b的前一个a了

        //把自己置空
        pNode.next = pNode.prev = null;
        pNode.t = null;
        //回收自己
        m_DoubleLinkNodePool.Recycle(pNode);
        m_Count--;
    }

    /// <summary>
    /// 把某个节点移动到头部
    /// </summary>
    /// <param name="pNode"></param>
    public void MoveToHead(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null || pNode == Head)
            return;

        if (pNode.prev == null && pNode.next == null)
            return;

        if (pNode == Tail)                      //该节点是尾节点
            Tail = pNode.prev;                  //设置尾结点是是他的上节点

        //设置前面节点和后面节点链接
        if (pNode.prev != null)                 
            pNode.prev.next = pNode.next;       

        //设置后面节点和前面节点链接
        if (pNode.next != null)                 
            pNode.next.prev = pNode.prev;

        pNode.prev = null;
        pNode.next = Head;
        Head.prev = pNode;
        Head = pNode;
        if (Tail == null)
        {
            Tail = Head;
        }
    }
}

/// <summary>
/// 双向链表集合
/// </summary>
/// <typeparam name="T"></typeparam>
public class CMapList<T> where T : class, new()
{
    DoubleLinkedList<T> m_DLink = new DoubleLinkedList<T>();

    Dictionary<T, DoubleLinkedListNode<T>> m_FindMap = new Dictionary<T, DoubleLinkedListNode<T>>();



    /// <summary>
    /// 插入一个节点到表头
    /// </summary>
    /// <param name="t"></param>
    public void InsetToHead(T t)
    {
        DoubleLinkedListNode<T> node = null;
        if (m_FindMap.TryGetValue(t, out node) && node != null)
        {
            m_DLink.AddToHeader(node);
            return;
        }

        m_DLink.AddToHeader(t);
        m_FindMap.Add(t, m_DLink.Head);
    }

    /// <summary>
    /// 从表尾弹出一个节点
    /// </summary>
    public void Pop()
    {
        if (m_DLink.Tail != null)
        {
            Remvoe(m_DLink.Tail.t);
        }
    }

    /// <summary>
    /// 删除某个节点
    /// </summary>
    /// <param name="t"></param>
    public void Remvoe(T t)
    {
        DoubleLinkedListNode<T> node = null;
        if (!m_FindMap.TryGetValue(t, out node) || node == null)
        {
            return;
        }
        m_DLink.RemoveNode(node);
        m_FindMap.Remove(t);
    }

    /// <summary>
    /// 获取尾部节点
    /// </summary>
    /// <returns></returns>
    public T Back()
    {
        return m_DLink.Tail == null ? null : m_DLink.Tail.t;
    }

    /// <summary>
    /// 返回节点个数
    /// </summary>
    /// <returns></returns>
    public int Size()
    {
        return m_FindMap.Count;
    }

    /// <summary>
    /// 查找是否存在该节点
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool Find(T t)
    {
        DoubleLinkedListNode<T> node = null;
        if (!m_FindMap.TryGetValue(t, out node) || node == null)
            return false;

        return true;
    }

    /// <summary>
    /// 刷新某个节点，把节点移动到头部
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool Refresh(T t)
    {
        DoubleLinkedListNode<T> node = null;
        if (!m_FindMap.TryGetValue(t, out node) || node == null)
            return false;

        m_DLink.MoveToHead(node);
        return true;
    }

    /// <summary>
    /// 清空双向链表
    /// </summary>
    public void Clear()
    {
        while (m_DLink.Tail !=null)
        {
            Remvoe(m_DLink.Tail.t);
        }
    }

    /// <summary>
    /// 析构函数，类被销毁的时候调用
    /// </summary>
    ~CMapList()
    {
        Clear();
    }
}

