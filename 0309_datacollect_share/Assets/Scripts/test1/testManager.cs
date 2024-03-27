using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public struct message2Python
{
    public bool isStart;
    public bool isDone;
    public int areaNum;
    public int objAng;
    public int hAng;
    public int vAng;
    public override string ToString()
    {
        int isStartInt = isStart ? 1 : 0;
        int isDoneInt = isDone ? 1 : 0;
        return $"{isStartInt} {isDoneInt} {areaNum} {objAng} {hAng} {vAng}";
    }
}
public class testManager : MonoBehaviour
{
    [Header("Assign")]
    public GameObject camera;
    public GameObject target;
    public TextMeshPro text;
    public PythonTest pT;

    [Header("Task Spaces")]
    public float radius = 0.6f;
    public int horizontalAngle1 = 60;
    public int horizontalAngle2 = 90;
    public int horizontalAngle3 = 120;
    public int verticalAngle1 = 60;
    public int verticalAngle2 = 90;
    public int verticalAngle3 = 120;
    public int objAngle1 = 0;
    public int objAngle2 = 120;
    public int objAngle3 = 240;
    [HideInInspector] public List<int> horizontalAngles;
    [HideInInspector] public List<int> verticalAngles;
    [HideInInspector] public List<int> objAngles;
    [HideInInspector] public List<(int, int)> hvAngles = new List<(int,int)>();
    [HideInInspector] public List<(int, (int,int))> hvAreas = new List<(int, (int, int))>();
    [HideInInspector] public List<(int, int, (int, int))> taskSpaces = new List<(int, int, (int, int))>();//area, obj Angle, (hA, vA)
    //Current task space
    [HideInInspector] public int hA;
    [HideInInspector] public int vA;
    [HideInInspector] public int oA;

    [Header("Flags")]
    public int index = 0;
    public bool beforeTrial = true;
    public bool isCalibrated = false;
    public bool isStart = false;
    public bool isWaiting = false;
    [HideInInspector] public nextTrialFoot nT;
    public bool isNext;

    [HideInInspector] public Vector3 newOriginPosition;
    [HideInInspector] public Quaternion newOriginRotation;

    [HideInInspector] public message2Python data = new message2Python();

    //private System.Diagnostics.Stopwatch stopwatch;
    //[HideInInspector] public float completionTime;
    //[HideInInspector] public List<float> time = new List<float>();

    void Start()
    {
        nT = GetComponent<nextTrialFoot>();
        isNext = nT.isNext;
        makeTrialCombi();
        //foreach (var item in taskSpaces)
        //    Debug.Log(item.Item1+","+item.Item2); //print area, objAngle order
    }
    void Update()
    {
        isNext = nT.isNext;
        if (Input.GetKeyDown(KeyCode.LeftAlt) && !isStart)
        {
            if (!isCalibrated)
                calibrate();
            else
            {
                isStart = true;
                beforeTrial = false;
                StartCoroutine(Trial());
            }
        }
        if (!isWaiting)
        {
            makeMessage(index);
        }
    }
    IEnumerator Trial()
    {
        while (index < taskSpaces.Count)
        {
            for (int i = 3; i >0; i--)
            {
                isWaiting = true;
                text.text = i.ToString();
                yield return new WaitForSeconds(1);
            }
            text.text = " ";
            if (!isNext)
            {
                Debug.Log("Trial started: " + index);
                hA = taskSpaces[index].Item3.Item1;
                vA = taskSpaces[index].Item3.Item2;
                oA = taskSpaces[index].Item2;
                makeMessage(index);
                //Debug.Log(data.ToString());
                isWaiting = false;
                target.transform.position = Spherical2Cartesian(radius, vA, hA);
                target.transform.rotation = Quaternion.Euler(0, oA, 0);
                target.SetActive(true);
                //stopwatch = System.Diagnostics.Stopwatch.StartNew();
                yield return new WaitUntil(() => isNext == true);
                //makeMessage(index);
                //stopwatch.Stop();
                //completionTime = (float)stopwatch.Elapsed.TotalSeconds;
                //time.Add(completionTime);
                index++;
                Debug.Log("Moving on to next trial");
            }
        }
        text.text = "End of Order";
        Debug.Log("end");
    }

    public void calibrate()
    {
        newOriginPosition = camera.transform.position;
        newOriginRotation = camera.transform.rotation;
        Debug.Log("calibrated");
        text.text = "calibrated";
        isCalibrated = true;
    }

    public void makeTrialCombi()
    {
        horizontalAngles = new List<int> { horizontalAngle1, horizontalAngle2, horizontalAngle3 };
        verticalAngles = new List<int> { verticalAngle1, verticalAngle2, verticalAngle3 };
        objAngles = new List<int> { objAngle1, objAngle2, objAngle3 };
        foreach (int hA in horizontalAngles)
            foreach (int vA in verticalAngles)
                hvAngles.Add((hA, vA));
        foreach (var angle in hvAngles)
        {
            int areaResult = area(angle.Item1, angle.Item2);
            hvAreas.Add((areaResult, (angle.Item1, angle.Item2)));
        }
        foreach (var hvA in hvAreas)
            foreach (int oA in objAngles)
                taskSpaces.Add((hvA.Item1, oA, hvA.Item2));
        Debug.Log(hvAreas.Count); //9
        Debug.Log("hvAngles: " + hvAngles.Count); //9
        Debug.Log("taskSpaces: " + taskSpaces.Count); //27
        //for (int i = 0; i < hvAreas.Count; i++)
        //{
        //    taskSpaces.Add((hvAreas[i], objAngles[i], hvAngles[i]));
        //}
        System.Random rnd = new System.Random();
        //taskSpaces = taskSpaces.OrderBy(x => rnd.Next()).ToList();//error
        int n = taskSpaces.Count;
        while (n > 1)
        {
            n--;
            int k = rnd.Next(n + 1);
            var value = taskSpaces[k];
            taskSpaces[k] = taskSpaces[n];
            taskSpaces[n] = value;
        }
    }
    public int area(int hA, int vA)
    {
        if (vA == 60)
        {
            if (hA == 120)
                return 1;
            if (hA == 90)
                return 2;
            if (hA == 60)
                return 3;
        }
        if (vA == 90)
        {
            if (hA == 120)
                return 4;
            if (hA == 90)
                return 5;
            if (hA == 60)
                return 6;
        }
        if (vA == 120)
        {
            if (hA == 120)
                return 7;
            if (hA == 90)
                return 8;
            if (hA == 60)
                return 9;
        }
        return 0;
    }
    public Vector3 Spherical2Cartesian(float r, float phi, float theta) //theta: horizontal, phi: vertical
    {
        float phiRad = phi * Mathf.Deg2Rad;
        float thetaRad = theta * Mathf.Deg2Rad;
        float x = r * Mathf.Cos(thetaRad) * Mathf.Sin(phiRad);
        float z = r * Mathf.Sin(thetaRad) * Mathf.Sin(phiRad);
        float y = r * Mathf.Cos(phiRad);
        Vector3 cartesianCoord = new Vector3(x, y, z);
        Vector3 rotatedCoord = Quaternion.Inverse(newOriginRotation) * cartesianCoord;
        //Vector3 additionalOffset = new Vector3(0f, -0.1f, 0f);
        //Vector3 offset = newOriginPosition + additionalOffset;
        Vector3 offset = newOriginPosition;
        return rotatedCoord + offset;
    }

    public void makeMessage(int i)
    {
        //message2Python data = new message2Python();
        data.isStart = isWaiting;
        data.isDone = isNext;
        data.areaNum = taskSpaces[i].Item1;
        data.objAng = taskSpaces[i].Item2;
        data.hAng = taskSpaces[i].Item3.Item1;
        data.vAng = taskSpaces[i].Item3.Item2;
    }
}
