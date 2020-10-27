/****************************************************
	文件：NetMsg.cs
	作者：NingWei
	日期：2020/09/04 15:04   	
	功能：协议数据
*****************************************************/
using IYNet;
using System;

namespace IYProtocal
{
    [Serializable]
    public class NetMsg : IYMsg
    {
        public ReqLogin reqLogin;
        public RspLogin rspLogin;
    }

    [Serializable]
    public class ReqLogin {
        public string account;
        public string pass;
    }

    [Serializable]
    public class RspLogin
    {
        public PlayerData playerData;
    }

    [Serializable]
    public class PlayerData
    {
        public int id;
        public string name;
        public int lv;
        public int exp;
        public int power;
        public int coin;
        public int diammond;
    }

    /// <summary>
    /// 错误码
    /// </summary>
    public enum ErrorCode {
        None = 0,       //没有错误
        AcctIsOnline,   //账号已经上线
        WrongPass,      //
    }

    /// <summary>
    /// 协议号
    /// </summary>
    public enum CMD {
        None = 0,
        //登录相关 100
        Reqlogin = 101,
        RspLogin = 102,
    }

    public class IPCfg {
        public const string srvIP = "127.0.0.1";

        public const int srvPort = 17666;
    }
}
