using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleLight : MonoBehaviour {
    [SerializeField]
    private Light target;
    [SerializeField]
    private AnimationCurve rangeCurve;
    private float i;
	// Use this for initialization
	void Start () {
        i = 0;
	}
	
	// Update is called once per frame
	void Update () {
        i += Time.deltaTime;
        target.range = rangeCurve.Evaluate(i);
	}
}
