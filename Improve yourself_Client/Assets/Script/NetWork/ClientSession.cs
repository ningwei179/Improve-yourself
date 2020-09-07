/****************************************************
    文件：ClientSession.cs
	作者：NingWei
    日期：2020/9/4 15:54:44
	功能：客户端Session
*****************************************************/

using PENet;
using Protocal;

public class ClientSession : PESession<NetMsg> 
{
    protected override void OnConnected()
    {
        Common.log("Server Connect To Server Succ");
    }

    protected override void OnReciveMsg(NetMsg msg)
    {
        Common.log("RcvPack CMD:"+((CMD)msg.cmd).ToString());
        NetWorkManager.Instance.AddNetPkg(msg);
    }

    protected override void OnDisConnected()
    {
        Common.log("Server DisConnected");
    }
}