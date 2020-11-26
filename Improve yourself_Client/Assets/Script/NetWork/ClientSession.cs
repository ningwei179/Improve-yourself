/****************************************************
    文件：ClientSession.cs
	作者：NingWei
    日期：2020/9/4 15:54:44
	功能：客户端Session
*****************************************************/

using IYNet;
using IYProtocal;
namespace Improve
{

    public class ClientSession : IYSession<NetMsg>
    {
        protected override void OnConnected()
        {
            IYCommon.IYSocketLog("Server Connect To Server Succ");
        }

        protected override void OnReciveMsg(NetMsg msg)
        {
            IYCommon.IYSocketLog("RcvPack CMD:" + ((CMD)msg.cmd).ToString());
            NetWorkManager.Instance.AddNetPkg(msg);
        }

        protected override void OnDisConnected()
        {
            IYCommon.IYSocketLog("Server DisConnected");
        }
    }
}