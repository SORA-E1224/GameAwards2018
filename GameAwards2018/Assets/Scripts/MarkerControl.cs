using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerControl : MonoBehaviour
{

    [SerializeField]
    float rotateSpeed = 0.0f;

    [SerializeField]
    bool IsVisibility = false;

    // Use this for initialization
    void Start()
    {
        GetComponentInChildren<MeshRenderer>().enabled = IsVisibility;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation *= Quaternion.AngleAxis(rotateSpeed * Time.deltaTime, Vector3.up);
    }
}
