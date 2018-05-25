using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorControl : MonoBehaviour
{
    PlayerControl playerScript;
    Animator animator;
    int animParamID;
    

    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();
        AnimatorControllerParameter animParam = animator.GetParameter(0);
        
        animParamID = animParam.nameHash;

        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();

        playerScript.OnTriggerActionTriggerEvent += delegate (object sender, EventArgsPlayer e)
        {
            DoorAction(e.col);
        };

    }

    // Update is called once per frame
    void Update()
    {

    }

    void DoorAction(Collider col)
    {
        if (gameObject != col.gameObject)
        {
            return;
        }

        bool getState = animator.GetBool(animParamID);
        animator.SetBool(animParamID, !getState);

    }
}
