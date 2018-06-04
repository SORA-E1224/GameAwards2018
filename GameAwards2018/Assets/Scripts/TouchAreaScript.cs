using System;
using UnityEngine;
using UnityEngine.Events;

public class TouchAreaScript : MonoBehaviour
{
    [Serializable]
    public class TriggerEvent : UnityEvent<GameObject> { }
    [SerializeField]
    TriggerEvent triggerEvent = new TriggerEvent();

    private void OnTriggerStay(Collider other)
    {
        if (transform.parent.gameObject == other.gameObject)
        {
            return;
        }

        CharaControl chara = transform.parent.GetComponent<CharaControl>();
        if (chara.healthState != CharaControl.HEALTH_STATE.OUTBREAK)
        {
            return;
        }

        if (other.tag == "Player")
        {
            triggerEvent.Invoke(other.gameObject);
        }
    }
}
