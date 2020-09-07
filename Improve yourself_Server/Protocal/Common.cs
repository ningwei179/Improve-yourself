/****************************************************
	文件：Common.cs
	作者：NingWei
	日期：2020/09/07 14:18   	
	功能：客户端服务端公用工具类
*****************************************************/
using PENet;
public enum LogType {
    Log = 0,
    Warm= 1,
    Error = 2,
    Info = 3
}

public class Common
{
    public static void log(string msg = "",LogType tp = LogType.Log) {
        LogLevel lv = (LogLevel)tp;
        PETool.LogMsg(msg, lv);
    }
}
