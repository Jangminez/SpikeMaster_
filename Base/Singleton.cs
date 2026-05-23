using UnityEngine;
using UnityEngine.SceneManagement;

namespace JangLib
{
    /// <summary>
    /// 제네릭 싱글톤 클래스입니다. 단일 인스턴스를 보장하며, 
    /// 인스턴스가 처음 필요할 때 생성됩니다.
    /// T는 매개변수 없는 생성자를 가진 클래스여야 합니다.
    /// </summary>
    public abstract class Singleton<T> where T : class, new()
    {
        protected static T _instance;
        private static readonly object _lock = new object();
        private static bool applicationIsQuitting;

        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    EditorLog.LogWarning($"[Singleton] '{typeof(T)}' 인스턴스가 애플리케이션 종료 시 이미 파괴되었습니다. null을 반환합니다.");
                    return null;
                }

                lock (_lock)
                {
                    return _instance ??= new T();
                }
            }
        }

        public static bool IsCreatedInstance() => _instance != null;
        public static void OnApplicationQuit() => applicationIsQuitting = true;
    }

    /// <summary>
    /// MonoBehaviour를 상속받는 클래스에 대해 단일 인스턴스를 보장하는 Unity 싱글톤 클래스입니다.
    /// 중복 인스턴스 생성을 방지하고, 애플리케이션 종료 시 고스트 객체가 생성되지 않도록 처리합니다.
    /// 싱글톤 동작을 강제하려면 싱글톤 클래스에 `protected T() {}`를 추가하세요.
    /// OnApplicationQuit()과 OnDestroy() 메서드를 오버라이드하여 애플리케이션 종료 시 처리 및 파괴 시 처리를 추가할 수 있습니다.
    /// OnApplicationQuit > OnDestroy 순서로 호출됩니다.
    /// </summary>
    public class SingletonWithMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool applicationIsQuitting;

        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    EditorLog.LogWarning($"[Singleton] '{typeof(T)}' 인스턴스가 애플리케이션 종료 시 이미 파괴되었습니다. null을 반환합니다.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindInstanceInScene();

                        if (_instance == null)
                        {
                            GameObject singletonObject = new GameObject($"{typeof(T)} (Singleton)");
                            _instance = singletonObject.AddComponent<T>();
                            DontDestroyOnLoad(singletonObject);
                        }
                        else
                        {
                            DontDestroyOnLoad(_instance.gameObject);
                            EditorLog.Log($"[Singleton] 기존 인스턴스를 사용합니다: {_instance.gameObject.name}");
                        }
                    }

                    return _instance;
                }
            }
        }

        private static T FindInstanceInScene()
        {
            foreach (var rootObj in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var instance = rootObj.GetComponentInChildren<T>();
                if (instance != null)
                    return instance;
            }

            return null;
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                EditorLog.LogWarning($"[Singleton] 중복된 인스턴스가 발견되었습니다. 기존 인스턴스를 유지하고 새 오브젝트를 삭제합니다: {gameObject.name}");
                Destroy(gameObject);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
        
        public static bool IsInited() => _instance != null;
        public static bool IsApplicationQuitting() => applicationIsQuitting;
    }
    
    /// <summary>
    /// 씬 내에서 유지되는 싱글톤 클래스입니다.
    /// </summary>
    public class SingletonWithScene<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }
        protected virtual void Awake()
        {
            if (Instance)
            {
                DestroyImmediate(gameObject);
                return;
            }
            Instance = this as T;
        }
    }
}