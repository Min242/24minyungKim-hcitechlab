using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Diagnostics;

public class pracMain : MonoBehaviour
{
    [Header("Assign")]
    public GameObject callHand;
    public GameObject callGaze;

    [HideInInspector] public bool isLogging = false;

    [HideInInspector] public UnityEvent recordToggle;
    [HideInInspector] public UnityEvent stopRecording;

    [HideInInspector] public Stopwatch sw3 = new Stopwatch();


    void Start()
    {
        recordToggle.AddListener(delegate { callGaze.SetActive(true); });
        recordToggle.AddListener(delegate { callHand.SetActive(true); });
        stopRecording.AddListener(delegate { callGaze.SetActive(false); });
        stopRecording.AddListener(delegate { callHand.SetActive(false); });

        callHand.SetActive(false);
        callGaze.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isLogging)
            {
                this.recordToggle.Invoke();
                isLogging = true;
                sw3.Start();
                //Debug.Log("Start: " + DateTime.Now.Ticks);
            }
            else
            {
                this.stopRecording.Invoke();
                isLogging = false;
                UnityEngine.Debug.Log("Stop: " + sw3.ElapsedTicks);
                sw3.Stop();
            }
            double elapsedTime = (sw3.ElapsedTicks / Stopwatch.Frequency) * 100;
            UnityEngine.Debug.Log("Log: " + isLogging + ", Time: " + elapsedTime);
        }
    }
}
