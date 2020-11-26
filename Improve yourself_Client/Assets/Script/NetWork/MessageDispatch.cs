/****************************************************
    文件：MessageDispatch.cs
	作者：NingWei
    日期：2020/9/7 17:54:8
	功能：服务器消息派遣
*****************************************************/


using IYProtocal;
namespace Improve
{

    public class MessageDispatch : Singleton<MessageDispatch>
    {
        public void Dispatch(NetMsg message)
        {
            if (message.rspLogin != null) { MessagePublisher.Instance.RaiseEvent(message.rspLogin); }
        }
    }
}