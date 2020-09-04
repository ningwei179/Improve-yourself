/****************************************************
    文件：NetWorkManager.cs
	作者：NingWei
    日期：2020/9/4 15:50:29
	功能：网络通信相关
*****************************************************/


using Protocal;

public class NetWorkManager : Singleton<NetWorkManager> 
{
    /// <summary>
    /// 网络通信管理初始化
    /// </summary>
    /// <param name="mono"></param>
    public void Init()
    {
        PENet.PESocket<ClientSession, NetMsg> client = new PENet.PESocket<ClientSession, NetMsg>();
        client.StartAsClient(IPCfg.srvIP, IPCfg.srvPort);
    }
}