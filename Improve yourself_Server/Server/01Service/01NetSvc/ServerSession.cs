/****************************************************
	文件：ServerSession.cs
	作者：NingWei
	日期：2020/09/04 15:14   	
	功能：ServerSession
*****************************************************/

using Protocal;

public class ServerSession : PENet.PESession<NetMsg> {

    protected override void OnConnected() {
        Common.log("Client Connect");
    }

    protected override void OnReciveMsg(NetMsg msg)
    {
        Common.log("RcvPack CMD:"+((CMD)msg.cmd).ToString());
        NetSvc.Instance.AddMsgQue(this,msg);
    }

    protected override void OnDisConnected()
    {
        Common.log("Client DisConnected");
    }
}
