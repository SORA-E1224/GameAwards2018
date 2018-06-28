using UnityEngine;

public abstract class CharaControl : MonoBehaviour
{
    public enum HEALTH_STATE
    {
        HEALTH = 0,
        IMMUNITY,
        OUTBREAK,
        DEAD,
        NONE
    }
    [HideInInspector]
    public HEALTH_STATE healthState;

    [SerializeField]
    public HEALTH_STATE StartHealthState = HEALTH_STATE.HEALTH;

    private float healthCount = 0f;

    [SerializeField, Range(0f, 100f)]
    private float ImmunityTime = 0f;

    [SerializeField]
    protected new Renderer renderer;

    [SerializeField, Range(1f, 100f)]
    protected float ChargeTime = 1f;

    [SerializeField, Range(1f, 100f)]
    protected float DeadTime = 1f;

    [SerializeField, Range(0f, 5f)]
    protected float RecoverTime = 0f;

    protected Color OutbreakBodyColor = Colors.ForestGreen;

    [SerializeField]
    protected Animator animator;
    protected int animHash_Dead = 0;

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

    public TagControl tagControl;

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
        Material mat = renderer.material;
        switch (healthState)
        {
            case HEALTH_STATE.HEALTH:
                MaxWalkSpeed = MoveDesc.HealthWalkSpeed;
                MaxRunSpeed = MoveDesc.HealthRunSpeed;
                MaxStamina = MoveDesc.HealthStamina;
                mat.color = Colors.White;
                break;
            case HEALTH_STATE.OUTBREAK:
                MaxWalkSpeed = MoveDesc.OutbreakWalkSpeed;
                MaxRunSpeed = MoveDesc.OutbreakRunSpeed;
                MaxStamina = MoveDesc.OutbreakStamina;
                mat.color = OutbreakBodyColor;
                break;
            default:
                MaxWalkSpeed = MoveDesc.HealthWalkSpeed;
                MaxRunSpeed = MoveDesc.HealthRunSpeed;
                MaxStamina = MoveDesc.HealthStamina;
                mat.color = Colors.White;
                break;
        }
        stamina = MaxStamina;
        animHash_Dead = Animator.StringToHash("Base Layer.Dead");
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

    public virtual void Dead()
    {
        animator.SetTrigger("IsDead");
        healthState = HEALTH_STATE.DEAD;
        var colorOverLifetime = particle.colorOverLifetime;
        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color32(0, 128, 255, 255), 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 0.72f), new GradientAlphaKey(0.0f, 1.0f) }
            );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
        particle.Play();
    }

}
