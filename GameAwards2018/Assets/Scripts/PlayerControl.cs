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

    private Vector3 move;

    private void OnValidate()
    {
        MoveDesc.RunSpeed = Mathf.Max(0.0f, MoveDesc.RunSpeed);
        MoveDesc.WalkSpeed = Mathf.Max(0.0f, Mathf.Min(MoveDesc.WalkSpeed, MoveDesc.RunSpeed));
        MoveDesc.FallRate = Mathf.Max(0.01f, MoveDesc.FallRate);
        MoveDesc.FallRate = Mathf.Min(1.0f, MoveDesc.FallRate);

        RotDesc.Speed = Mathf.Max(0.0f, RotDesc.Speed);
    }

    public event EventHandler<EventArgsPlayer> OnTriggerStayEvent = delegate { };
    public event EventHandler<EventArgsPlayer> OnTriggerExitEvent = delegate { };
    public event EventHandler<EventArgsPlayer> OnTriggerEnterEvent = delegate { };

    EventArgsPlayer args;

    private void Awake()
    {
        args = new EventArgsPlayer();
    }

    // Use this for initialization
    void Start()
    {
        move = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        Rotate();
        Move();
    }

    void MoveCalc()
    {
        float inputX = Input.GetAxis("LeftStickX");
        float inputY = Input.GetAxis("LeftStickY");
        float Speed = 0;
        if (Input.GetAxis("RunTrigger") < 0.8f)
        {
            Speed = MoveDesc.WalkSpeed;
        }
        else
        {
            Speed = MoveDesc.RunSpeed;
        }

        Vector3 accel = Vector3.zero;
        if (Mathf.Abs(inputX) > 0.3f)
        {
            accel += transform.right * inputX * Time.deltaTime * Speed;
        }
        if (Mathf.Abs(inputY) > 0.3f)
        {
            accel += transform.forward * inputY * Time.deltaTime * Speed;
        }

        move += accel;
        move *= MoveDesc.FallRate;

        if (move.magnitude < 0.01f)
        {
            move = Vector3.zero;
        }
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
            camera.transform.localRotation *= Quaternion.AngleAxis(inputTriggerY * Time.deltaTime * RotDesc.Speed, -Vector3.right);
        }
    }

    private void Move()
    {
        MoveCalc();
        transform.position += move;
    }

    private void OnTriggerStay(Collider other)
    {
        if (Input.GetAxis("ActionTrigger") > 0.9f)
        {
            args.col = other;
            OnTriggerStayEvent(this, args);
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
