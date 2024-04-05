using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Leap;
using Leap.Unity;
using System;
using System.IO;
using System.Diagnostics;

public class pracCallHand : MonoBehaviour
{
    public LeapProvider leapProvider;
    [HideInInspector] public Controller controller;
    Hand _rightHand;

    public pracMain pM;

    /// <summary>
    /// Booleans
    /// </summary>
    private bool isLoggingHand = false;
    public bool printFramerateHand = false;

    private StreamWriter writer = null;

    /// <summary>
    /// count fps, frametime
    /// </summary>
    long deltaTick;
    long prevTick = 1000;
    long microseconds;
    double fps;
    float mili;
    int rowIdx = 0;
    private Stopwatch sw = new Stopwatch();
    float currTime;
    float FPS;
    private long prevtime = 0;
    long ticks;

    float handTimer = 0f;
    int handDataCount = 0;

    void Start()
    {
        controller = new Controller();
    }

    private void OnEnable()
    {
        leapProvider.OnUpdateFrame += OnUpdateFrame;
    }
    private void OnDisable()
    {
        leapProvider.OnUpdateFrame -= OnUpdateFrame;
        StopLoggingHand();
    }
    void OnUpdateFrame(Frame frame)
    {
        //_rightHand = Hands.Provider.GetHand(Chirality.Right);
        if (!isLoggingHand)
            StartLoggingHand();
        if (isLoggingHand && printFramerateHand)
        {
            handTimer += Time.deltaTime;
            if (handTimer >= 1.0f)
            {
                UnityEngine.Debug.Log("Hand data rows per second: " + handDataCount);
                handDataCount = 0;
                handTimer = 0f;
            }
        }
        if (isLoggingHand)
        {
            _rightHand = frame.GetHand(Chirality.Right);
            currTime = controller.Now() - prevtime;
            fps = 1000 * 1000 / currTime;
            prevtime = controller.Now();
            if (_rightHand != null)
            {
                OnUpdateHand(_rightHand);
            }
            else
                makeBlankRow();
            rowIdx++;
            sw.Stop();
            deltaTick = sw.ElapsedTicks - prevTick;
            microseconds = deltaTick / (Stopwatch.Frequency / (1000L * 1000L)); //microseconds
            FPS = (float)1000000 / (float)microseconds;
            mili = (float)microseconds / (float)1000;
            prevTick = sw.ElapsedTicks;
            sw.Start();
        }
    }
    void Log(string[] values)
    {
        if (!isLoggingHand || writer == null)
            return;
        string line = "";
        for (int i = 0; i < values.Length; ++i)
        {
            //values[i] = values[i].Replace("\r", "").Replace("\n", ""); // Remove new lines so they don't break csv
            values[i] = values[i] == null ? "" : values[i].Replace("\r", "").Replace("\n", "");
            line += values[i] + (i == (values.Length - 1) ? "" : ";"); // Do not add semicolon to last data string
        }
        writer.WriteLine(line);
    }
    void StartLoggingHand()
    {
        if (isLoggingHand)
        {
            UnityEngine.Debug.LogWarning("Logging was on when StartLogging was called. No new log was started.");
            return;
        }
        isLoggingHand = true;
        string logPath = Application.dataPath + "/Logs/";
        Directory.CreateDirectory(logPath);
        DateTime now = DateTime.Now;
        string fileName = string.Format("Hand-{0}-{1:00}-{2:00}-{3:00}-{4:00}",
            now.Year, now.Month, now.Day, now.Hour, now.Minute);
        string path = logPath + fileName + ".csv";
        writer = new StreamWriter(path);
        string[] ColumnNames = makeHandColumn();
        Log(ColumnNames);
        UnityEngine.Debug.Log("Hand Logging Start");
    }
    //void LogHand(Hand _hand)
    //{
    //    currTime = controller.Now() - prevtime;
    //    fps = 1000 * 1000 / currTime;
    //    prevtime = controller.Now();
    //    if (_hand != null)
    //    {
    //        OnUpdateHand(_hand);
    //    }
    //    else
    //        makeBlankRow();
    //    rowIdx++;
    //}
    void OnUpdateHand(Hand _hand)
    {
        string[] logData = new string[281];
        logData[0] = rowIdx.ToString();
        logData[1] = currTime.ToString();
        logData[2] = fps.ToString();
        logData[3] = mili.ToString();
        double elapsedTime = (pM.sw3.ElapsedTicks / Stopwatch.Frequency) * 1_000_000_000;
        logData[3] = elapsedTime.ToString();
        logData[5] = _rightHand.PalmPosition.x.ToString();
        logData[6] = _rightHand.PalmPosition.y.ToString();
        logData[7] = _rightHand.PalmPosition.z.ToString();
        logData[8] = _rightHand.PalmVelocity.x.ToString();
        logData[9] = _rightHand.PalmVelocity.y.ToString();
        logData[10] = _rightHand.PalmVelocity.z.ToString();
        logData[11] = _rightHand.PalmNormal.x.ToString();
        logData[12] = _rightHand.PalmNormal.y.ToString();
        logData[13] = _rightHand.PalmNormal.z.ToString();
        logData[14] = _rightHand.Direction.x.ToString();
        logData[15] = _rightHand.Direction.y.ToString();
        logData[16] = _rightHand.Direction.z.ToString();
        logData[17] = _rightHand.Rotation.x.ToString();
        logData[18] = _rightHand.Rotation.y.ToString();
        logData[19] = _rightHand.Rotation.z.ToString();
        logData[20] = _rightHand.Rotation.w.ToString();
        int index = 21;
        foreach (Finger _finger in _hand.Fingers)
        {
            for (int b = 0; b < 4; b++)
            {
                Bone _bone = _finger.bones[b];
                logData[index++] = _bone.PrevJoint.x.ToString();
                logData[index++] = _bone.PrevJoint.y.ToString();
                logData[index++] = _bone.PrevJoint.z.ToString();
                logData[index++] = _bone.Center.x.ToString();
                logData[index++] = _bone.Center.y.ToString();
                logData[index++] = _bone.Center.z.ToString();
                logData[index++] = _bone.Direction.x.ToString();
                logData[index++] = _bone.Direction.y.ToString();
                logData[index++] = _bone.Direction.z.ToString();
                logData[index++] = _bone.Rotation.x.ToString();
                logData[index++] = _bone.Rotation.y.ToString();
                logData[index++] = _bone.Rotation.z.ToString();
                logData[index++] = _bone.Rotation.w.ToString();
            }
        }
        Log(logData);
    }
    void makeBlankRow()
    {
        string[] blankRow = new string[281];
        blankRow[0] = rowIdx.ToString();
        blankRow[1] = currTime.ToString();
        blankRow[2] = fps.ToString();
        blankRow[3] = mili.ToString();
        double elapsedTime = (pM.sw3.ElapsedTicks / Stopwatch.Frequency) * 100;
        blankRow[4] = elapsedTime.ToString();
        Log(blankRow);
    }
    private string[] makeHandColumn()
    {
        string[] handC = new string[281];
        string[] initialData = new string[] { "Idx", "CurrTime", "FPS", "Frametime(ms)", "SystemTime",
                    "Palm Position X", "Palm Position Y", "Palm Position Z",
                    "Palm Velocity X", "Palm Velocity Y", "Palm Velocity Z",
                    "Palm Normal X", "Palm Normal Y", "Palm Normal Z",
                    "Palm Direction X", "Palm Direction Y", "Palm Direction Z",
                    "Palm Orientation X", "Palm Orientation Y", "Palm Orientation Z", "Palm Orientation W"};
        initialData.CopyTo(handC, 0);
        int index = 20;
        for (int i = 1; i <= 5; i++)
            for (int j = 1; j <= 4; j++)
            {
                handC[index++] = $"Finger{i} Bone{j} Prevjoint Position X";
                handC[index++] = $"Finger{i} Bone{j} Prevjoint Position Y";
                handC[index++] = $"Finger{i} Bone{j} Prevjoint Position Z";
                handC[index++] = $"Finger {i} Bone{j} Center Position X";
                handC[index++] = $"Finger {i} Bone{j} Center Position Y";
                handC[index++] = $"Finger {i} Bone{j} Center Position Z";
                handC[index++] = $"Finger {i} Bone{j} Direction Position X";
                handC[index++] = $"Finger {i} Bone{j} Direction Position Y";
                handC[index++] = $"Finger {i} Bone{j} Direction Position Z";
                handC[index++] = $"Finger{i} Bone{j} Rotation X";
                handC[index++] = $"Finger{i} Bone{j} Rotation Y";
                handC[index++] = $"Finger{i} Bone{j} Rotation Z";
                handC[index++] = $"Finger{i} Bone{j} Rotation W";
            }
        return handC;
    }
    void StopLoggingHand()
    {
        if (!isLoggingHand)
            return;

        if (writer != null)
        {
            writer.Flush();
            writer.Close();
            writer = null;
        }
        isLoggingHand = false;
        UnityEngine.Debug.Log("Logging Hand ended");
    }

}
