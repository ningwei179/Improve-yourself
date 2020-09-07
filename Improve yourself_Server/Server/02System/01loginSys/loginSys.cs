/****************************************************
	文件：loginSys.cs
	作者：NingWei
	日期：2020/09/07 14:02   	
	功能：登录模块
*****************************************************/

using Protocal;

public class loginSys
{
    private static loginSys instance = null;
    public static loginSys Instance
    {
        get
        {
            if (instance == null)
                instance = new loginSys();
            return instance;
        }
    }

    private CacheSvc cacheSvc;

    public void Init()
    {
        cacheSvc = CacheSvc.Instance;
        Common.log("login Init Done");
    }

    public void ReqLogin(MsgPack msgPack) {
        ReqLogin data = msgPack.msg.reqLogin;
        //当前账号是否已经上线
        NetMsg msg = new NetMsg();
        msg.cmd = (int)CMD.RspLogin;
        
        
        //已上线：返回错误信息
        if (cacheSvc.IsAcctOnLine(data.account))
        {
            msg.err = (int)ErrorCode.AcctIsOnline;
        }
        else {
            //未上线：
            //账号是否存在
            PlayerData playerData = cacheSvc.GetPlayerData(data.account, data.pass);
            if (playerData == null)
            {
                //存在密码错误
                msg.err = (int)ErrorCode.WrongPass;
            }
            else {
                msg.rspLogin = new RspLogin();
                msg.rspLogin.playerData = playerData;
                cacheSvc.AcctOnline(data.account, msgPack.session, playerData);
            }
        }
        //回应客户端
        msgPack.session.SendMsg(msg);
    }
}
