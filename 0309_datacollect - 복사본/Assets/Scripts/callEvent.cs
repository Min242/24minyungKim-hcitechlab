using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class callEvent : MonoBehaviour
{
    public UnityEvent onStartSending;
    public testManager tM;
    [HideInInspector] public bool isWaiting;
    [HideInInspector] public bool isCalibrated;
    [HideInInspector] public bool beforeTrial;
    [HideInInspector] public message2Python data;

    [HideInInspector] public PythonTest pT;

    void Start()
    {
        //tM = FindObjectOfType<testManager>();
        isWaiting = tM.isWaiting;
        isCalibrated = tM.isCalibrated;
        pT = GetComponent<PythonTest>();
    }
    public void taskStart()
    {
        onStartSending.Invoke();
    }
    void Update()
    {
        data = tM.data;
        isWaiting = tM.isWaiting;
        isCalibrated = tM.isCalibrated;
        beforeTrial = tM.beforeTrial;
        if (!isWaiting && isCalibrated && !beforeTrial)
        {
            pT.SendToPython(data.ToString());
        }
    }
}
