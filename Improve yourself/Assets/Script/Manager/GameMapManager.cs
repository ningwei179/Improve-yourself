using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// 游戏场景管理器
/// </summary>
public class GameMapManager : Singleton<GameMapManager>
{
    /// <summary>
    /// 加载场景完成回调
    /// </summary>
    public Action LoadSceneOverCallBack;

    /// <summary>
    /// 加载场景前回调
    /// </summary>
    public Action LoadSceneEnterCallBack;

    /// <summary>
    /// 当前场景的名称
    /// </summary>
    public string CurrentMapName { get; set; }

    //场景加载是否完成
    public bool AlreadyLoadScene { get; set; }

    /// <summary>
    /// 切换场景的进度条
    /// </summary>
    public static int LoadingProgress = 0;

    WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();

    private MonoBehaviour m_Mono;
    /// <summary>
    /// 场景管理初始化
    /// </summary>
    /// <param name="mono"></param>
    public void Init(MonoBehaviour mono)
    {
        m_Mono = mono;
    }

    /// <summary>
    /// 加载场景
    /// </summary>
    /// <param name="name"></param>
    public void LoadScene(string name)
    {
        LoadingProgress = 0;
        m_Mono.StartCoroutine(LoadSceneAsync(name));

        //加载场景的时候打开loadingUI
        UIManager.Instance.PopUpWindow(ConStr.LoadingPanel, true, name);
    }

    /// <summary>
    /// 设置场景环境
    /// </summary>
    /// <param name="name"></param>
    void SetSceneSetting(string name)
    {
        //设置各种场景环境，可以根据配表来
    }

    IEnumerator LoadSceneAsync(string name)
    {
        LoadSceneEnterCallBack?.Invoke();

        ClearCache();
        AlreadyLoadScene = false;
        //为内存考虑，先加载一个空场景，顶掉当前运行的场景
        AsyncOperation unloadScene = SceneManager.LoadSceneAsync(ConStr.EmptyScene, LoadSceneMode.Single);
        while (unloadScene != null && unloadScene.isDone)
        {
            yield return endOfFrame;
        }

        LoadingProgress = 0;
        int targetProgress = 0;
        AsyncOperation asyncScene = SceneManager.LoadSceneAsync(name);
        if (asyncScene != null && !asyncScene.isDone)
        {
            asyncScene.allowSceneActivation = false;
            while (asyncScene.progress<0.9f)
            {
                targetProgress = (int)asyncScene.progress *100;
                yield return endOfFrame;
                //平滑过渡
                while (LoadingProgress< targetProgress)
                {
                    ++LoadingProgress;
                    yield return endOfFrame;
                }
            }

            CurrentMapName = name;
            //自行加载剩余的10%
            targetProgress = 100;
            while (LoadingProgress < targetProgress - 2)
            {
                ++LoadingProgress;
                yield return endOfFrame;
            }

            LoadingProgress = 100;

            asyncScene.allowSceneActivation = true;

            AlreadyLoadScene = true;

            LoadSceneOverCallBack?.Invoke();
        }
    }

    /// <summary>
    /// 跳场景需要清除的东西
    /// </summary>
    private void ClearCache()
    {
        ObjectManager.Instance.ClearCache();
        ResourceManager.Instance.ClearCache();
    }
}
