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

    [SerializeField]
    protected MeshRenderer renderer;

    [SerializeField, Range(1f, 100f)]
    protected float ChargeTime = 1f;

    [SerializeField, Range(1f, 100f)]
    protected float DeadTime = 1f;

    [SerializeField, Range(0f, 5f)]
    protected float RecoverTime = 0f;

    [System.Serializable]
    public class MOVE_DESC
    {
        public float HealthWalkSpeed;
        public float HealthRunSpeed;

        public float OutbreakWalkSpeed;
        public float OutbreakRunSpeed;

        public float HealthStamina;
        public float OutbreakStamina;

        public float UseStaminaPoints;
        public float CureStaminaPoints;
    }

    [SerializeField]
    protected MOVE_DESC MoveDesc;
    protected float MaxWalkSpeed;
    protected float MaxRunSpeed;
    protected float MaxStamina;

    protected float stamina;

    [System.Serializable]
    public class ROTATE_DESC
    {
        public float Speed;
    }

    [SerializeField]
    protected ROTATE_DESC RotDesc;

    [SerializeField]
    protected ParticleSystem particle;

    [SerializeField]
    protected bool IsVisibility = false;

    private void OnValidate()
    {
        MoveDesc.HealthRunSpeed = Mathf.Max(0.0f, MoveDesc.HealthRunSpeed);
        MoveDesc.HealthWalkSpeed = Mathf.Max(0.0f, Mathf.Min(MoveDesc.HealthWalkSpeed, MoveDesc.HealthRunSpeed));
        MoveDesc.OutbreakRunSpeed = Mathf.Max(0.0f, MoveDesc.OutbreakRunSpeed);
        MoveDesc.OutbreakWalkSpeed = Mathf.Max(0.0f, Mathf.Min(MoveDesc.OutbreakWalkSpeed, MoveDesc.OutbreakRunSpeed));
        MoveDesc.HealthStamina = Mathf.Max(0f, MoveDesc.HealthStamina);
        MoveDesc.OutbreakStamina = Mathf.Max(0f, MoveDesc.OutbreakStamina);
        MoveDesc.UseStaminaPoints = Mathf.Max(0f, MoveDesc.UseStaminaPoints);
        MoveDesc.CureStaminaPoints = Mathf.Max(0f, MoveDesc.CureStaminaPoints);

        RotDesc.Speed = Mathf.Max(0.0f, RotDesc.Speed);

        RecoverTime = Mathf.Max(Mathf.Epsilon, RecoverTime);
    }

    // Use this for initialization
    protected virtual void Start()
    {
        healthCount = 0f;
        healthState = StartHealthState;
        switch (healthState)
        {
            case HEALTH_STATE.HEALTH:
                MaxWalkSpeed = MoveDesc.HealthWalkSpeed;
                MaxRunSpeed = MoveDesc.HealthRunSpeed;
                MaxStamina = MoveDesc.HealthStamina;
                break;
            case HEALTH_STATE.OUTBREAK:
                MaxWalkSpeed = MoveDesc.OutbreakWalkSpeed;
                MaxRunSpeed = MoveDesc.OutbreakRunSpeed;
                MaxStamina = MoveDesc.OutbreakStamina;
                break;
            default:
                MaxWalkSpeed = MoveDesc.HealthWalkSpeed;
                MaxRunSpeed = MoveDesc.HealthRunSpeed;
                MaxStamina = MoveDesc.HealthStamina;
                break;
        }
        stamina = MaxStamina;
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

    public abstract void Dead();

}
