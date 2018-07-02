using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class TagControl : MonoBehaviour
{
    public enum TagState
    {
        READY = 0,
        GAME,
        FINISH
    }

    public TagState tagState { get; private set; }

    List<GameObject> playerList = new List<GameObject>();
    private int targetCamera = 0;
    private bool trigger;
    private bool isWinner;

    [SerializeField]
    TextMeshProUGUI timerText;
    [SerializeField]
    TextMeshProUGUI noticeText;
    [SerializeField, Range(1f, 1000f)]
    float ReadyTime = 3f;
    [SerializeField, Range(1f, 1000f)]
    float CycleTime = 30f;
    [SerializeField, Range(1f, 1000f)]
    float DeadTime = 10f;

    float noticeCount = 0f;
    bool IsCycle = true;
    float time;

    private void OnValidate()
    {
        time = CycleTime;
    }

    // Use this for initialization
    void Start()
    {
        tagState = TagState.READY;
        noticeCount = 0f;
        noticeText.text = "READY";
        isWinner = false;
        RectTransform rectTrans = noticeText.GetComponent<RectTransform>();
        Vector3 firstPos = new Vector3((Screen.width + rectTrans.sizeDelta.x) / 2f, 0f, 0f);
        rectTrans.localPosition = firstPos;
        GameObject[] respawnList = GameObject.FindGameObjectsWithTag("Respawn");
        GameObject player = Instantiate(Resources.Load("Prefabs/Player")) as GameObject;
        player.GetComponent<CharaControl>().tagControl = this;
        System.Random rand = new System.Random();
        int playerRespawnNum = rand.Next(respawnList.Length);
        player.transform.position = respawnList[playerRespawnNum].transform.position;
        player.GetComponent<CharaControl>().healthState = CharaControl.HEALTH_STATE.OUTBREAK;
        player.transform.localEulerAngles = new Vector3(0f, Mathf.Atan2(player.transform.position.x, player.transform.position.z) * Mathf.Rad2Deg + 180f, 0f);
        playerList.Add(player);
        targetCamera = 0;
        for (int i = 0; i < respawnList.Length; i++)
        {
            if (i == playerRespawnNum)
            {
                continue;
            }
            GameObject enemy = null;
            enemy = Instantiate(Resources.Load("Prefabs/Enemy")) as GameObject;
            enemy.GetComponent<CharaControl>().tagControl = this;
            enemy.GetComponent<NavMeshAgent>().Warp(respawnList[i].transform.position);
            enemy.GetComponent<NavMeshAgent>().enabled = true;
            enemy.transform.localEulerAngles = new Vector3(0f, Mathf.Atan2(enemy.transform.position.x, enemy.transform.position.z) * Mathf.Rad2Deg + 180f, 0f);
            enemy.GetComponent<CharaControl>().healthState = CharaControl.HEALTH_STATE.HEALTH;
            playerList.Add(enemy);
        }
        trigger = false;
        IsCycle = true;
        time = CycleTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneController.IsFade())
        {
            return;
        }

        TagProcessing();
    }

    void TagProcessing()
    {
        switch (tagState)
        {
            case TagState.READY:
                Ready();
                break;
            case TagState.GAME:
                Game();
                break;
            case TagState.FINISH:
                Finish();
                break;
            default:
                break;
        }

    }

    void Ready()
    {
        noticeCount += Time.deltaTime;
        RectTransform rectTrans = noticeText.GetComponent<RectTransform>();
        Vector3 firstPos = new Vector3((Screen.width + rectTrans.sizeDelta.x) / 2f, 0f, 0f);
        Vector3 finishPos = new Vector3(-(Screen.width + rectTrans.sizeDelta.x) / 2f, 0f, 0f);
        rectTrans.localPosition = Vector3.Lerp(firstPos, finishPos, noticeCount / ReadyTime);
        if (noticeCount > ReadyTime)
        {
            noticeCount = 0f;
            if (noticeText.text == "READY")
            {
                noticeText.text = "GO";
            }
            else
            {
                tagState = TagState.GAME;
            }
        }

    }

    void Game()
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

                        if (i == 0)
                        {
                            isWinner = false;
                            tagState = TagState.FINISH;
                            noticeText.text = "LOSE";
                            noticeText.color = Colors.DarkBlue;
                            break;
                        }
                        if (playerList.Count == 1)
                        {
                            isWinner = true;
                            tagState = TagState.FINISH;
                            noticeText.text = "Win";
                            noticeText.color = Colors.Gold;
                            break;
                        }
                    }
                    i++;
                }
            }
            string timeStr;
            int timeDecimal = Mathf.FloorToInt((time - Mathf.Floor(time)) * 100f);
            timeStr = string.Format("{0:D2}:{1:D2}", Mathf.FloorToInt(time), timeDecimal);
            timerText.SetText(timeStr);
            if (time < 10f)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
        else
        {
            timerText.SetText("BREAK");
            timerText.color = Color.yellow;
            if (time < 0f)
            {
                time = CycleTime;
                IsCycle = true;

                System.Random rand = new System.Random();
                CharaControl nextOutbreakChara = playerList[rand.Next(playerList.Count)].GetComponent<CharaControl>();
                nextOutbreakChara.Caught();
            }
        }


        if (!Debug.isDebugBuild)
        {
            return;
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

    void Finish()
    {
        noticeCount += Time.deltaTime;
        RectTransform rectTrans = noticeText.GetComponent<RectTransform>();
        Vector3 firstPos = new Vector3((Screen.width + rectTrans.sizeDelta.x) / 2f, 0f, 0f);
        Vector3 finishPos = new Vector3(-(Screen.width + rectTrans.sizeDelta.x) / 2f, 0f, 0f);
        rectTrans.localPosition = Vector3.Lerp(firstPos, finishPos, noticeCount / DeadTime);
        if (noticeCount > DeadTime)
        {
            SceneController.Fade(SceneController.FADE_MODE.FADEMODE_OUT, "Title");
        }
    }
}
