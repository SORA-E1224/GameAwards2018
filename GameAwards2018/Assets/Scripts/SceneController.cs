using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneController : MonoBehaviour
{
    public enum FADE_MODE
    {
        FADEMODE_NONE = 0,
        FADEMODE_IN = 1,
        FADEMODE_OUT = 2
    }

    static public FADE_MODE mode { get; private set; }
    static private string nextScene;

    [SerializeField]
    [Range(0.1f, 10.0f)]
    private float fadeTime;
    private float fadeCount;

    private Material mat;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        Object obj = Instantiate(Resources.Load("Prefabs/pre_SceneController"));
        DontDestroyOnLoad(obj);
    }

    // Use this for initialization
    void Awake()
    {
        mat = GetComponentInChildren<Image>().material;
        mode = FADE_MODE.FADEMODE_NONE;
        nextScene = null;
        fadeCount = fadeTime;
        mat.SetFloat("_Percent", 1.0f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        if (mode == FADE_MODE.FADEMODE_NONE)
        {
            return;
        }

        bool IsLoad = false;

        if (mode == FADE_MODE.FADEMODE_IN)
        {
            fadeCount -= Time.deltaTime;
            if (fadeCount < 0)
            {
                fadeCount = 0.0f;
                mode = FADE_MODE.FADEMODE_NONE;
                if (!string.IsNullOrEmpty(nextScene))
                {
                    IsLoad = true;
                }
            }
        }
        else if (mode == FADE_MODE.FADEMODE_OUT)
        {
            fadeCount += Time.deltaTime;
            if (fadeCount > fadeTime)
            {
                fadeCount = fadeTime;
                mode = FADE_MODE.FADEMODE_NONE;
                if (!string.IsNullOrEmpty(nextScene))
                {
                    IsLoad = true;
                }
            }
        }

        mat.SetFloat("_Percent", fadeCount / fadeTime);

        if (!IsLoad)
        {
            return;
        }

        string nextSceneBuf = nextScene;
        nextScene = null;
        SceneManager.LoadScene(nextSceneBuf);
    }

    static public void Fade(FADE_MODE fadeMode, string sceneName = "")
    {
        mode = fadeMode;

        if (string.IsNullOrEmpty(sceneName))
        {
            return;
        }

        nextScene = sceneName;

    }

    static public void Load(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }
}
