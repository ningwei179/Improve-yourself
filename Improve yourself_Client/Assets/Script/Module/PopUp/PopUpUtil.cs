/****************************************************
    文件：PopUpUtil.cs
	作者：NingWei
    日期：2020/10/9 15:10:44
	功能：弹窗工具类
*****************************************************/

using System;
using UnityEngine;
using static CommonEnum;

public class PopUpPanelParams
{
    public string Title;
    public string Msg;
    public MessageBoxStyleType StyleType;
    public string OkTitle;
    public Action OkAction;
    public string CancleTitle;
    public Action CancleAction;
    public int RemainTime;
    public Action TimeoutAction;
    public Action CloseAction;
    public bool HideCloseBtn;

    public void Init(string title, string msg, MessageBoxStyleType styleType, string okTitle = "",
        Action okAction = null,
        string cancleTitle = "", System.Action cancleAction = null, bool hideCloseBtn = false,
        Action closeAction = null, int remainTime = 0,
        Action timeoutAction = null)
    {
        Title = title;//LocalizationManager.Instance.GetString(title);
        Msg = msg;// LocalizationManager.Instance.GetString(msg);
        StyleType = styleType;
        OkTitle = okTitle;// LocalizationManager.Instance.GetString(okTitle);
        OkAction = okAction;
        CancleTitle = cancleTitle;// LocalizationManager.Instance.GetString(cancleTitle);
        CancleAction = cancleAction;
        RemainTime = remainTime;
        TimeoutAction = timeoutAction;
        CloseAction = closeAction;
        HideCloseBtn = hideCloseBtn;
    }
}

public class PopUpUtil
{
    //调用方法模板
    //把需要的参数传入，不需要的参数删掉就行了，同时所有文本参数都自动翻译了，文本参数可以直接填多语言key
    //PopUpUtil.OpenPopUp(title:"", msg:"", styleType: MessageBoxStyleType.OK, okTitle:"", okAction:null, cancelTitle:"", cancelAction:null, 
    //        hideCloseBtn:false, closeAction:null, remainTime:0, timeoutAction:null, showFullMask:false, blowGuide:false);


    /// <summary>
    /// 单按钮，提示信息
    /// </summary>
    /// <param name="title"></param>
    /// <param name="msg"></param>
    internal static void OpenPopUpOK(string title, string msg)
    {
        OpenPopUpOK(title, msg, "确定");
    }

    /// <summary>
    /// 单按钮，提示信息，含按钮点击事件， 标题默认PrefabText389 确定按钮文字默认PrefabText3
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="okAction"></param>
    internal static void OpenPopUpOK(string msg, Action okAction = null, bool pushToStack = false)
    {
        OpenPopUpOK("提示", msg, "确定", okAction, pushToStack);
    }

    /// <summary>
    /// 单按钮，提示信息
    /// </summary>
    /// <param name="title"></param>
    /// <param name="msg"></param>
    internal static void OpenPopUpOK(string title, string msg, string okTitle, Action okAction = null, bool pushToStack = false)
    {
        PopUpPanelParams m_Params = new PopUpPanelParams();
        m_Params.Init(title, msg, MessageBoxStyleType.OK, okTitle, okAction, null, null, false, null, 0, null);
        UIManager.Instance.ShowUI(ConStr.PopUpPanel, true, AssetAddress.Resources, paramList:m_Params);
    }

    /// <summary>
    /// 双按钮，有确认的回调
    /// </summary>
    /// <param name="title"></param>
    /// <param name="msg"></param>
    /// <param name="okAction"></param>
    internal static void OpenPopUp(string title, string msg, Action okAction)
    {
        OpenPopUp(title, msg, okAction, "确定");
    }

    /// <summary>
    /// 双按钮，确认按钮可以设置文字
    /// </summary>
    /// <param name="title"></param>
    /// <param name="msg"></param>
    /// <param name="okAction"></param>
    /// <param name="okTitle"></param>
    internal static void OpenPopUp(string title, string msg, Action okAction, string okTitle)
    {
        PopUpPanelParams m_Params = new PopUpPanelParams();
        m_Params.Init(title, msg, MessageBoxStyleType.OK_CANCLE, okTitle, okAction, "取消", null, false, null, 0, null);
        UIManager.Instance.ShowUI(ConStr.PopUpPanel, true, AssetAddress.Resources, paramList: m_Params);
    }

    /// <summary>
    /// 双按钮，
    /// </summary>
    /// <param name="title"></param>
    /// <param name="msg"></param>
    /// <param name="okAction"></param>
    /// <param name="okTitle"></param>
    internal static void OpenPopUp(string title, string msg, Action okAction, string okTitle, string cancleTitle = "", System.Action cancleAction = null)
    {
        PopUpPanelParams m_Params = new PopUpPanelParams();
        m_Params.Init(title, msg, MessageBoxStyleType.OK_CANCLE, okTitle, okAction, "取消", null, false, null, 0, null);
        UIManager.Instance.ShowUI(ConStr.PopUpPanel, true, AssetAddress.Resources, paramList: m_Params);
    }

    /// <summary>
    /// 双按钮，
    /// </summary>
    /// <param name="title"></param>
    /// <param name="msg"></param>
    /// <param name="okAction"></param>
    /// <param name="cancleAction"></param>
    internal static void OpenPopUp(string title, string msg, Action okAction, Action cancelAction = null)
    {
        PopUpPanelParams m_Params = new PopUpPanelParams();
        m_Params.Init(title, msg, MessageBoxStyleType.OK_CANCLE, "确定", okAction, "取消", cancelAction, false, null, 0, null);
        UIManager.Instance.ShowUI(ConStr.PopUpPanel, true, AssetAddress.Resources, paramList: m_Params);
    }

    /// <summary>
    /// 双按钮，没有关闭x按钮
    /// </summary>
    /// <param name="title"></param>
    /// <param name="msg"></param>
    /// <param name="okAction"></param>
    /// <param name="cancleAction"></param>
    internal static void OpenPopUpX(string title, string msg, Action okAction, Action cancelAction = null)
    {
        PopUpPanelParams m_Params = new PopUpPanelParams();
        m_Params.Init(title, msg, MessageBoxStyleType.OK_CANCLE, "确定", okAction, "取消", cancelAction, true, null, 0, null);
        UIManager.Instance.ShowUI(ConStr.PopUpPanel, true, AssetAddress.Resources, paramList: m_Params);
    }

    /// <summary>
    /// 打开弹窗
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="msg">文本</param>
    /// <param name="styleType">类型中间按钮，左右按钮，没有按钮</param>
    /// <param name="okTitle">右边，或者中间按钮文字</param>
    /// <param name="okAction">右边，或者中间按钮回调</param>
    /// <param name="cancleTitle">左边按钮文字</param>
    /// <param name="cancleAction">左边按钮回调</param>
    /// <param name="hideCloseBtn">是否隐藏右上角的关闭按钮</param>
    /// <param name="closeAction">关闭按钮回调</param>
    /// <param name="remainTime">左边按钮能按的倒计时</param>
    /// <param name="timeoutAction">左边按钮倒计时结束后的回调</param>
    /// <param name="showFullMask">是否显示遮罩</param>
    /// <param name="blowGuide"></param>
    internal static void OpenPopUp(string title, string msg, MessageBoxStyleType styleType = MessageBoxStyleType.OK, string okTitle = "", System.Action okAction = null,
        string cancelTitle = "", System.Action cancelAction = null, bool hideCloseBtn = false, System.Action closeAction = null, int remainTime = 0,
        System.Action timeoutAction = null)
    {
        PopUpPanelParams m_Params = new PopUpPanelParams();
        m_Params.Init(title, msg, MessageBoxStyleType.OK_CANCLE, okTitle, okAction, cancelTitle, cancelAction, hideCloseBtn, closeAction, remainTime, timeoutAction);
        UIManager.Instance.ShowUI(ConStr.PopUpPanel, true, AssetAddress.Resources, paramList: m_Params);
    }
}