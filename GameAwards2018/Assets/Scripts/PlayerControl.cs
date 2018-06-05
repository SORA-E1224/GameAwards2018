using UnityEngine;
using System;

public class PlayerControl : CharaControl
{
    [System.Serializable]
    class MOVE_DESC
    {
        public float WalkSpeed;
        public float RunSpeed;
        public float FallRate;
    }

    [SerializeField]
    private MOVE_DESC MoveDesc;

    [System.Serializable]
    class ROTATE_DESC
    {
        public float Speed;
    }

    [SerializeField]
    private ROTATE_DESC RotDesc;

    private new CharacterController characterController;

    private void OnValidate()
    {
        MoveDesc.RunSpeed = Mathf.Max(0.0f, MoveDesc.RunSpeed);
        MoveDesc.WalkSpeed = Mathf.Max(0.0f, Mathf.Min(MoveDesc.WalkSpeed, MoveDesc.RunSpeed));
        MoveDesc.FallRate = Mathf.Max(0.0f, MoveDesc.FallRate);
        MoveDesc.FallRate = Mathf.Min(1.0f, MoveDesc.FallRate);

        RotDesc.Speed = Mathf.Max(0.0f, RotDesc.Speed);
    }

    private bool IsActionTrigger = false;
    private bool IsCharge = false;
    private float chargeCount = 0f;
    [SerializeField]
    ParticleSystem particle;
    [SerializeField, Range(0f, 100f)]
    float ChargeTime = 0f;

    [SerializeField]
    bool IsVisibility = false;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        healthState = HEALTH_STATE.OUTBREAK;
        IsActionTrigger = false;
        IsCharge = false;
        characterController = GetComponent<CharacterController>();

        GameObject touchField = transform.Find("TouchField").gameObject;
        touchField.GetComponent<MeshRenderer>().enabled = IsVisibility;

        SceneController.WriteDebugTextEvent += delegate (object sender, EventArgs e)
        {
            SceneController.WriteLineDebugText("***PlayerData***");
            SceneController.WriteLineDebugText(string.Format("Health : {0}", healthState));
        };
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (Input.GetAxis("ActionTrigger") < 0.9f)
        {
            IsActionTrigger = false;
        }
        else
        {
            IsActionTrigger = true;
        }

        if (IsCharge)
        {
            chargeCount += Time.deltaTime;
            if (chargeCount > ChargeTime)
            {
                chargeCount = 0f;
                IsCharge = false;
                particle.Stop();
            }
            return;
        }

        Rotate();
        Move();
    }

    void MoveCalc()
    {
        float Speed = 0;
        if (Input.GetAxis("RunTrigger") < 0.8f)
        {
            Speed = MoveDesc.WalkSpeed;
        }
        else
        {
            Speed = MoveDesc.RunSpeed;
        }

        float inputX = Input.GetAxis("LeftStickX");
        float inputY = Input.GetAxis("LeftStickY");

        Vector3 accel = Vector3.zero;
        if (Mathf.Abs(inputX) > 0.3f)
        {
            accel += transform.right * inputX;
        }
        if (Mathf.Abs(inputY) > 0.3f)
        {
            accel += transform.forward * inputY;
        }
        accel *= Speed;
        accel += Physics.gravity;
        accel *= Time.deltaTime;

        characterController.Move(accel);
    }

    void Rotate()
    {
        float inputTriggerX = Input.GetAxis("CameraTriggerX");
        float inputTriggerY = Input.GetAxis("CameraTriggerY");

        if (Mathf.Abs(inputTriggerX) > 0.3f)
        {
            transform.localRotation *= Quaternion.AngleAxis(inputTriggerX * Time.deltaTime * RotDesc.Speed, transform.up);
        }
        if (Mathf.Abs(inputTriggerY) > 0.3f)
        {
            Camera camera = GetComponentInChildren<Camera>();
            Vector3 editCameraRotation = camera.transform.localRotation.eulerAngles;
            editCameraRotation.x -= inputTriggerY * Time.deltaTime * RotDesc.Speed;
            if (editCameraRotation.x > 180.0f)
            {
                editCameraRotation.x -= 360f;
            }
            if (editCameraRotation.x > 60.0f)
            {
                editCameraRotation.x = 60.0f;
            }
            if (editCameraRotation.x < -60.0f)
            {
                editCameraRotation.x = -60.0f;
            }
            camera.transform.localRotation = Quaternion.Euler(editCameraRotation);
        }
    }

    private void Move()
    {
        MoveCalc();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Door")
        {
            if (Input.GetAxis("ActionTrigger") > 0.9f)
            {
                if (!IsActionTrigger)
                {
                    other.transform.parent.GetComponent<DoorControl>().DoorAction();
                    IsActionTrigger = true;
                }
            }
        }
    }

    public void Catch(GameObject player)
    {
        CharaControl charaControl = player.GetComponent<CharaControl>();
        if (charaControl.healthState != HEALTH_STATE.HEALTH)
        {
            return;
        }

        if (Input.GetAxis("ActionTrigger") > 0.9f)
        {
            if (!IsActionTrigger)
            {
                charaControl.Caught();
                healthState = HEALTH_STATE.IMMUNITY;
                IsActionTrigger = true;
            }
        }
    }

    public override void Caught()
    {
        if (healthState != HEALTH_STATE.HEALTH)
        {
            return;
        }

        healthState = HEALTH_STATE.OUTBREAK;
        IsCharge = true;
        chargeCount = 0f;
        particle.Play();
    }

    public override void Dead()
    {
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

    public override void Destroy()
    {
        Destroy(gameObject);
    }
}
