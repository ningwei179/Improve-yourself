/****************************************************
	文件：ServerSession.cs
	作者：NingWei
	日期：2020/09/04 15:14   	
	功能：ServerSession
*****************************************************/

using IYNet;
using IYProtocal;

public class ServerSession:IYSession<NetMsg>{
    public int sessionID = 0;

    protected override void OnConnected() {
        IYCommon.IYSocketLog("Client Connect");
    }

    protected override void OnReciveMsg(NetMsg msg)
    {
        IYCommon.IYSocketLog("RcvPack CMD:" + ((CMD)msg.cmd).ToString());
        NetSvc.Instance.AddMsgQue(this, msg);
    }

    protected override void OnDisConnected()
    {
        IYCommon.IYSocketLog("Client DisConnected");
    }
}
