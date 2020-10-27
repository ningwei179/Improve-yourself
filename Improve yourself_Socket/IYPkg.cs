/****************************************************
	文件：IYPkg.cs
	作者：NingWei
	日期：2020/10/27 15:19   	
	功能：数据包
*****************************************************/
using System;

namespace IYNet
{
    class IYPkg
    {
        public int headLen = 4;
        public byte[] headBuff = null;
        public int headIndex = 0;

        public int bodyLen = 0;
        public byte[] bodyBuff = null;
        public int bodyIndex = 0;

        public IYPkg()
        {
            headBuff = new byte[4];
        }

        public void InitBodyBuff()
        {
            bodyLen = BitConverter.ToInt32(headBuff, 0);
            bodyBuff = new byte[bodyLen];
        }

        public void ResetData()
        {
            headIndex = 0;
            bodyLen = 0;
            bodyBuff = null;
            bodyIndex = 0;
        }
    }
}
