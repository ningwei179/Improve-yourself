/****************************************************
	文件：IYSession.cs
	作者：NingWei
	日期：2020/10/27 15:20   	
	功能：IYSession
*****************************************************/
using System;
using System.Net.Sockets;

namespace IYNet
{
    public abstract class IYSession<T> where T : IYMsg
    {
        private Socket skt;
        private Action closeCB;

        #region Recevie
        public void StartRcvData(Socket skt, Action closeCB)
        {
            try
            {
                this.skt = skt;
                this.closeCB = closeCB;

                OnConnected();

                IYPkg pack = new IYPkg();
                skt.BeginReceive(
                    pack.headBuff,
                    0,
                    pack.headLen,
                    SocketFlags.None,
                    new AsyncCallback(RcvHeadData),
                    pack);
            }
            catch (Exception e)
            {
                IYTool.LogMsg("StartRcvData:" + e.Message, LogLevel.Error);
            }
        }

        private void RcvHeadData(IAsyncResult ar)
        {
            try
            {
                IYPkg pack = (IYPkg)ar.AsyncState;
                if (skt.Available == 0)
                {
                    OnDisConnected();
                    Clear();
                    return;
                }
                int len = skt.EndReceive(ar);
                if (len > 0)
                {
                    pack.headIndex += len;
                    if (pack.headIndex < pack.headLen)
                    {
                        skt.BeginReceive(
                            pack.headBuff,
                            pack.headIndex,
                            pack.headLen - pack.headIndex,
                            SocketFlags.None,
                            new AsyncCallback(RcvHeadData),
                            pack);
                    }
                    else
                    {
                        pack.InitBodyBuff();
                        skt.BeginReceive(pack.bodyBuff,
                            0,
                            pack.bodyLen,
                            SocketFlags.None,
                            new AsyncCallback(RcvBodyData),
                            pack);
                    }
                }
                else
                {
                    OnDisConnected();
                    Clear();
                }
            }
            catch (Exception e)
            {
                IYTool.LogMsg("RcvHeadError:" + e.Message, LogLevel.Error);
            }
        }

        private void RcvBodyData(IAsyncResult ar)
        {
            try
            {
                IYPkg pack = (IYPkg)ar.AsyncState;
                int len = skt.EndReceive(ar);
                if (len > 0)
                {
                    pack.bodyIndex += len;
                    if (pack.bodyIndex < pack.bodyLen)
                    {
                        skt.BeginReceive(pack.bodyBuff,
                            pack.bodyIndex,
                            pack.bodyLen - pack.bodyIndex,
                            SocketFlags.None,
                            new AsyncCallback(RcvBodyData),
                            pack);
                    }
                    else
                    {
                        T msg = IYTool.DeSerialize<T>(pack.bodyBuff);
                        OnReciveMsg(msg);

                        //loop recive
                        pack.ResetData();
                        skt.BeginReceive(
                            pack.headBuff,
                            0,
                            pack.headLen,
                            SocketFlags.None,
                            new AsyncCallback(RcvHeadData),
                            pack);
                    }
                }
                else
                {
                    OnDisConnected();
                    Clear();
                }
            }
            catch (Exception e)
            {
                IYTool.LogMsg("RcvBodyError:" + e.Message, LogLevel.Error);
            }
        }
        #endregion

        #region Send
        /// <summary>
        /// Send message data
        /// </summary>
        public void SendMsg(T msg)
        {
            byte[] data = IYTool.PackLenInfo(IYTool.Serialize<T>(msg));
            SendMsg(data);
        }

        /// <summary>
        /// Send binary data
        /// </summary>
        public void SendMsg(byte[] data)
        {
            NetworkStream ns = null;
            try
            {
                ns = new NetworkStream(skt);
                if (ns.CanWrite)
                {
                    ns.BeginWrite(
                        data,
                        0,
                        data.Length,
                        new AsyncCallback(SendCB),
                        ns);
                }
            }
            catch (Exception e)
            {
                IYTool.LogMsg("SndMsgError:" + e.Message, LogLevel.Error);
            }
        }

        private void SendCB(IAsyncResult ar)
        {
            NetworkStream ns = (NetworkStream)ar.AsyncState;
            try
            {
                ns.EndWrite(ar);
                ns.Flush();
                ns.Close();
            }
            catch (Exception e)
            {
                IYTool.LogMsg("SndMsgError:" + e.Message, LogLevel.Error);
            }
        }
        #endregion

        /// <summary>
        /// Release Resource
        /// </summary>
        private void Clear()
        {
            if (closeCB != null)
            {
                closeCB();
            }
            skt.Close();
        }

        /// <summary>
        /// Connect network
        /// </summary>
        protected virtual void OnConnected()
        {
            IYTool.LogMsg("New Seesion Connected.", LogLevel.Info);
        }

        /// <summary>
        /// Receive network message
        /// </summary>
        protected virtual void OnReciveMsg(T msg)
        {
            IYTool.LogMsg("Receive Network Message.", LogLevel.Info);
        }

        /// <summary>
        /// Disconnect network
        /// </summary>
        protected virtual void OnDisConnected()
        {
            IYTool.LogMsg("Session Disconnected.", LogLevel.Info);
        }
    }
}
