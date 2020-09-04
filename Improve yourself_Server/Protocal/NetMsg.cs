/****************************************************
	文件：NetMsg.cs
	作者：NingWei
	日期：2020/09/04 15:04   	
	功能：
*****************************************************/
using PENet;
using System;

namespace Protocal
{
    [Serializable]
    public class NetMsg:PEMsg
    {
        public string text;
    }

    public class IPCfg {
        public const string srvIP = "127.0.0.1";

        public const int srvPort = 17666;
    }
}
