using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TagControl : MonoBehaviour
{
    List<GameObject> playerList = new List<GameObject>();
    private int targetCamera = 0;
    private bool trigger;

    [SerializeField]
    TextMeshProUGUI textMeshPro;
    [SerializeField, Range(1f, 1000f)]
    float CycleTime = 30f;

    float time = 0f;

    // Use this for initialization
    void Start()
    {
        GameObject[] searchList = GameObject.FindGameObjectsWithTag("Player");
        playerList.AddRange(searchList);
        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].GetComponentInChildren<Camera>().depth == 0)
            {
                targetCamera = i;
                return;
            }
        }
        trigger = false;
        time = CycleTime;
    }

    // Update is called once per frame
    void Update()
    {
        time -= Time.deltaTime;
        if (time < 0f)
        {
            time = CycleTime;
        }
        string timeStr;
        int timeDecimal = Mathf.FloorToInt((time - Mathf.Floor(time)) * 100f);
        timeStr = string.Format("{0:D2}:{1:D2}", Mathf.FloorToInt(time), timeDecimal);
        textMeshPro.SetText(timeStr);
        if (time < 10f)
        {
            textMeshPro.color = Color.red;
        }
        else
        {
            textMeshPro.color = Color.white;
        }

        if (Input.GetAxis("ChangeCamera") > 0.9f && !trigger)
        {
            playerList[targetCamera].GetComponentInChildren<Camera>().depth = -1;
            targetCamera++;
            if (targetCamera >= playerList.Count)
            {
                targetCamera = 0;
            }
            playerList[targetCamera].GetComponentInChildren<Camera>().depth = 0;
            trigger = true;
        }

        if (Input.GetAxis("ChangeCamera") < -0.9f && !trigger)
        {
            playerList[targetCamera].GetComponentInChildren<Camera>().depth = -1;
            targetCamera--;
            if (targetCamera < 0)
            {
                targetCamera = playerList.Count - 1;
            }
            playerList[targetCamera].GetComponentInChildren<Camera>().depth = 0;
            trigger = true;
        }

        if (Mathf.Abs(Input.GetAxis("ChangeCamera")) < 0.9f)
        {
            trigger = false;
        }
    }
}
