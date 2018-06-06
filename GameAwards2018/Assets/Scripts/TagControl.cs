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
    [SerializeField, Range(1f, 1000f)]
    float DeadTime = 10f;

    bool IsCycle = true;
    float time;

    private void OnValidate()
    {
        time = CycleTime;
    }

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
        IsCycle = true;
        time = CycleTime;
    }

    // Update is called once per frame
    void Update()
    {
        time -= Time.deltaTime;
        if (IsCycle)
        {
            if (time < 0f)
            {
                time = DeadTime;
                IsCycle = false;
                for (int i = 0; i < playerList.Count;)
                {
                    CharaControl charaControl = playerList[i].GetComponent<CharaControl>();
                    if (charaControl.healthState == CharaControl.HEALTH_STATE.OUTBREAK)
                    {
                        charaControl.Dead();
                        playerList.RemoveAt(i);
                    }
                    i++;
                }
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
        }
        else
        {
            textMeshPro.SetText("BREAK");
            textMeshPro.color = Color.yellow;
            if (time < 0f)
            {
                time = CycleTime;
                IsCycle = true;

                System.Random rand = new System.Random();
                CharaControl nextOutbreakChara = playerList[rand.Next(playerList.Count)].GetComponent<CharaControl>();
                nextOutbreakChara.Caught();
            }
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
