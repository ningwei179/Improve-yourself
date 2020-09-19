/****************************************************
    文件：PlayerContext.cs
	作者：NingWei
    日期：2020/9/7 18:8:4
	功能：服务器返回的消息内容
*****************************************************/


using Protocal;
/// <summary>
/// 服务器返回的消息内容
/// </summary>
public partial class MessageContext : Singleton<MessageContext>
{
    private bool _isServerMsgRegistered = false;

    public void RegisterServerMessage()
    {
        if (_isServerMsgRegistered)
            return;
        MessagePublisher.Instance.Subscribe<RspLogin>(this.RspLogin);

        _isServerMsgRegistered = true;
    }
}