/****************************************************
	文件：UIManager.cs
	作者：NingWei
	日期：2020/09/07 11:34   	
	功能：UI管理器
*****************************************************/

public class UIRegister : ILSingleton<UIRegister>
{
    /// <summary>
    /// 注册所有的UI
    /// </summary>
    public void RegisterAllUI()
    {
        UIManager.Instance.Register<PopUpWindow>(ConStr.PopUpPanel);
        UIManager.Instance.Register<LoadingWindow>(ConStr.LoadingPanel);
        UIManager.Instance.Register<MenuWindow>(ConStr.MenuPanel);
        UIManager.Instance.Register<MainWindow>(ConStr.MainPanel);
    }
}
