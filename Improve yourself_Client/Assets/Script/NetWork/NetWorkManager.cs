/****************************************************
    文件：NetWorkManager.cs
	作者：NingWei
    日期：2020/9/4 15:50:29
	功能：网络通信相关
*****************************************************/

using Protocal;
using System.Collections.Generic;
using UnityEngine;

public class NetWorkManager : Singleton<NetWorkManager> 
{
    public PENet.PESocket<ClientSession, NetMsg> m_Client;

    private static readonly string obj = "lock";

    private Queue<NetMsg> msgQue = new Queue<NetMsg>();

    /// <summary>
    /// 网络通信管理初始化
    /// </summary>
    /// <param name="mono"></param>
    public void Init()
    {
        m_Client = new PENet.PESocket<ClientSession, NetMsg>();
        
        m_Client.SetLog(true, (string msg, int lv) => {
            switch (lv) {
                case 0:
                    msg = "log:" + msg;
                    Debug.Log(msg);
                    break;
                case 1:
                    msg = "Warning:" + msg;
                    Debug.LogWarning(msg);
                    break;
                case 2:
                    msg = "Error:" + msg;
                    Debug.LogError(msg);
                    break;
                case 3:
                    msg = "Info:" + msg;
                    Debug.Log(msg);
                    break;
            }
        });

        m_Client.StartAsClient(IPCfg.srvIP, IPCfg.srvPort);

        // 数据层注册网络事件
        MessageContext.Instance.RegisterServerMessage();
    }

    public void SendMsg(NetMsg msg) {
        if (m_Client.session != null)
        {
            m_Client.session.SendMsg(msg);
        }
        else {
            Debug.LogError("服务器未连接");
            Init();
        }
    }

    public void AddNetPkg(NetMsg msg) {
        lock (obj) {
            msgQue.Enqueue(msg);
        }
    }

    public void Update() {
        if (msgQue.Count > 0)
        {
            Common.log("PackCount:" + msgQue.Count);
            lock (obj)
            {
                NetMsg msg = msgQue.Dequeue();
                ProcessMsg(msg);
            }
        }
    }

    private void ProcessMsg(NetMsg msg) {
        
        if (msg.err != (int)ErrorCode.None)
        {
            switch ((ErrorCode)msg.err)
            {
                case ErrorCode.AcctIsOnline:

                    break;
                case ErrorCode.WrongPass:
                    break;
            }
            return;
        }
        MessagePublisher.Instance.Distribute(msg);
    }
}