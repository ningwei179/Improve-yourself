/****************************************************
	文件：ServerRoot.cs
	作者：NingWei
	日期：2020/09/07 14:02   	
	功能：服务器目录
*****************************************************/
public class ServerRoot
{
    private static ServerRoot instance = null;

    public static ServerRoot Instance {
        get {
            if (instance == null)
                instance = new ServerRoot();
            return instance;
        }
    }

    public void Init() {
        //数据层TODO

        //服务层 TODO
        CacheSvc.Instance.Init();
        NetSvc.Instance.Init();

        //
    }

    public void Update() {
        NetSvc.Instance.Update();
    }
}
