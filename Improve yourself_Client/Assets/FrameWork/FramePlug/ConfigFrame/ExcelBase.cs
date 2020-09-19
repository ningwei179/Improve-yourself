/****************************************************
    文件：ExcelBase.cs
	作者：NingWei
    日期：2020/9/8 13:16:39
	功能：Nothing
*****************************************************/


public class ExcelBase 
{
#if UNITY_EDITOR
    public virtual void Construction() { }
#endif

    public virtual void Init() { }
}