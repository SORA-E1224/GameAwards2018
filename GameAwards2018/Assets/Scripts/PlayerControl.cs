using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerControl : CharaControl
{
    private new CharacterController characterController;

    private bool IsActionTrigger = false;
    private bool IsCharge = false;
    private bool IsRecover = false;
    private bool UnableRun = false;
    private float count = 0f;

    private float CaughtStaminaBuff = 0f;

    [SerializeField]
    Slider staminaGage;
    Image gageColor;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        healthState = HEALTH_STATE.OUTBREAK;
        IsActionTrigger = false;
        IsCharge = false;
        IsRecover = false;
        count = 0f;
        characterController = GetComponent<CharacterController>();

        GameObject touchField = transform.Find("TouchField").gameObject;
        touchField.GetComponent<MeshRenderer>().enabled = IsVisibility;

        Material mat = renderer.material;
        mat.color = Color.green;

        RectTransform rectTrans = staminaGage.GetComponent<RectTransform>();
        rectTrans.sizeDelta = new Vector2(Screen.width * MaxStamina / 4f / 100f, 120f);
        CaughtStaminaBuff = 0f;
        gageColor = staminaGage.gameObject.transform.Find("Fill Area").GetComponentInChildren<Image>();
        gageColor.color = Colors.Aqua;

        SceneController.WriteDebugTextEvent += delegate (object sender, EventArgs e)
        {
            SceneController.WriteLineDebugText("***PlayerData***");
            SceneController.WriteLineDebugText(string.Format("Health : {0}", healthState));
            SceneController.WriteLineDebugText(string.Format("Stamina : {0}", stamina));
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

        if (healthState == HEALTH_STATE.DEAD)
        {
            count += Time.deltaTime;
            Material mat = renderer.material;
            mat.color = Color.Lerp(Color.green, new Color(0f, 1f, 0f, 0f), count / DeadTime);
            if (count > DeadTime)
            {
                count = 0f;
                Destroy(gameObject);
            }
            return;
        }

        if (IsRecover)
        {
            count += Time.deltaTime;
            Material mat = renderer.material;
            mat.color = Color.Lerp(Color.green, Color.yellow, count / RecoverTime);
            float gageWidth = Mathf.Lerp(MoveDesc.OutbreakStamina, MoveDesc.HealthStamina, count / RecoverTime);
            if (stamina > gageWidth)
            {
                stamina = gageWidth;
            }
            RectTransform rectTrans = staminaGage.GetComponent<RectTransform>();
            rectTrans.sizeDelta = new Vector2(Screen.width * gageWidth / 4f / 100f, 120f);
            if (count > RecoverTime)
            {
                count = 0f;
                IsRecover = false;
                mat.color = Color.yellow;
                rectTrans.sizeDelta = new Vector2(Screen.width * MaxStamina / 4f / 100f, 120f);
            }
            return;
        }

        if (IsCharge)
        {
            count += Time.deltaTime;
            Material mat = renderer.material;
            mat.color = Color.Lerp(Color.yellow, Color.green, count / ChargeTime);
            float gageWidth = Mathf.Lerp(MoveDesc.HealthStamina, MoveDesc.OutbreakStamina, count / ChargeTime);
            staminaGage.value = Mathf.Lerp(CaughtStaminaBuff, 1.0f, count / ChargeTime);
            RectTransform rectTrans = staminaGage.GetComponent<RectTransform>();
            rectTrans.sizeDelta = new Vector2(Screen.width * gageWidth / 4f / 100f, 120f);
            if (count > ChargeTime)
            {
                staminaGage.value = 1f;
                CaughtStaminaBuff = 0f;
                mat.color = Color.green;
                count = 0f;
                IsCharge = false;
                particle.Stop();
                rectTrans.sizeDelta = new Vector2(Screen.width * MaxStamina / 4f / 100f, 120f);
            }
            return;
        }

        Rotate();
        Move();
    }

    void MoveCalc()
    {
        float Speed = 0;
        bool IsRun = false;
        if (Input.GetAxis("RunTrigger") > 0.8f && !UnableRun && characterController.velocity.magnitude > Mathf.Epsilon)
        {
            stamina -= MoveDesc.UseStaminaPoints * Time.deltaTime;
            if (stamina < 0)
            {
                stamina = 0;
                UnableRun = true;
                gageColor.color = Colors.IndianRed;
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
                gageColor.color = Colors.Aqua;

            }
        }

        if (staminaGage)
        {
            staminaGage.value = stamina / MaxStamina;
        }

        if (IsRun)
        {
            Speed = MaxRunSpeed;
        }
        else
        {
            Speed = MaxWalkSpeed;
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
                Material mat = renderer.material;
                mat.color = Color.yellow;
                charaControl.Caught();
                healthState = HEALTH_STATE.IMMUNITY;
                IsActionTrigger = true;
                MaxStamina = MoveDesc.HealthStamina;
                MaxWalkSpeed = MoveDesc.HealthWalkSpeed;
                MaxRunSpeed = MoveDesc.HealthRunSpeed;
                IsRecover = true;
                count = 0f;
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
        count = 0f;
        particle.Play();
        CaughtStaminaBuff = stamina / MaxStamina;
        MaxStamina = MoveDesc.OutbreakStamina;
        MaxWalkSpeed = MoveDesc.OutbreakWalkSpeed;
        MaxRunSpeed = MoveDesc.OutbreakRunSpeed;
        stamina = MaxStamina;
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

}
