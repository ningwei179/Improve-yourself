/****************************************************
	文件：ServerSession.cs
	作者：NingWei
	日期：2020/09/04 15:14   	
	功能：
*****************************************************/

using PENet;
using Protocal;

public class ServerSession : PENet.PESession<NetMsg> {

    protected override void OnConnected() {
        PETool.LogMsg("Client Connect");
    }

    protected override void OnReciveMsg(NetMsg msg)
    {
        PETool.LogMsg("Client Req"+msg.text);
    }

    protected override void OnDisConnected()
    {
        PETool.LogMsg("Client DisConnected");
    }
}
