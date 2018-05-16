﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        SceneController.Fade(SceneController.FADE_MODE.FADEMODE_IN);
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneController.mode != SceneController.FADE_MODE.FADEMODE_NONE)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneController.Fade(SceneController.FADE_MODE.FADEMODE_OUT, "GameMain");
        }
    }
}