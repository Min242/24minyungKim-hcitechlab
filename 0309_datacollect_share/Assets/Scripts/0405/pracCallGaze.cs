using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.XR;
using Varjo.XR;
using System.Diagnostics;


public class pracCallGaze : MonoBehaviour
{
    [Header("Assign")]
    public GameObject camera;
    public pracMain pM;

    /// <summary>
    /// Booleans
    /// </summary>
    //private bool first = true;
    private bool isLoggingGaze = false;
    public bool printFramerate = false;

    float gazeTimer = 0f;
    int gazeDataCount = 0;

    private List<VarjoEyeTracking.GazeData> dataSinceLastUpdate;
    private List<VarjoEyeTracking.EyeMeasurements> eyeMeasurementsSinceLastUpdate;
    private StreamWriter writer = null;

    private static readonly string[] ColumnNames =
        { "Frame", "CaptureTime", "Current Time", "LogTime", "fps", "frametime(ms)", "us" ,
        "HMDPosition", "HMDRotation", "GazeStatus",
        "CombinedGazeForwardX", "CombinedGazeForwardY","CombinedGazeForwardZ",
        "CombinedGazeOriginX", "CombinedGazeOriginY", "CombinedGazeOriginZ",
        "HitPointX", "HitPointY", "HitPointZ",
        "InterPupillaryDistanceInMM", "LeftEyeStatus", "LeftEyeForward", "LeftEyePosition", "LeftPupilIrisDiameterRatio", "LeftPupilDiameterInMM", "LeftIrisDiameterInMM",
        "RightEyeStatus", "RightEyeForward", "RightEyePosition", "RightPupilIrisDiameterRatio", "RightPupilDiameterInMM", "RightIrisDiameterInMM",
        "FocusDistance", "FocusStability"};
    private const string ValidString = "VALID";
    private const string InvalidString = "INVALID";

    /// <summary>
    /// count fps, frametime
    /// </summary>
    long deltaTick;
    long prevTick = 1000;
    long microseconds;
    double fps;
    float mili;
    private Stopwatch sw = new Stopwatch();
    //private Stopwatch sw2 = new Stopwatch();

    void Update()
    {
        if (!isLoggingGaze)
            StartLoggingGaze();
        if (isLoggingGaze && printFramerate)
        {
            gazeTimer += Time.deltaTime;
            if (gazeTimer >= 1.0f)
            {
                UnityEngine.Debug.Log("Gaze data rows per second: " + gazeDataCount);
                gazeDataCount = 0;
                gazeTimer = 0f;
            }
        }
        if (isLoggingGaze)
        {
            int dataCount = VarjoEyeTracking.GetGazeList(out dataSinceLastUpdate, out eyeMeasurementsSinceLastUpdate);
            if (printFramerate) gazeDataCount += dataCount;
            for (int i = 0; i < dataCount; i++)
            {
                LogGazeData(dataSinceLastUpdate[i], eyeMeasurementsSinceLastUpdate[i]);
            }
            sw.Stop();
            deltaTick = sw.ElapsedTicks - prevTick;
            microseconds = deltaTick / (Stopwatch.Frequency / (1000L * 1000L)); //microseconds
            fps = (float)1000000 / (float)microseconds;
            mili = (float)microseconds / (float)1000;
            prevTick = sw.ElapsedTicks;
            sw.Start();
        }
    }
    void OnDisable()
    {
        StopLoggingGaze();
        //UnityEngine.Debug.Log("gaze end: " + sw2.ElapsedTicks);
    }

    void LogGazeData(VarjoEyeTracking.GazeData data, VarjoEyeTracking.EyeMeasurements eyeMeasurements)
    {
        string[] logData = new string[34];
        logData[0] = data.frameNumber.ToString();
        logData[1] = data.captureTime.ToString();
        long currentVarjoTimestamp = VarjoTime.GetVarjoTimestamp();
        logData[2] = currentVarjoTimestamp.ToString();
        //logData[3] = DateTime.Now.Ticks.ToString();
        //logData[3] = VarjoTime.ConvertVarjoTimestampToDateTime(currentVarjoTimestamp).ToString();
        double elapsedTime = (pM.sw3.ElapsedTicks / Stopwatch.Frequency) * 100;
        logData[3] = elapsedTime.ToString();
        logData[4] = fps.ToString();
        logData[5] = mili.ToString();
        logData[6] = microseconds.ToString();
        logData[7] = camera.transform.localPosition.ToString();
        logData[8] = camera.transform.localPosition.ToString();
        bool invalid = data.status == VarjoEyeTracking.GazeStatus.Invalid;
        logData[9] = invalid ? InvalidString : ValidString;
        Vector3 transformGaze = transform.TransformDirection(data.gaze.forward);
        logData[10] = invalid ? "" : transformGaze.x.ToString();
        logData[11] = invalid ? "" : transformGaze.y.ToString();
        logData[12] = invalid ? "" : transformGaze.z.ToString();
        Vector3 transformOrigin = transform.TransformPoint(data.gaze.origin);
        logData[13] = invalid ? "" : transformOrigin.x.ToString();
        logData[14] = invalid ? "" : transformOrigin.y.ToString();
        logData[15] = invalid ? "" : transformOrigin.z.ToString();
        Ray gazeRay = new Ray(transformOrigin, transformGaze);
        RaycastHit hit;
        if (Physics.Raycast(gazeRay, out hit))
        {
            logData[16] = invalid ? "" : hit.point.x.ToString();
            logData[17] = invalid ? "" : hit.point.y.ToString();
            logData[18] = invalid ? "" : hit.point.z.ToString();
        }
        else
        {
            Vector3 pointAtDistance = transform.TransformPoint(data.gaze.origin)
                + transform.TransformDirection(data.gaze.forward).normalized * 0.6f;
            logData[16] = invalid ? "" : pointAtDistance.x.ToString(); //onScreen X
            logData[17] = invalid ? "" : pointAtDistance.y.ToString(); //onScreen Y
            logData[18] = invalid ? "" : pointAtDistance.z.ToString(); //onScreen Z
        }
        logData[19] = invalid ? "" : eyeMeasurements.interPupillaryDistanceInMM.ToString();
        bool leftInvalid = data.leftStatus == VarjoEyeTracking.GazeEyeStatus.Invalid;
        logData[20] = leftInvalid ? InvalidString : ValidString;
        logData[21] = leftInvalid ? "" : data.left.forward.ToString("F3");
        logData[22] = leftInvalid ? "" : data.left.origin.ToString("F3");
        logData[23] = leftInvalid ? "" : eyeMeasurements.leftPupilIrisDiameterRatio.ToString("F3");
        logData[24] = leftInvalid ? "" : eyeMeasurements.leftPupilDiameterInMM.ToString("F3");
        logData[25] = leftInvalid ? "" : eyeMeasurements.leftIrisDiameterInMM.ToString("F3");
        bool rightInvalid = data.rightStatus == VarjoEyeTracking.GazeEyeStatus.Invalid;
        logData[26] = rightInvalid ? InvalidString : ValidString;
        logData[27] = rightInvalid ? "" : data.right.forward.ToString("F3");
        logData[28] = rightInvalid ? "" : data.right.origin.ToString("F3");
        logData[29] = rightInvalid ? "" : eyeMeasurements.rightPupilIrisDiameterRatio.ToString("F3");
        logData[30] = rightInvalid ? "" : eyeMeasurements.rightPupilDiameterInMM.ToString("F3");
        logData[31] = rightInvalid ? "" : eyeMeasurements.rightIrisDiameterInMM.ToString("F3");
        logData[32] = invalid ? "" : data.focusDistance.ToString();
        logData[33] = invalid ? "" : data.focusStability.ToString();
        Log(logData);
    }
    void Log(string[] values)
    {
        if (!isLoggingGaze || writer == null)
            return;

        string line = "";
        for (int i = 0; i < values.Length; ++i)
        {
            values[i] = values[i].Replace("\r", "").Replace("\n", ""); // Remove new lines so they don't break csv
            line += values[i] + (i == (values.Length - 1) ? "" : ";"); // Do not add semicolon to last data string
        }
        writer.WriteLine(line);
    }
    void StartLoggingGaze()
    {
        if (isLoggingGaze)
        {
            UnityEngine.Debug.LogWarning("Logging was on when StartLogging was called. No new log was started.");
            return;
        }
        isLoggingGaze = true;
        string logPath = Application.dataPath + "/Logs/";
        Directory.CreateDirectory(logPath);
        DateTime now = DateTime.Now;
        string fileName = string.Format("Gaze-{0}-{1:00}-{2:00}-{3:00}-{4:00}",
            now.Year, now.Month, now.Day, now.Hour, now.Minute);
        string path = logPath + fileName + ".csv";
        writer = new StreamWriter(path);
        Log(ColumnNames);
        UnityEngine.Debug.Log("Gaze Logging Start");
    }
    void StopLoggingGaze()
    {
        if (!isLoggingGaze)
            return;

        if (writer != null)
        {
            writer.Flush();
            writer.Close();
            writer = null;
        }
        isLoggingGaze = false;
        UnityEngine.Debug.Log("Logging Gaze ended");
    }
}
