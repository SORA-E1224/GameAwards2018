﻿using UnityEngine;
using System;

public class EventArgsPlayer : EventArgs
{
    public Collider col;
}

public class PlayerControl : MonoBehaviour
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

    public event EventHandler<EventArgsPlayer> OnTriggerActionEvent = delegate { };
    public event EventHandler<EventArgsPlayer> OnTriggerActionTriggerEvent = delegate { };
    public event EventHandler<EventArgsPlayer> OnTriggerExitEvent = delegate { };
    public event EventHandler<EventArgsPlayer> OnTriggerEnterEvent = delegate { };
    private bool IsActionTrigger = false;

    EventArgsPlayer args;

    private void Awake()
    {
        args = new EventArgsPlayer();
    }

    // Use this for initialization
    void Start()
    {
        IsActionTrigger = false;
        characterController = GetComponent<CharacterController>();
        SceneController.WriteDebugTextEvent += delegate (object sender, EventArgs e)
          {
              SceneController.WriteLineDebugText("***PlayerData***");
              SceneController.WriteLineDebugText(string.Format("Velocity:({0:F2}, {1:F2}, {2:F2})", characterController.velocity.x, characterController.velocity.y, characterController.velocity.z));
              SceneController.WriteLineDebugText(string.Format("Speed:{0:F2}", characterController.velocity.magnitude));
          };
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("ActionTrigger") < 0.9f)
        {
            IsActionTrigger = false;
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
        if (Input.GetAxis("ActionTrigger") > 0.9f)
        {
            args.col = other;
            OnTriggerActionEvent(this, args);
            if(!IsActionTrigger)
            {
                OnTriggerActionTriggerEvent(this, args);
                IsActionTrigger = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        args.col = other;
        OnTriggerEnterEvent(this, args);
    }

    private void OnTriggerExit(Collider other)
    {
        args.col = other;
        OnTriggerExitEvent(this, args);
    }
}
