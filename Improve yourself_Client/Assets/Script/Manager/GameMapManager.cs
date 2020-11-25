/****************************************************
	文件：GameMapManager.cs
	作者：NingWei
	日期：2020/09/07 11:28   	
	功能：场景管理器
*****************************************************/
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

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
        ILRuntimeManager.Instance.OpenUI(name);
        //UIManager.Instance.OpenUI<LoadingWindow>(ConStr.LoadingPanel, paramList:name);
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
        LoadingProgress = 0;
        int targetProgress = 0;
        //为内存考虑，先加载一个空场景，顶掉当前运行的场景
        if (FrameConstr.UseAssetAddress == AssetAddress.Addressable)
        {
            AsyncOperationHandle unloadScene = Addressables.LoadSceneAsync(ConStr.EmptyScene, LoadSceneMode.Single);
            yield return unloadScene.IsDone;
            AsyncOperationHandle asyncScene = Addressables.LoadSceneAsync(name);
            if (asyncScene.Result != null && !asyncScene.IsDone)
            {
                while ( asyncScene.PercentComplete < 0.9f)
                {
                    targetProgress = (int)asyncScene.PercentComplete * 100;
                    yield return endOfFrame;
                    //平滑过渡
                    while (LoadingProgress < targetProgress)
                    {
                        ++LoadingProgress;
                        yield return endOfFrame;
                    }
                }

                CurrentMapName = name;
                //自行加载剩余的10%
                targetProgress = 100;
                while (LoadingProgress < targetProgress - 1)
                {
                    ++LoadingProgress;
                    yield return endOfFrame;
                }

                LoadingProgress = 100;

                AlreadyLoadScene = true;

                LoadSceneOverCallBack?.Invoke();
            }
        }
        else { 
            AsyncOperation unloadScene = SceneManager.LoadSceneAsync(ConStr.EmptyScene, LoadSceneMode.Single);
            while (unloadScene != null && unloadScene.isDone)
            {
                yield return endOfFrame;
            }
            AsyncOperation asyncScene = SceneManager.LoadSceneAsync(name);
            if (asyncScene != null && !asyncScene.isDone)
            {
                asyncScene.allowSceneActivation = false;
                while (asyncScene.progress < 0.9f)
                {
                    targetProgress = (int)asyncScene.progress * 100;
                    yield return endOfFrame;
                    //平滑过渡
                    while (LoadingProgress < targetProgress)
                    {
                        ++LoadingProgress;
                        yield return endOfFrame;
                    }
                }

                CurrentMapName = name;
                //自行加载剩余的10%
                targetProgress = 100;
                while (LoadingProgress < targetProgress - 1)
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
