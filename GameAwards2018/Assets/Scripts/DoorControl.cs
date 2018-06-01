using UnityEngine;

public class DoorControl : MonoBehaviour
{
    PlayerControl playerScript;
    Animator animator;
    int animParamID;
    int stateCloseHash, stateClosedHash;

    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();
        AnimatorControllerParameter animParam = animator.GetParameter(0);

        animParamID = animParam.nameHash;

        stateCloseHash = Animator.StringToHash("Base Layer.door_close");
        stateClosedHash = Animator.StringToHash("Base Layer.door_closed");
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void DoorAction()
    {
        bool getState = animator.GetBool(animParamID);
        animator.SetBool(animParamID, !getState);

    }

    public void DoorOpen()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (info.fullPathHash == stateCloseHash || info.fullPathHash == stateClosedHash)
        {
            animator.SetBool(animParamID, true);
        }
    }
}
