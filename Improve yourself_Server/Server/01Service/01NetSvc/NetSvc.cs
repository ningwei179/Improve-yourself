/****************************************************
	文件：NetSvc.cs
	作者：NingWei
	日期：2020/09/07 13:59   	
	功能：网络服务处
*****************************************************/
using PENet;
using Protocal;
using System.Collections.Generic;

public class MsgPack {
    public ServerSession session;
    public NetMsg msg;
    public MsgPack(ServerSession session, NetMsg msg) {
        this.session = session;
        this.msg = msg;
    }
}

public class NetSvc
{
    private static NetSvc instance = null;
    public static NetSvc Instance
    {
        get
        {
            if (instance == null)
                instance = new NetSvc();
            return instance;
        }
    }

    public static readonly string obj = "lock";
    private Queue<MsgPack> msgPackQue = new Queue<MsgPack>();

    public void Init()
    {
        PESocket<ServerSession, NetMsg> server = new PESocket<ServerSession, NetMsg>();
        server.StartAsServer(IPCfg.srvIP, IPCfg.srvPort);

        Common.log("NetSvc Init Done");
    }

    public void AddMsgQue(ServerSession session,NetMsg msg) {
        lock (obj) {
            msgPackQue.Enqueue(new MsgPack(session, msg));
        }
    }

    public void Update() {
        if (msgPackQue.Count > 0) {
            Common.log("PackCount:" + msgPackQue.Count);
            lock (obj) {
                MsgPack msgPack = msgPackQue.Dequeue();
                HandOutMsg(msgPack);
            }
        }
    }

    private void HandOutMsg(MsgPack msgPack) {
        switch ((CMD)msgPack.msg.cmd) {
            case CMD.Reqlogin:
                loginSys.Instance.ReqLogin(msgPack);
                break;
        }
    }
}
