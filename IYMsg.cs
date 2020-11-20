/****************************************************
	文件：IYMsg.cs
	作者：NingWei
	日期：2020/10/27 15:20   	
	功能：消息包
*****************************************************/

namespace IYNet
{
    using System;

    [Serializable]
    public abstract class IYMsg
    {
        public int seq;
        public int cmd;
        public int err;
    }
}
