/****************************************************
    文件：MessagePublisher.cs
	作者：NingWei
    日期：2020/9/7 17:55:15
	功能：服务器消息发布
*****************************************************/


using Protocal;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MessagePublisher : Singleton<MessagePublisher>
{
    /// <summary>
    /// 服务器消息委托
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    public delegate void MessageHandler<T>(T message);

    private Dictionary<string, Delegate> messageEventDic = new Dictionary<string, System.Delegate>();

    /// <summary>
    /// 服务器消息订阅
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="messageHandler"></param>
    public void Subscribe<T>(MessageHandler<T> messageHandler)
    {
        string type = typeof(T).Name;
        if (!messageEventDic.ContainsKey(type))
        {
            messageEventDic[type] = null;
        }
        messageEventDic[type] = (MessageHandler<T>)messageEventDic[type] + messageHandler;
    }

    /// <summary>
    /// 服务器消息取消订阅
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="messageHandler"></param>
    public void Unsubscribe<T>(MessageHandler<T> messageHandler)
    {
        string type = typeof(T).Name;
        if (!messageEventDic.ContainsKey(type))
        {
            messageEventDic[type] = null;
        }
        messageEventDic[type] = (MessageHandler<T>)messageEventDic[type] - messageHandler;
    }

    /// <summary>
    /// 服务器消息执行
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="msg"></param>
    public void RaiseEvent<T>(T msg)
    {
        string key = msg.GetType().Name;
        if (messageEventDic.ContainsKey(key))
        {
            MessageHandler<T> Handler = (MessageHandler<T>)messageEventDic[key];
            if (Handler != null)
            {
                try
                {
                    Handler(msg);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(string.Format("Setup error:{0}, {1}, {2}, {3}", ex.InnerException, ex.Message, ex.Source, ex.StackTrace));
                }
            }
            else
            {
                Debug.LogWarning("No handler subscribed for {0}" + msg.ToString());
            }
        }
    }

    /// <summary>
    /// 服务器消息派遣
    /// </summary>
    /// <param name="message"></param>
    public void Distribute(NetMsg message)
    {
        if (message == null)
        {
            return;
        }
        MessageDispatch.Instance.Dispatch(message);
    }
}