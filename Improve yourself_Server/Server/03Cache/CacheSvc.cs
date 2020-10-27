/****************************************************
	文件：CacheSvc.cs
	作者：NingWei
	日期：2020/09/07 15:19   	
	功能：
*****************************************************/

using IYProtocal;
using System.Collections.Generic;

public class CacheSvc
{
    private static CacheSvc instance = null;
    public static CacheSvc Instance
    {
        get
        {
            if (instance == null)
                instance = new CacheSvc();
            return instance;
        }
    }

    private Dictionary<string, ServerSession> onLineAcctDic = new Dictionary<string, ServerSession>();

    private Dictionary<ServerSession, PlayerData> onLineSessionDic = new Dictionary<ServerSession, PlayerData>();

    public void Init()
    {
        IYCommon.IYSocketLog("CacheSvc Init Done");
    }

    public bool IsAcctOnLine(string acct) {
        return onLineAcctDic.ContainsKey(acct);
    }

    /// <summary>
    /// 从数据库写数据
    /// </summary>
    /// <param name="acct"></param>
    /// <param name="pass"></param>
    /// <returns></returns>
    public PlayerData GetPlayerData(string acct,string pass) {
        return null;
    }

    /// <summary>
    /// 账号上线，缓存账号，session，和玩家数据
    /// </summary>
    /// <param name="acct"></param>
    /// <param name="session"></param>
    /// <param name="playerData"></param>
    public void AcctOnline(string acct, ServerSession session, PlayerData playerData) {
        onLineAcctDic.Add(acct, session);
        onLineSessionDic.Add(session, playerData);
    }
}
