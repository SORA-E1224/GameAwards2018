using UnityEngine;

public class CharaControl : MonoBehaviour
{
    protected bool isDead;

    enum HEALTH_STATE
    {
        HEALTH = 0,
        INVINCIBLE,
        OUTBREAK
    }


    // Use this for initialization
    void Start()
    {
        isDead = false;
    }

    void Caught()
    {

    }
}
