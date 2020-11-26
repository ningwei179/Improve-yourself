using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
namespace Improve
{

    public class LoadFromFileExample : MonoBehaviour
{
    private string rolePath = "AssetBundles/role";  //模型路径
    private string materialPath = "AssetBundles/materials";   //材质路径
    private string texturePath = "AssetBundles/texture";   //贴图路径
    private string httpPath = @"http://localhost/AssetBundles/";  //mainfestAb路径
    private AssetBundle role;   //角色assetBundle
    private AssetBundle material;   //角色assetBundle
    private AssetBundle texture;   //角色assetBundle

    //官网说依赖包要先加载，实际跑下来，先实例化对象，再加载依赖包也是可行的，
    //这种做法可能会导致先实例化的对象丢失一些依赖资源，比如材质，贴图丢失一会儿
    //模型变红，变白，过一会才显示正常
    // Start is called before the first frame update
    void Start()
    {
        //第一种加载AB的方式 LoadFromMemoryAsync
        //异步加载
        //StartCoroutine(LoadFromMemoryAsync());
        //同步加载
        //role = AssetBundle.LoadFromMemory(File.ReadAllBytes(rolePath));

        //第二种加载AB的方式 LoadFromFileAsync
        //异步加载
        //StartCoroutine(LoadFromFileAsync());
        //同步加载
        //material = AssetBundle.LoadFromFile(materialPath);

        //第三种加载AB的方式 WWW
        //StartCoroutine(LoadFromWWW());

        //实例化角色
        //GameObject wallPrefab = role.LoadAsset<GameObject>("C0002");
        //Instantiate(wallPrefab);

        //第四种加载AB的方式 UnityWebRequest
        //StartCoroutine(LoadFromWebRequest("AssetBundles"));
    }

    //异步加载角色
    IEnumerator LoadFromMemoryAsync()
    {
        AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(rolePath));
        yield return request;
        role = request.assetBundle;
        //实例化角色
        GameObject wallPrefab = role.LoadAsset<GameObject>("C0002");
        Instantiate(wallPrefab);
    }

    /// <summary>
    /// 异步加载材质
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadFromFileAsync()
    {
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(materialPath);
        yield return request;
        material = request.assetBundle;
    }

    /// <summary>
    /// WWW加载贴图
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadFromWWW()
    {
        WWW www = WWW.LoadFromCacheOrDownload(@"file:/D:\Improve yourself\AssetBundles\texture", 1);
        yield return www;
        if (string.IsNullOrEmpty(www.error) == false)
        {
            Debug.Log(www.error); yield break;
        }
        texture = www.assetBundle;
    }

    /// <summary>
    /// WWW加载贴图
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadFromWebRequest(string bundleName)
    {
        using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(httpPath + bundleName))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                AssetBundle ab = DownloadHandlerAssetBundle.GetContent(uwr);
                if (bundleName == "AssetBundles")
                {
                    AssetBundleManifest manifest = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                    string[] strs = manifest.GetAllAssetBundles();
                    foreach (string name in strs)
                    {
                        print(name);
                        StartCoroutine(LoadFromWebRequest(name));
                    }
                }
                else if (bundleName == "role")
                {
                    //实例化角色
                    GameObject wallPrefab = ab.LoadAsset<GameObject>("C0002");
                    Instantiate(wallPrefab);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    }
}
