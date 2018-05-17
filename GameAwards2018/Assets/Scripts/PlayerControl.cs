using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [System.Serializable]
    class MOVE_DESC
    {
        public float MaxSpeed;
        public float Speed;
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
        MoveDesc.MaxSpeed = Mathf.Max(0.0f, MoveDesc.MaxSpeed);
        MoveDesc.Speed = Mathf.Max(0.0f, Mathf.Min(MoveDesc.Speed, MoveDesc.MaxSpeed));
        MoveDesc.FallRate = Mathf.Max(0.0f, MoveDesc.FallRate);
        MoveDesc.FallRate = Mathf.Min(1.0f, MoveDesc.FallRate);

        RotDesc.Speed = Mathf.Max(0.0f, RotDesc.Speed);
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
        if (Mathf.Abs(inputX) > 0.1f)
        {
            move += transform.right * inputX * Time.deltaTime * MoveDesc.Speed;
        }
        if (Mathf.Abs(inputY) > 0.1f)
        {
            move += transform.forward * inputY * Time.deltaTime * MoveDesc.Speed;
        }

        move *= MoveDesc.FallRate;

        if (move.magnitude > MoveDesc.MaxSpeed)
        {
            move = move.normalized * MoveDesc.MaxSpeed;
        }
        if (move.magnitude < 0.01f)
        {
            move = Vector3.zero;
        }
    }

    void Rotate()
    {
        float inputTrigger = Input.GetAxis("CameraTrigger");
        //Debug.Log(inputTrigger);
        if (Mathf.Abs(inputTrigger) < 0.1f)
        {
            return;
        }

        Vector3 newRot = transform.localEulerAngles;
        newRot.y += inputTrigger * Time.deltaTime * RotDesc.Speed;
        transform.localEulerAngles = newRot;

    }

    private void Move()
    {
        MoveCalc();
        transform.position += move;
    }
}
