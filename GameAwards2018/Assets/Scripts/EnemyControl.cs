using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyControl : MonoBehaviour
{
    private NavMeshAgent agent;

    enum ENEMY_STATE
    {
        NONE = 0,
        PATROL,
        CHASE,
        RUN,
        MAX
    }

    ENEMY_STATE state;
    GameObject chaseObj;
    GameObject[] markers;
    int targetIndex;
    float count;

    [System.Serializable]
    class VISION_DESC
    {
        public float Visibility;
        public float Angle;
        public float DetectTime;
        public GameObject Field;
    }
    [SerializeField]
    VISION_DESC visionDesc;

    [System.Serializable]
    class HEARING_DESC
    {
        public float Radius;
        public GameObject Field;
    }
    [SerializeField]
    HEARING_DESC hearingDesc;

    [SerializeField]
    LayerMask mask;

    private void OnValidate()
    {
        if (visionDesc.Field)
        {
            visionDesc.Visibility = Mathf.Max(0f, visionDesc.Visibility);
            visionDesc.Angle = Mathf.Min(160f, Mathf.Max(0f, visionDesc.Angle));
            visionDesc.DetectTime = Mathf.Max(0f, visionDesc.DetectTime);
            Vector3 scale = Vector3.zero;
            scale.z = visionDesc.Visibility;
            scale.x = 2f * visionDesc.Visibility * Mathf.Tan(Mathf.Deg2Rad * visionDesc.Angle / 2f);
            scale.y = scale.x * 9f / 16f;
            visionDesc.Field.transform.localScale = scale;
        }

        if (hearingDesc.Field)
        {
            hearingDesc.Radius = Mathf.Max(0f, hearingDesc.Radius);
            hearingDesc.Field.transform.localScale = new Vector3(hearingDesc.Radius, 1.0f, hearingDesc.Radius);
        }
    }

    // Use this for initialization
    void Start()
    {
        count = 0f;
        state = ENEMY_STATE.PATROL;
        markers = GameObject.FindGameObjectsWithTag("Marker");
        agent = GetComponent<NavMeshAgent>();
        System.Random rand = new System.Random();
        targetIndex = rand.Next(markers.Length);
        agent.SetDestination(markers[targetIndex].transform.position);
        SceneController.WriteDebugTextEvent += delegate (object sender, EventArgs e)
        {
            SceneController.WriteLineDebugText("***EnemyData***");
            string buf;
            if (targetIndex < 0 || targetIndex >= markers.Length)
            {
                buf = "None";
            }
            else
            {
                buf = markers[targetIndex].name;
            }
            SceneController.WriteLineDebugText(string.Format("Target : {0}", buf));
            SceneController.WriteLineDebugText(string.Format("State : {0}", state));
        };
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case ENEMY_STATE.NONE:
                agent.isStopped = true;
                break;
            case ENEMY_STATE.PATROL:
                Vector3 dist = markers[targetIndex].transform.position - transform.position;
                if (dist.magnitude < 1.0f)
                {
                    System.Random rand = new System.Random();
                    targetIndex = rand.Next(markers.Length);
                    agent.SetDestination(markers[targetIndex].transform.position);
                }
                break;
            case ENEMY_STATE.CHASE:
                if (!chaseObj)
                {
                    break;
                }
                agent.SetDestination(chaseObj.transform.position);
                Vector3 runnerDist = chaseObj.transform.position - transform.position;
                if (runnerDist.magnitude > hearingDesc.Radius)
                {
                    count += Time.deltaTime;
                    if (count > visionDesc.DetectTime)
                    {
                        count = 0f;
                        state = ENEMY_STATE.PATROL;
                        agent.SetDestination(markers[targetIndex].transform.position);
                        chaseObj = null;
                    }
                }
                else
                {
                    count = 0f;
                }
                break;
            case ENEMY_STATE.RUN:
                if (targetIndex == -1)
                {
                    SearchEscapeMarker();
                }
                Vector3 markerDist = markers[targetIndex].transform.position - transform.position;
                if (markerDist.magnitude < 1.0f)
                {
                    SearchEscapeMarker();
                }

                Vector3 chaserDist = chaseObj.transform.position - transform.position;
                if (chaserDist.magnitude > hearingDesc.Radius)
                {
                    count += Time.deltaTime;
                    if (count > visionDesc.DetectTime)
                    {
                        count = 0f;
                        state = ENEMY_STATE.PATROL;
                        chaseObj = null;
                    }
                }
                else
                {
                    count = 0f;
                }

                break;
            default:
                state = ENEMY_STATE.NONE;
                break;
        }
    }

    public void DiscoverPlayer(GameObject player)
    {
        MeshCollider col = visionDesc.Field.GetComponent<MeshCollider>();
        Vector3 rayDir = player.GetComponent<CapsuleCollider>().center + player.transform.position - col.transform.position;
        rayDir.Normalize();
        Ray ray = new Ray(col.transform.position, rayDir);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, Mathf.Infinity, mask);
        Debug.DrawRay(col.transform.position, rayDir * hit.distance, Color.red);
        Debug.Log(hit.collider.tag);
        if (tag == "Outbreak")
        {
            if (hit.collider.tag == "Health")
            {
                chaseObj = hit.collider.gameObject;
                state = ENEMY_STATE.CHASE;
                count = 0f;
            }
        }
        else if (tag == "Health")
        {
            if (hit.collider.tag == "Outbreak")
            {
                chaseObj = hit.collider.gameObject;
                state = ENEMY_STATE.RUN;
                targetIndex = -1;
                count = 0f;
            }
        }
    }

    private void SearchEscapeMarker()
    {
        Vector3 OBDir = chaseObj.transform.position - transform.position;
        OBDir.Normalize();
        float evaluateMarker = 0f;
        for (var i = 0; i < markers.Length; i++)
        {
            Vector3 MarkerDir = markers[i].transform.position - transform.position;
            float markerDist = MarkerDir.magnitude;
            MarkerDir.Normalize();
            float angleOBtoMarker = Mathf.Acos(Vector3.Dot(OBDir, MarkerDir)) / Mathf.PI;
            if (evaluateMarker < markerDist * Mathf.Pow(angleOBtoMarker, 2.0f))
            {
                evaluateMarker = markerDist * Mathf.Pow(angleOBtoMarker, 2.0f);
                targetIndex = i;
            }
        }
        agent.SetDestination(markers[targetIndex].transform.position);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Door")
        {
            other.transform.parent.GetComponent<DoorControl>().DoorOpen();
        }
    }
}
