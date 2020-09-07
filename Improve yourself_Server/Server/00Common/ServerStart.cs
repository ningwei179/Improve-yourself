/****************************************************
	文件：ServerStart.cs
	作者：NingWei
	日期：2020/09/04 15:04   	
	功能：服务器main方法
*****************************************************/
class ServerStart
{
    static void Main(string[] args)
    {
        ServerRoot.Instance.Init();
        

        while (true)
        {
            ServerRoot.Instance.Update();
        }
    }
}