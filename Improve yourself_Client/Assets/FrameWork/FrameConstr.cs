/****************************************************
    文件：FrameConstr.cs
	作者：NingWei
    日期：2020/10/10 14:42:12
	功能：框架用的常量字符串
*****************************************************/
namespace Improve
{
    /// <summary>
    /// UI类型
    /// </summary>
    public enum UIType { 
        UGUI,
        FariyGUI,
    }

    public enum ILRuntimeState { 
        Open,
        Close
    }
    /// <summary>
    /// 资产地址
    /// </summary>
    public enum AssetAddress
    {
        Resources,          //从resource加载
        AssetBundle,        //从AssetBundle加载
        Addressable         //从Addressable加载
    }

    /// <summary>
    /// 代码热更方式
    /// 可以俩个一起用
    /// 写demo先分开
    /// </summary>
    public enum CodeHotType
    {
        InjectFix,          //用InjectFix
        ILRuntime,          //用ILRuntime
    }

    public class FrameConstr
    {
        public const ILRuntimeState ILState = ILRuntimeState.Open;

        /// <summary>
        /// 资产来源
        /// </summary>
        public const AssetAddress UseAssetAddress = AssetAddress.Addressable;

        /// <summary>
        /// 热更方式
        /// </summary>
        public const CodeHotType HotType = CodeHotType.ILRuntime;


        /// <summary>
        /// AB包的加密秘钥
        /// </summary>
        public const string m_ABSecretKey = "Improve";

        /// <summary>
        /// 资源服务器地址
        /// </summary>
        public const string m_ResServerIp = "http://127.0.0.1/";
    }
}