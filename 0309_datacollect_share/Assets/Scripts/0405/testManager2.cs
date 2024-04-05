using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

using Leap;
using Leap.Unity;
using Varjo.XR;

[System.Serializable]
public struct Obj
{
    public string name;
    public GameObject gameObj;
    public Vector3 orientation1;
    public Vector3 orientation2;
    public Vector3 orientation3;
    public Vector3 orientation4;
    //public int numTask;
}
public class testManager2 : MonoBehaviour
{
    [Header("Assign")]
    public TextMeshPro text;
    public GameObject cross;
    public GameObject fixPoint; //on the cross
    public LeapProvider leapProvider;

    public GameObject callHand;
    public GameObject callGaze;

    [Header("Object List")]
    public List<Obj> Objects;
    [Header("Trial Conditions")]
    public float radius = 0.6f;
    public int horizontalAngle1 = 60;
    public int horizontalAngle2 = 90;
    public int horizontalAngle3 = 120;
    public int verticalAngle1 = 60;
    public int verticalAngle2 = 90;
    public int verticalAngle3 = 120;

    [Header("Current Progress")]
    public string currPhase;
    private List<string> phase = new List<string> { "intro", "task" };

    public bool isGrasped = false;
    public bool isTask = false;
    public bool isReady = false;

    [HideInInspector] public UnityEvent Intro;
    [HideInInspector] public UnityEvent Task;
    [HideInInspector] public UnityEvent recordToggle;
    [HideInInspector] public UnityEvent stopRecording;

    //private List<VarjoEyeTracking.GazeData> gaze;
    //private List<VarjoEyeTracking.GazeData> gaze;
    private Hand _rightHand;

    void Start()
    {
        currPhase = phase[0];
        Intro.AddListener(delegate { StartCoroutine(countdownIntro(5)); });
        Intro.AddListener(delegate { cross.SetActive(true); });

        Task.AddListener(delegate { cross.SetActive(false); });

        recordToggle.AddListener(delegate { callGaze.SetActive(true); });
        recordToggle.AddListener(delegate { callHand.SetActive(true); });

        stopRecording.AddListener(delegate { callGaze.SetActive(false); });
        stopRecording.AddListener(delegate { callHand.SetActive(false); });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGrasped)
            {
                currPhase = "intro";
                isGrasped = false;
            }
        }
        if (currPhase == "intro")
        {
            this.Intro.Invoke();
            checkIfReady(); //Here, currPhase = "task" toggles. 
        }
        else if (currPhase == "task")
        {
            this.Task.Invoke();
        }
    }
    IEnumerator countdownIntro(int sec)
    {
        Debug.Log("Countdown starting");
        while (!isReady)
        {
            int time = sec;
            while (time > 0)
            {
                text.text = time.ToString();
                Debug.Log(time);
                yield return new WaitForSeconds(1);
                time--;
                if (isReady && time == 1)
                    this.recordToggle.Invoke();
            }
            if (isReady)
                currPhase = "task";
        }
    }
    void checkIfReady()
    {
        bool isGazeReady = false;
        bool isHandReady = false;
        var gaze = VarjoEyeTracking.GetGaze();
        if (gaze.status == VarjoEyeTracking.GazeStatus.Valid)
        {
            Vector3 gazeDirection = transform.TransformDirection(gaze.gaze.forward);
            Vector3 gazeOrigin = transform.TransformPoint(gaze.gaze.origin);
            Ray gazeRay = new Ray(gazeOrigin, gazeDirection);
            RaycastHit hit;
            if (Physics.Raycast(gazeRay, out hit))
            {
                float distance = Vector3.Distance(hit.point, fixPoint.transform.position);
                if (hit.transform.gameObject == cross && distance <= 0.5f)
                    isGazeReady = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isHandReady = true;
        }
        if (isGazeReady && isHandReady)
            isReady = true;
    }
    void makeTrial()
    {

    }
}

//if (Input.GetMouseButtonDown(0))
//{
//    if (currPhase == phase[0])
//    {
//        Debug.Log(Objects[0].name);
//        currPhase = phase[1];
//    }
//    else if (currPhase == phase[1])
//    {
//        Debug.Log("2");
//        currPhase = phase[2];
//    }
//    else if (currPhase == phase[2])
//    {
//        Debug.Log("3");
//        currPhase = phase[0];
//    }
//}
