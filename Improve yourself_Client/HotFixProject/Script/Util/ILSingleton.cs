/****************************************************
	文件：Singleton.cs
	作者：NingWei
	日期：2020/09/07 11:32   	
	功能：单例工厂
*****************************************************/

public class ILSingleton<T> where T:new()
{
    private static T m_Instance;

    public static T Instance
    {
        get
        {
            if(m_Instance == null)
                m_Instance = new T();
            return m_Instance;
        }
    }
}
