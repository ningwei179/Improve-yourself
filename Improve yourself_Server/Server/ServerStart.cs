/****************************************************
	文件：ServerStart.cs
	作者：NingWei
	日期：2020/09/04 15:04   	
	功能：
*****************************************************/
using PENet;
using Protocal;
namespace Server { 
    class ServerStart
    {
        static void Main(string[] args)
        {
            PESocket<ServerSession, NetMsg> server = new PESocket<ServerSession, NetMsg>();
            server.StartAsServer(IPCfg.srvIP, IPCfg.srvPort);

            while (true) {

            }
        }
    }
}