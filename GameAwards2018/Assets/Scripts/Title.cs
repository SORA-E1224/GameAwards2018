using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Title : MonoBehaviour
{
    [SerializeField]
    private Animator target;

    private Animator playerAnimator;

    [SerializeField, Range(0f, 500f)]
    private float walkspead;

    [SerializeField]
    private GameObject Marker;

    [SerializeField]
    private Collider TitleStop;

    [SerializeField]
    private Collider door;

    private int Idle, Stretch, Walk;
    private int currentState, prevState, doorState;
    private bool TControl, Move, LastWalk;
    private GameObject cameraObj;


    void Awake()
    {
        Idle = Animator.StringToHash("Base Layer.WAIT00");
        Stretch = Animator.StringToHash("Base Layer.WAIT01");
        Walk = Animator.StringToHash("Base Layer.GetUpAfter.Male_walking1");
    }

    public void GetUp()
    {
        AnimatorStateInfo anim = playerAnimator.GetCurrentAnimatorStateInfo(0);
        prevState = currentState;
        currentState = anim.fullPathHash;

        if (anim.fullPathHash == Idle)
        {
            playerAnimator.SetTrigger("Stretch");
        }
        else if (anim.fullPathHash == Stretch)
        {
            playerAnimator.SetTrigger("Wait");
        }
    }
    // Use this for initialization
    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        cameraObj = GetComponentInChildren<Camera>().gameObject;
        TControl = false;
        Move = false;
        LastWalk = false;
    }

    // Update is called once per frame
    void Update()
    {

        if (!SceneController.IsFade())
        {
            GetUp();
            if (currentState == Idle && prevState == Stretch)
            {
                SceneController.Fade(SceneController.FADE_MODE.FADEMODE_OUT);
            }
        }
        if (SceneController.mode == SceneController.FADE_MODE.FADEMODE_OUT_FINISH)
        {
            transform.localRotation = Quaternion.identity;
            transform.transform.position = new Vector3(0.5f, 0.065f, -10.75f);
            playerAnimator.ResetTrigger("Wait");
            playerAnimator.ResetTrigger("Stretch");
            playerAnimator.SetTrigger("GetUp");
            cameraObj.transform.rotation = Quaternion.identity;
            SceneController.Fade(SceneController.FADE_MODE.FADEMODE_IN);
        }


        if (currentState == Walk && Move == false)
        {
            transform.position += new Vector3(0f, 0f, walkspead * Time.deltaTime);
            cameraObj.transform.rotation = Quaternion.identity;
        }
        if (TControl == true)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                target.SetTrigger("character_nearby");
                playerAnimator.ResetTrigger("Stop");
                Move = true;

            }
        }
        if (Move == true && !LastWalk)
        {
            transform.position += new Vector3(Marker.transform.position.x - transform.position.x, 0f, Marker.transform.position.z - transform.position.z) / 200f;
            Vector3 currentRotation = transform.rotation.eulerAngles;
            transform.LookAt(Marker.transform.position);
            Vector3 targetRotation = transform.rotation.eulerAngles;
            Vector3 diff = targetRotation - currentRotation;
            transform.rotation = Quaternion.Euler(currentRotation + diff / 10f);

        }

        if (LastWalk)
        {
            transform.position += new Vector3(0f, 0f, walkspead * Time.deltaTime);
            Vector3 currentRotation = transform.rotation.eulerAngles;
            Vector3 diff = Vector3.zero - currentRotation;
            transform.rotation = Quaternion.Euler(currentRotation + diff / 10f);
        }
    }

    public void SetAnimator(string act)
    {
        playerAnimator.SetTrigger(act);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Trigger")
        {
            playerAnimator.SetTrigger("Stop");
            TControl = true;
        }
        else if (other.tag == "Door")
        {
            LastWalk = true;
        }
        else if (other.tag == "SceneChanger")
        {
            SceneController.Fade(SceneController.FADE_MODE.FADEMODE_OUT, "GameMain");
        }
    }

}
