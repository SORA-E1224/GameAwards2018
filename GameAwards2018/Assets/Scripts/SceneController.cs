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

    static public Text debugText;

    [SerializeField]
    private Material matFade;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        Object obj = Instantiate(Resources.Load("Prefabs/pre_SceneController"));
        DontDestroyOnLoad(obj);
    }

    private int frameCount = 0;
    private float prevTime = 0.0f;

    // Use this for initialization
    void Awake()
    {
        debugText = GetComponentInChildren<Text>();
        mode = FADE_MODE.FADEMODE_NONE;
        nextScene = null;
        fadeCount = fadeTime;
        matFade.SetFloat("_Percent", 0.0f);
    }

    private void Start()
    {
        if (!Debug.isDebugBuild)
        {
            debugText.gameObject.SetActive(false);
            return;
        }

        frameCount = 0;
        prevTime = 0.0f;
    }

    private void Update()
    {
        if (Debug.isDebugBuild)
        {
            frameCount++;
            float time = Time.realtimeSinceStartup - prevTime;

            if (time >= 0.5f)
            {
                debugText.text = string.Format("{0:F2}fps", frameCount / time);

                frameCount = 0;
                prevTime = Time.realtimeSinceStartup;
            }
        }

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

        matFade.SetFloat("_Percent", fadeCount / fadeTime);

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
