using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    private AudioSource m_Audio;
    private AudioClip clip;
    void Awake()
    {
        //加载AssetBundle配置文件
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        ResourceManager.Instance.Init(this);
        m_Audio = this.GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //clip = ResourceManager.Instance.LoadResource<AudioClip>("Assets/GameData/Sounds/music_battle.mp3");
        //m_Audio.clip = clip;
        //m_Audio.Play();

        ResourceManager.Instance.AsyncLoadResource("Assets/GameData/Sounds/music_battle.mp3",
            (string path, Object obj, object param1, object param2, object param3) =>
            {
                clip = obj as AudioClip;
                m_Audio.clip = clip;
                m_Audio.Play();
            }, LoadResPriority.RES_MIDDLE);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ResourceManager.Instance.ReleaseResource(clip, true);
            m_Audio.clip = null;
            clip = null;
        }
    }

    private void OnApplicationQuit()
    {
#if UNITY_EDITOR
        Resources.UnloadUnusedAssets();
#endif
    }
}
