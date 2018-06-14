using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyControl : CharaControl
{
    private NavMeshAgent agent;
    private bool UnableRun = false;

    enum ENEMY_STATE
    {
        DEAD = 0,
        PATROL,
        CHASE,
        RUN,
        CHARGE,
        RECOVER
    }

    ENEMY_STATE state;
    GameObject chaseObj;
    GameObject[] markers;
    int targetIndex;
    float count;

    [System.Serializable]
    class VISION_DESC
    {
        public float Visibility;
        public float Angle;
        public float DetectTime;
        public GameObject Field;
    }
    [SerializeField]
    VISION_DESC visionDesc;

    [System.Serializable]
    class HEARING_DESC
    {
        public float Radius;
        public GameObject Field;
    }
    [SerializeField]
    HEARING_DESC hearingDesc;

    [SerializeField]
    LayerMask mask;


    private void OnValidate()
    {
        if (visionDesc.Field)
        {
            visionDesc.Visibility = Mathf.Max(0f, visionDesc.Visibility);
            visionDesc.Angle = Mathf.Min(160f, Mathf.Max(0f, visionDesc.Angle));
            visionDesc.DetectTime = Mathf.Max(0f, visionDesc.DetectTime);
            Vector3 scale = Vector3.zero;
            scale.z = visionDesc.Visibility;
            scale.x = 2f * visionDesc.Visibility * Mathf.Tan(Mathf.Deg2Rad * visionDesc.Angle / 2f);
            scale.y = scale.x * 9f / 16f;
            visionDesc.Field.transform.localScale = scale;
        }

        if (hearingDesc.Field)
        {
            hearingDesc.Radius = Mathf.Max(0f, hearingDesc.Radius);
            hearingDesc.Field.transform.localScale = new Vector3(hearingDesc.Radius, 1.0f, hearingDesc.Radius);
        }
    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        count = 0f;
        state = ENEMY_STATE.PATROL;
        markers = GameObject.FindGameObjectsWithTag("Marker");
        agent = GetComponent<NavMeshAgent>();
        System.Random rand = new System.Random();
        targetIndex = rand.Next(markers.Length);
        agent.SetDestination(markers[targetIndex].transform.position);
        agent.speed = MaxWalkSpeed;
        agent.angularSpeed = RotDesc.Speed;
        
        visionDesc.Field.GetComponent<MeshRenderer>().enabled = IsVisibility;
        hearingDesc.Field.GetComponent<MeshRenderer>().enabled = IsVisibility;

        Material mat = renderer.material;
        mat.color = Color.yellow;
        UnableRun = false;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        AIProcessing();
    }

    // Discovering Player Func
    public void DiscoverPlayer(GameObject player)
    {
        MeshCollider col = visionDesc.Field.GetComponent<MeshCollider>();
        Vector3 rayDir = player.GetComponent<CapsuleCollider>().center + player.transform.position - col.transform.position;
        rayDir.Normalize();
        Ray ray = new Ray(col.transform.position, rayDir);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, Mathf.Infinity, mask);
        Debug.DrawRay(col.transform.position, rayDir * hit.distance, Color.red);
        CharaControl charaScript = hit.collider.GetComponent<CharaControl>();
        if (!charaScript)
        {
            return;
        }

        if (healthState == HEALTH_STATE.OUTBREAK && state != ENEMY_STATE.CHARGE)
        {
            if (charaScript.healthState == HEALTH_STATE.HEALTH)
            {
                chaseObj = hit.collider.gameObject;
                state = ENEMY_STATE.CHASE;
                count = 0f;
            }
        }
        else if (healthState == HEALTH_STATE.HEALTH || healthState == HEALTH_STATE.IMMUNITY)
        {
            if (charaScript.healthState == HEALTH_STATE.OUTBREAK)
            {
                chaseObj = hit.collider.gameObject;
                state = ENEMY_STATE.RUN;
                count = 0f;
                SearchEscapeMarker();
            }
        }
    }

    // Search the farthest Marker from Chaser Func
    private void SearchEscapeMarker()
    {
        Vector3 OBDir = chaseObj.transform.position - transform.position;
        OBDir.Normalize();
        float evaluateMarker = 0f;
        for (var i = 0; i < markers.Length; i++)
        {
            Vector3 MarkerDir = markers[i].transform.position - transform.position;
            float markerDist = MarkerDir.magnitude;
            MarkerDir.Normalize();
            float angleOBtoMarker = Mathf.Acos(Vector3.Dot(OBDir, MarkerDir)) / Mathf.PI;
            if (evaluateMarker < markerDist * Mathf.Pow(angleOBtoMarker, 2.0f))
            {
                evaluateMarker = markerDist * Mathf.Pow(angleOBtoMarker, 2.0f);
                targetIndex = i;
            }
        }
        agent.SetDestination(markers[targetIndex].transform.position);
    }

    // OnTrigger Func
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Door")
        {
            other.transform.parent.GetComponent<DoorControl>().DoorOpen();
        }
    }

    // Caught by Outbreak Func
    public override void Caught()
    {
        if (healthState != HEALTH_STATE.HEALTH)
        {
            return;
        }

        healthState = HEALTH_STATE.OUTBREAK;
        state = ENEMY_STATE.CHARGE;
        count = 0f;
        particle.Play();
        MaxStamina = MoveDesc.OutbreakStamina;
        MaxWalkSpeed = MoveDesc.OutbreakWalkSpeed;
        MaxRunSpeed = MoveDesc.OutbreakRunSpeed;
        stamina = MaxStamina;
        agent.speed = MaxWalkSpeed;
    }

    // Dead Func
    public override void Dead()
    {
        healthState = HEALTH_STATE.DEAD;
        state = ENEMY_STATE.DEAD;
        var colorOverLifetime = particle.colorOverLifetime;
        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color32(0, 128, 255, 255), 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 0.72f), new GradientAlphaKey(0.0f, 1.0f) }
            );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
        particle.Play();
    }

    // CPU AI Processing Func
    void AIProcessing()
    {
        switch (state)
        {
            case ENEMY_STATE.DEAD:
                if (Dying())
                {
                    return;
                }
                break;
            case ENEMY_STATE.PATROL:
                Patrol();
                break;
            case ENEMY_STATE.CHASE:
                Chase();
                break;
            case ENEMY_STATE.RUN:
                Run();
                break;
            case ENEMY_STATE.CHARGE:
                Charge();
                break;
            case ENEMY_STATE.RECOVER:
                Recover();
                break;
            default:
                state = ENEMY_STATE.DEAD;
                break;
        }
    }

    // Dying Func
    bool Dying()
    {
        Material mat = renderer.material;
        agent.isStopped = true;
        count += Time.deltaTime;
        mat.color = Color.Lerp(Color.green, new Color(0f, 1f, 0f, 0f), count / DeadTime);
        if (count > DeadTime)
        {
            count = 0f;
            Destroy(gameObject);
            return true;
        }
        return false;
    }

    // Patrol Func
    void Patrol()
    {
        stamina += MoveDesc.CureStaminaPoints * Time.deltaTime;
        if (stamina > MaxStamina)
        {
            stamina = MaxStamina;
            UnableRun = false;
        }
        if (agent.remainingDistance < 1.0f)
        {
            System.Random rand = new System.Random();
            targetIndex = rand.Next(markers.Length);
            agent.SetDestination(markers[targetIndex].transform.position);
        }
    }

    // Chase Func
    void Chase()
    {
        Material mat = renderer.material;
        if (!chaseObj)
        {
            state = ENEMY_STATE.PATROL;
            return;
        }
        bool IsRun = false;
        if (!UnableRun)
        {
            stamina -= MoveDesc.UseStaminaPoints * Time.deltaTime;
            if (stamina < 0)
            {
                stamina = 0;
                UnableRun = true;
            }
            else
            {
                IsRun = true;
            }
        }
        else
        {
            stamina += MoveDesc.CureStaminaPoints * Time.deltaTime;
            if (stamina > MaxStamina)
            {
                stamina = MaxStamina;
                UnableRun = false;
            }
        }

        if (IsRun)
        {
            agent.speed = MaxRunSpeed;
        }
        else
        {
            agent.speed = MaxWalkSpeed;
        }

        agent.SetDestination(chaseObj.transform.position);
        // Catch Health Player
        if (agent.remainingDistance < 2.0f)
        {
            CharaControl charaControl = chaseObj.GetComponent<CharaControl>();
            if (charaControl.healthState == HEALTH_STATE.HEALTH)
            {
                charaControl.Caught();
                state = ENEMY_STATE.RECOVER;
                healthState = HEALTH_STATE.IMMUNITY;
                mat.color = Color.yellow;
                MaxStamina = MoveDesc.HealthStamina;
                MaxWalkSpeed = MoveDesc.HealthWalkSpeed;
                MaxRunSpeed = MoveDesc.HealthRunSpeed;
                agent.speed = MaxWalkSpeed;
            }
        }
        // Missing Player
        if (agent.remainingDistance > hearingDesc.Radius)
        {
            count += Time.deltaTime;
            if (count > visionDesc.DetectTime)
            {
                count = 0f;
                state = ENEMY_STATE.PATROL;
                agent.SetDestination(markers[targetIndex].transform.position);
                chaseObj = null;
                agent.speed = MaxWalkSpeed;
            }
        }
        else
        {
            count = 0f;
        }
    }

    // Run Func
    void Run()
    {
        if (!chaseObj)
        {
            state = ENEMY_STATE.PATROL;
            return;
        }

        bool IsRun = false;
        if (!UnableRun)
        {
            stamina -= MoveDesc.UseStaminaPoints * Time.deltaTime;
            if (stamina < 0)
            {
                stamina = 0;
                UnableRun = true;
            }
            else
            {
                IsRun = true;
            }
        }
        else
        {
            stamina += MoveDesc.CureStaminaPoints * Time.deltaTime;
            if (stamina > MaxStamina)
            {
                stamina = MaxStamina;
                UnableRun = false;
            }
        }

        if (IsRun)
        {
            agent.speed = MaxRunSpeed;
        }
        else
        {
            agent.speed = MaxWalkSpeed;
        }

        if (agent.remainingDistance < 1.0f)
        {
            SearchEscapeMarker();
        }

        // Keep Safe Distance
        Vector3 chaserDist = chaseObj.transform.position - transform.position;
        if (chaserDist.magnitude > hearingDesc.Radius)
        {
            count += Time.deltaTime;
            if (count > visionDesc.DetectTime)
            {
                count = 0f;
                state = ENEMY_STATE.PATROL;
                chaseObj = null;
                agent.speed = MaxWalkSpeed;
            }
        }
        else
        {
            count = 0f;
        }
    }

    // Charge Func
    void Charge()
    {
        Material mat = renderer.material;
        agent.isStopped = true;
        count += Time.deltaTime;
        mat.color = Color.Lerp(Color.yellow, Color.green, count / ChargeTime);
        if (count > ChargeTime)
        {
            count = 0f;
            mat.color = Color.green;
            state = ENEMY_STATE.PATROL;
            agent.isStopped = false;
            particle.Stop();
        }
    }

    // Recover Func
    void Recover()
    {
        Material mat = renderer.material;
        agent.isStopped = true;
        count += Time.deltaTime;
        mat.color = Color.Lerp(Color.green, Color.yellow, count / RecoverTime);
        if (count > RecoverTime)
        {
            count = 0f;
            mat.color = Color.yellow;
            state = ENEMY_STATE.PATROL;
            agent.isStopped = false;
        }
    }
}
