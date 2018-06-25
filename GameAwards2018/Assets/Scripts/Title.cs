using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Title : MonoBehaviour
{
    [SerializeField]
    private Animator target;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (SceneController.mode == SceneController.FADE_MODE.FADEMODE_NONE) {
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            target.SetTrigger("character_nearby");
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            target.ResetTrigger("character_nearby");
        }

    }
}
