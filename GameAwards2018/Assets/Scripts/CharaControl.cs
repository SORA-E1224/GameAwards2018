using UnityEngine;

public abstract class CharaControl : MonoBehaviour
{
    public enum HEALTH_STATE
    {
        HEALTH = 0,
        IMMUNITY,
        OUTBREAK,
        DEAD
    }
    [HideInInspector]
    public HEALTH_STATE healthState;

    [SerializeField]
    HEALTH_STATE StartHealthState = HEALTH_STATE.HEALTH;

    private float healthCount = 0f;

    [SerializeField, Range(0f, 100f)]
    private float ImmunityTime = 0f;

    // Use this for initialization
    protected virtual void Start()
    {
        healthCount = 0f;
        healthState = StartHealthState;
    }

    protected virtual void Update()
    {
        if (healthState == HEALTH_STATE.IMMUNITY)
        {
            healthCount += Time.deltaTime;
            if (healthCount > ImmunityTime)
            {
                healthCount = 0f;
                healthState = HEALTH_STATE.HEALTH;
            }
        }


    }

    public abstract void Caught();
}
