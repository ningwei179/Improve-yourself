/****************************************************
    文件：PopUpWindow.cs
	作者：NingWei
    日期：2020/10/9 14:50:49
	功能：弹窗界面
*****************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;
using static CommonEnum;

public class PopUpWindow :Window
{
    private PopUpPanel m_Panel;

    private string m_SceneName;

    public override string PrefabName()
    {
        return "PopUpPanel.prefab";
    }

    PopUpPanelParams m_params;
    //回调
    public Action okOnClick { set; get; }
    public Action cancleOnClick { set; get; }
    public Action timeoutCallback { set; get; }
    public Action closeOnClick { set; get; }

    private const float TIMEOUT = 4f;   //默认的超时时间
    private int leftTime;
    private string strCancleTitle;  //取消按钮的字符串

    public override void Awake(params object[] paramList)
    {
        m_Panel = GameObject.GetComponent<PopUpPanel>();
        AddButtonClickListener(m_Panel.BtnClose, BtnCloseOnClick);
        AddButtonClickListener(m_Panel.BtnLeft, BtnLeftOnClick);
        AddButtonClickListener(m_Panel.BtnRight, BtnRightOnClick);
        AddButtonClickListener(m_Panel.BtnMiddle, BtnMiddleOnClick);

    }

    public override void OnShow(params object[] paramList)
    {
        m_params = paramList[0] as PopUpPanelParams;

        okOnClick = m_params.OkAction;
        cancleOnClick = m_params.CancleAction;
        timeoutCallback = m_params.TimeoutAction;
        closeOnClick = m_params.CloseAction;

        this.strCancleTitle = m_params.CancleTitle;
        m_Panel.Title.text = m_params.Title;
        m_Panel.Msg.text = m_params.Msg;
        m_Panel.BtnLeft.gameObject.SetActive(false);
        m_Panel.BtnRight.gameObject.SetActive(false);
        m_Panel.BtnMiddle.gameObject.SetActive(false);
        if (m_params.StyleType == MessageBoxStyleType.OK)
        {
            m_Panel.BtnMiddle.gameObject.SetActive(true);
            if (m_params.OkTitle.Length > 0)
                m_Panel.BtnMiddle.GetComponentInChildren<Text>().text = m_params.OkTitle;
            else
                m_Panel.BtnMiddle.GetComponentInChildren<Text>().text = "确定";//LocalizationManager.Instance.GetString("PrefabText3");
        }
        else if (m_params.StyleType == MessageBoxStyleType.OK_CANCLE)
        {
            m_Panel.BtnLeft.gameObject.SetActive(true);
            m_Panel.BtnRight.gameObject.SetActive(true);
            if (m_params.CancleTitle.Length > 0)
                m_Panel.BtnLeft.GetComponentInChildren<Text>().text = m_params.CancleTitle;
            else
                m_Panel.BtnLeft.GetComponentInChildren<Text>().text = "取消";//LocalizationManager.Instance.GetString("PrefabText3");

            if (m_params.OkTitle.Length > 0)
                m_Panel.BtnRight.GetComponentInChildren<Text>().text = m_params.OkTitle;
            else
                m_Panel.BtnRight.GetComponentInChildren<Text>().text = "确定";//LocalizationManager.Instance.GetString("PrefabText3");
        }

        m_Panel.BtnClose.gameObject.SetActive(!m_params.HideCloseBtn);

        this.leftTime = m_params.RemainTime;
        bool isNeedTimer = (m_params.TimeoutAction != null && m_params.RemainTime > 0 ? true : false);
        if (isNeedTimer)
        {
            TimerController.Instance.AddTimeTask((id) =>
            {
                leftTime -= 1;
                m_Panel.BtnLeft.GetComponentInChildren<Text>().text = string.Format("{0}（{1}）", this.strCancleTitle, leftTime);
            },0, TimeUnit.Second , leftTime);
        }
    }

    private void BtnMiddleOnClick()
    {
        OnClose();
        okOnClick?.Invoke();
    }

    private void BtnRightOnClick()
    {
        OnClose();
        okOnClick?.Invoke();
    }

    private void BtnLeftOnClick()
    {
        OnClose();
        cancleOnClick?.Invoke();
    }

    private void BtnCloseOnClick()
    {
        OnClose();
        closeOnClick?.Invoke();
    }

    public override void OnClose()
    {
        base.OnClose();
    }

    public override void OnUpdate()
    {
        if (m_Panel == null)
            return;
    }
}