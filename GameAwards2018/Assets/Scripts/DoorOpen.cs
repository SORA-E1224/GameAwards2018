using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    enum DOOR_STATE
    {
        DOOR_OPENED,
        DOOR_CLOSED,
        DOOR_OPENING,
        DOOR_CLOSING
    }
    private DOOR_STATE state;

    PlayerControl playerScript;
    uint playerCount;

    // Use this for initialization
    void Start()
    {
        playerCount = 0;
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();

        playerScript.OnTriggerStayEvent += delegate (object sender, EventArgsPlayer e)
        {
            DoorOpenAction(e.col);
        };

        playerScript.OnTriggerEnterEvent += delegate (object sender, EventArgsPlayer e)
        {
            IsDoorOpening(e.col);
        };

        playerScript.OnTriggerExitEvent += delegate (object sender, EventArgsPlayer e)
        {
            IsDoorClosing(e.col);
        };

        state = DOOR_STATE.DOOR_CLOSED;

    }

    // Update is called once per frame
    void Update()
    {
        Transform hingeTransform = transform.Find("Hinge");
        Vector3 posBuf = hingeTransform.localPosition;

        if (state == DOOR_STATE.DOOR_OPENING)
        {
            posBuf.y += Time.deltaTime * 5.0f;
            if (posBuf.y > 5.0f)
            {
                posBuf.y = 5.0f;
                state = DOOR_STATE.DOOR_OPENED;
            }
        }

        if (state == DOOR_STATE.DOOR_CLOSING)
        {
            posBuf.y -= Time.deltaTime * 5.0f;
            if (posBuf.y < 0.0f)
            {
                posBuf.y = 0.0f;
                state = DOOR_STATE.DOOR_CLOSED;
            }
        }

        hingeTransform.localPosition = posBuf;
    }

    void DoorOpenAction(Collider col)
    {
        if (gameObject != col.gameObject)
        {
            return;
        }

        if (state == DOOR_STATE.DOOR_OPENING || state == DOOR_STATE.DOOR_OPENED)
        {
            return;
        }

        state = DOOR_STATE.DOOR_OPENING;

    }

    void IsDoorOpening(Collider col)
    {
        if (gameObject != col.gameObject)
        {
            return;
        }

        playerCount++;

        if (state != DOOR_STATE.DOOR_CLOSING)
        {
            return;
        }

        state = DOOR_STATE.DOOR_OPENING;
    }


    void IsDoorClosing(Collider col)
    {
        if (gameObject != col.gameObject)
        {
            return;
        }

        playerCount--;

        if (state == DOOR_STATE.DOOR_CLOSING || state == DOOR_STATE.DOOR_CLOSED)
        {
            return;
        }

        if (playerCount == 0)
        {
            state = DOOR_STATE.DOOR_CLOSING;
        }
    }
}
