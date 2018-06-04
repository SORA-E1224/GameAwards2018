using System;
using UnityEngine;
using UnityEngine.Events;

public class EnemyVisionScript : MonoBehaviour
{
    [Serializable]
    public class TriggerEvent : UnityEvent<GameObject> { }
    [SerializeField]
    TriggerEvent triggerEvent = new TriggerEvent();

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            triggerEvent.Invoke(other.gameObject);
        }
    }
}
