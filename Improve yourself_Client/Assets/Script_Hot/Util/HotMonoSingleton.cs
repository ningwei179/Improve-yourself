using UnityEngine;
namespace HotFixProject
{
    public class HotMonoSingleton<T> : MonoBehaviour where T : HotMonoSingleton<T>
    {
        protected static T instance;

        public static T Instance
        {
            get { return instance; }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = (T)this;
            }
            else
            {
                Debug.LogError("Get a second instance of this class" + this.GetType());
            }
        }
    }
}