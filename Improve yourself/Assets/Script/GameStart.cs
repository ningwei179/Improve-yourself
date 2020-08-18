using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    //private AudioSource m_Audio;
    //private AudioClip clip;
    private GameObject GameObj = null;
    void Awake()
    {
        GameObject.DontDestroyOnLoad(this);
        //加载AssetBundle配置文件
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        ResourceManager.Instance.Init(this);
        ObjectManager.Instance.Init(transform.Find("ResourcePoolTrs"), transform.Find("SceneTrs"));
        //m_Audio = this.GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //clip = ResourceManager.Instance.LoadResource<AudioClip>("Assets/GameData/Sounds/music_battle.mp3");
        //m_Audio.clip = clip;
        //m_Audio.Play();

        //ResourceManager.Instance.AsyncLoadResource("Assets/GameData/Sounds/music_battle.mp3",
        //    (string path, Object obj, object param1, object param2, object param3) =>
        //    {
        //        clip = obj as AudioClip;
        //        m_Audio.clip = clip;
        //        m_Audio.Play();
        //    }, LoadResPriority.RES_MIDDLE);
        //ObjectManager.Instance.InstantiateObjectAsync("Assets/GameData/Prefabs/C0001.prefab", (string path,Object obj,object param1, object param2, object param3) =>
        //{
        //    GameObj = GameObject.Instantiate(obj) as GameObject;
        //},LoadResPriority.RES_HIGHT,true);
        //ObjectManager.Instance.PreLoadGameObject("Assets/GameData/Prefabs/C0001.prefab", 20);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            //ResourceManager.Instance.PreloadRes("Assets/GameData/Sounds/music_battle.mp3");
            //ResourceManager.Instance.ReleaseResource(clip, true);
            //m_Audio.clip = null;
            //clip = null;
            ObjectManager.Instance.ReleaseObject(GameObj);
            GameObj = null;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            //ResourceManager.Instance.PreloadRes("Assets/GameData/Sounds/music_battle.mp3");
            //ResourceManager.Instance.ReleaseResource(clip, true);
            //m_Audio.clip = null;
            //clip = null;
            //GameObj = ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/C0001.prefab", true);
            ObjectManager.Instance.InstantiateObjectAsync("Assets/GameData/Prefabs/C0001.prefab", (string path, Object obj, object param1, object param2, object param3) =>
            {
                GameObj = obj as GameObject;
            }, LoadResPriority.RES_HIGHT, true);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            //ResourceManager.Instance.PreloadRes("Assets/GameData/Sounds/music_battle.mp3");
            //ResourceManager.Instance.ReleaseResource(clip, true);
            //m_Audio.clip = null;
            //clip = null;
            ObjectManager.Instance.ReleaseObject(GameObj, 0,true);
            GameObj = null;
        }
    }

    private void OnApplicationQuit()
    {
#if UNITY_EDITOR
        ResourceManager.Instance.ClearCache();
        Resources.UnloadUnusedAssets();
#endif
    }
}
