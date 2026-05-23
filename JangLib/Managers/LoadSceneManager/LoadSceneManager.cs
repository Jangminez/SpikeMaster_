using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SceneType
{
    TitleScene,
    MainScene,
    PlayScene,
    LoadScene
}

public static class SceneName
{
    public const string PLAY_SCENE = "PlayScene";
    public const string TITLE_SCENE = "TitleScene";
    public const string MAIN_SCENE = "MainScene";
    public const string LOAD_SCENE = "LoadScene";

    public static string GetSceneByType(SceneType type)
    {
        return type switch
        {
            SceneType.PlayScene => PLAY_SCENE,
            SceneType.TitleScene => TITLE_SCENE,
            SceneType.MainScene => MAIN_SCENE,
            SceneType.LoadScene => LOAD_SCENE,
            _ => string.Empty
        };
    }
}

public class LoadSceneManager : MonoBehaviour
{
    [Header("Progress Bar")]
    [SerializeField] Slider _progressSld;

    static string nextScene;

    public static void LoadSceneAsync(SceneType type)
    {
        nextScene = SceneName.GetSceneByType(type);
        SceneManager.LoadScene(SceneName.LOAD_SCENE);
    }

    private void Start()
    {
        StartCoroutine(LoadSceneCO());
    }

    private IEnumerator LoadSceneCO()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float timer = 0f;

        while (!op.isDone)
        {
            yield return null;

            if (op.progress > 0.9f)
            {
                _progressSld.value = op.progress;
            }
            else
            {
                timer += Time.unscaledDeltaTime;
                _progressSld.value = Mathf.Lerp(0.2f, 1f, timer);

                if (_progressSld.value >= 1f)
                {
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }
}