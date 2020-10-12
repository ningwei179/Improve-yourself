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
    private float leftTime;
    private string strCancleTitle;  //取消按钮的字符串

    public override void Awake(params object[] paramList)
    {
        m_Panel = GameObject.GetComponent<PopUpPanel>();
        AddButtonClickListener(m_Panel.BtnClose, BtnCloseOnClick);
        AddButtonClickListener(m_Panel.BtnLeft, BtnLeftOnClick);
        AddButtonClickListener(m_Panel.BtnRight, BtnRightOnClick);
        AddButtonClickListener(m_Panel.BtnMiddle, BtnMiddleOnClick);

    }

    private Timer CountDownTimer;
    /// <summary>
    /// 开始倒计时
    /// </summary>
    public void StartCountDown()
    {
        if (CountDownTimer != null)
        {
            CountDownTimer.stop();
            TimerController.Instance.deleteTimer(CountDownTimer);
        }
        CountDownTimer = TimerController.Instance.createFixedTimer(1);
        CountDownTimer.addTimerEventListener(UpdateChatMessageCallBack);
        CountDownTimer.start();
    }

    /// <summary>
    /// 更新聊天消息计时器回调
    /// </summary>
    /// <param name="timer"></param>
    public void UpdateChatMessageCallBack(Timer timer)
    {
        leftTime -= 1;
        m_Panel.BtnLeft.GetComponentInChildren<Text>().text = string.Format("{0}（{1}）", this.strCancleTitle, leftTime);
        if (leftTime <= 0)
        {
            if (CountDownTimer != null)
            {
                CountDownTimer.stop();
                TimerController.Instance.deleteTimer(CountDownTimer);
            }
            if (timeoutCallback != null)
            {
                CloseUI();
                timeoutCallback();
            }
        }
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
            StartCountDown();
        }
    }

    private void BtnMiddleOnClick()
    {
        CloseUI();
        okOnClick?.Invoke();
    }

    private void BtnRightOnClick()
    {
        CloseUI();
        okOnClick?.Invoke();
    }

    private void BtnLeftOnClick()
    {
        CloseUI();
        cancleOnClick?.Invoke();
    }

    private void BtnCloseOnClick()
    {
        CloseUI();
        closeOnClick?.Invoke();
    }

    private void CloseUI() {
        UIManager.Instance.CloseWindow(ConStr.PopUpPanel);
    }

    public override void OnUpdate()
    {
        if (m_Panel == null)
            return;
    }
}