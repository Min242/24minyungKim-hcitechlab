//use StreamWriter
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Varjo.XR;
using UnityEngine.Events;
using TMPro;
using Leap;
using Leap.Unity;
using System.Diagnostics;

public class gazeNHand : MonoBehaviour
{
    public GameObject camera;
    public TextMeshPro text;

    public UnityEvent recordToggleEvent;
    public UnityEvent stopRecord;

    private const string ValidString = "VALID";
    private const string InvalidString = "INVALID";

    private static readonly List<string> gazeColumnNames = new List<string>{ "Frame", "CaptureTime", "HMDPosition", "HMDRotation",
        "GazeStatus", "CombinedGazeForwardX", "CombinedGazeForwardY","CombinedGazeForwardZ",
        "CombinedGazeOriginX","CombinedGazeOriginY","CombinedGazeOriginZ",
        "HitPointX", "HitPointY", "HitPointZ",
        "InterPupillaryDistanceInMM", "LeftEyeStatus", "LeftEyeForward", "LeftEyePosition", "LeftPupilIrisDiameterRatio", "LeftPupilDiameterInMM", "LeftIrisDiameterInMM",
        "RightEyeStatus", "RightEyeForward", "RightEyePosition", "RightPupilIrisDiameterRatio", "RightPupilDiameterInMM", "RightIrisDiameterInMM", "FocusDistance", "FocusStability"};
    private static readonly List<string> columnNames = new List<string>();
    private StreamWriter writer = null;

    public string path;
    public bool isLogging;

    private float fps;

    public LeapProvider leapProvider;
    public Controller controller;
    //public Hand _leftHand;
    public Hand _rightHand;

    private Stopwatch stopwatch = new Stopwatch();

    void Start()
    {
        makeHandColumn();

        recordToggleEvent.AddListener(StartLogging);
        //recordToggleEvent.AddListener(logGaze);

        string logPath = @"C:Assets\Logs\";   
        DateTime now = DateTime.Now;
        string fileName = string.Format("{0}-{1:00}-{2:00}-{3:00}-{4:00}", now.Year, now.Month, now.Day, now.Hour, now.Minute);
        path = logPath + fileName + ".csv";

        controller = new Leap.Controller();

    }
    void Update()
    {
        //Hand _leftHand = Hands.Provider.GetHand(Chirality.Left);
        _rightHand = Hands.Provider.GetHand(Chirality.Right);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isLogging)
            {
                this.recordToggleEvent.Invoke();
                UnityEngine.Debug.Log("Start logging");
                stopwatch.Start();
            }
            else
            {
                StopLogging();
                text.text = "Stop Logging";
                stopwatch.Stop();
            }
        }
        if (isLogging)
        {
            logGaze();
            //fps = 1 / Time.deltaTime;
            fps = 1.0f / (stopwatch.ElapsedMilliseconds / 1000.0f);
            stopwatch.Restart();
        }
    }

    void logGaze()
    {
        //int dataCount = VarjoEyeTracking.GetGazeList(out gazeData, out eyeMeasurements);
        VarjoEyeTracking.GazeData gazeData = VarjoEyeTracking.GetGaze();
        VarjoEyeTracking.EyeMeasurements eyeMeasurements = VarjoEyeTracking.GetEyeMeasurements();
        List<string> logData = new List<string>();//GazeData: 29°³ + Hand: 
        logData.Add(gazeData.frameNumber.ToString());
        logData.Add(gazeData.captureTime.ToString());
        logData.Add(camera.transform.localPosition.ToString());//HMD
        logData.Add(camera.transform.localRotation.ToString());
        bool invalid = gazeData.status == VarjoEyeTracking.GazeStatus.Invalid;
        logData.Add(invalid ? InvalidString : ValidString);//gaze status
        Vector3 transformGaze = transform.TransformDirection(gazeData.gaze.forward);
        logData.Add(invalid ? "" : transformGaze.x.ToString()); //gaze forward
        logData.Add(invalid ? "" : transformGaze.y.ToString());
        logData.Add(invalid ? "" : transformGaze.z.ToString());
        Vector3 transformOrigin = transform.TransformPoint(gazeData.gaze.origin);
        logData.Add(invalid ? "" : transformOrigin.x.ToString());// gaze forward origin X
        logData.Add(invalid ? "" : transformOrigin.y.ToString());
        logData.Add(invalid ? "" : transformOrigin.z.ToString());
        Ray gazeRay = new Ray(transformOrigin, transformGaze);
        RaycastHit hit;
        if (Physics.Raycast(gazeRay, out hit))
        {
            logData.Add(invalid ? "" : hit.point.x.ToString());// hitPoint
            logData.Add(invalid ? "" : hit.point.y.ToString());
            logData.Add(invalid ? "" : hit.point.z.ToString());
        }
        else
        {
            Vector3 pointAtDistance = transformOrigin + transformGaze.normalized * 0.6f;
            logData.Add(invalid ? "" : pointAtDistance.x.ToString());// on screen
            logData.Add(invalid ? "" : pointAtDistance.y.ToString());
            logData.Add(invalid ? "" : pointAtDistance.z.ToString());
        }//14

        logData.Add(invalid ? "" : eyeMeasurements.interPupillaryDistanceInMM.ToString("F3")); //IPD
        bool leftInvalid = gazeData.leftStatus == VarjoEyeTracking.GazeEyeStatus.Invalid;
        logData.Add(leftInvalid ? InvalidString : ValidString); //left eye
        logData.Add(leftInvalid ? "" : gazeData.left.forward.ToString("F3"));
        logData.Add(leftInvalid ? "" : gazeData.left.origin.ToString("F3"));
        logData.Add(leftInvalid ? "" : eyeMeasurements.leftPupilIrisDiameterRatio.ToString("F3"));
        logData.Add(leftInvalid ? "" : eyeMeasurements.leftPupilDiameterInMM.ToString("F3"));
        logData.Add(leftInvalid ? "" : eyeMeasurements.leftPupilDiameterInMM.ToString("F3"));
        bool rightInvalid = gazeData.rightStatus == VarjoEyeTracking.GazeEyeStatus.Invalid; // Right Eye
        logData.Add(rightInvalid ? InvalidString : ValidString);
        logData.Add(rightInvalid ? "" : gazeData.right.forward.ToString("F3"));
        logData.Add(rightInvalid ? "" : gazeData.right.origin.ToString("F3"));
        logData.Add(rightInvalid ? "" : eyeMeasurements.rightPupilIrisDiameterRatio.ToString("F3"));
        logData.Add(rightInvalid ? "" : eyeMeasurements.rightPupilDiameterInMM.ToString("F3"));
        logData.Add(rightInvalid ? "" : eyeMeasurements.rightIrisDiameterInMM.ToString("F3"));
        logData.Add(invalid ? "" : gazeData.focusDistance.ToString()); //focus distance
        logData.Add(invalid ? "" : gazeData.focusStability.ToString());//15. Tot. 29

        ///Hand
        Frame frame = leapProvider.CurrentFrame; // 30
        logData.Add(controller.Now().ToString());
        foreach (var hand in frame.Hands)
        {
            if (hand.IsRight)
            {
                logData.Add(_rightHand.PalmPosition.x.ToString()); // Palm Position
                logData.Add(_rightHand.PalmPosition.y.ToString());
                logData.Add(_rightHand.PalmPosition.z.ToString());
                logData.Add(_rightHand.PalmVelocity.x.ToString()); //Palm Velocity
                logData.Add(_rightHand.PalmVelocity.y.ToString());
                logData.Add(_rightHand.PalmVelocity.z.ToString());
                logData.Add(_rightHand.PalmNormal.x.ToString()); //Palm Normal
                logData.Add(_rightHand.PalmNormal.y.ToString());
                logData.Add(_rightHand.PalmNormal.z.ToString());
                logData.Add(_rightHand.Direction.x.ToString());// Palm Direction
                logData.Add(_rightHand.Direction.y.ToString());
                logData.Add(_rightHand.Direction.z.ToString());
                logData.Add(_rightHand.Rotation.x.ToString());// Palm Orientation
                logData.Add(_rightHand.Rotation.y.ToString());
                logData.Add(_rightHand.Rotation.z.ToString());
                logData.Add(_rightHand.Rotation.w.ToString()); //49
                int index = 46;
                foreach (Finger _finger in _rightHand.Fingers)
                {
                    for (int b = 0; b < 4; b++)
                    {
                        //Debug.Log(_finger.bones.Length);
                        Bone _bone = _finger.bones[b];
                        logData.Add(_bone.PrevJoint.x.ToString());//PrevJoint Position
                        logData.Add(_bone.PrevJoint.y.ToString());
                        logData.Add(_bone.PrevJoint.z.ToString());
                        logData.Add(_bone.Center.x.ToString());//Bone Center Position
                        logData.Add(_bone.Center.y.ToString());
                        logData.Add(_bone.Center.z.ToString());
                        logData.Add(_bone.Direction.x.ToString());//Base to Tip Direction
                        logData.Add(_bone.Direction.y.ToString());
                        logData.Add(_bone.Direction.z.ToString());
                        logData.Add(_bone.Rotation.x.ToString());//Rotation
                        logData.Add(_bone.Rotation.y.ToString());
                        logData.Add(_bone.Rotation.z.ToString());
                        logData.Add(_bone.Rotation.w.ToString());
                    }
                }
            }
            else
            {
                logData[30] = logData[31] = logData[32] = logData[33] = logData[34] = logData[35] = InvalidString; // Palm Position and Palm Velocity
                logData[36] = logData[37] = logData[38] = logData[39] = logData[40] = logData[41] = InvalidString; // Palm Normal and Palm Direction
                logData[42] = logData[43] = logData[44] = logData[45] = InvalidString; // Palm Orientation

                for (int index = 46; index < logData.Count; index++)
                {
                    logData[index] = InvalidString; // Finger and Bone data
                }
            }
        }
        logData.Add(fps.ToString());
        Log(logData);
    }
    void StartLogging()
    {
        //string header = string.Join(";", columnNames) + Environment.NewLine;
        //string header = columnNames + ";" + Environment.NewLine;
        writer = new StreamWriter(path);
        //File.AppendAllText(path, header);
        Log(columnNames);
        UnityEngine.Debug.Log("Log file started at: " + path);
        isLogging = true;
    }
    void Log(List<string> values)
    {
        string line = "";
        for (int i = 0; i < values.Count; ++i)
        {
            values[i] = values[i].Replace("\r", "").Replace("\n", ""); // Remove new lines so they don't break csv
            line += values[i] + (i == (values.Count - 1) ? "" : ";"); // Do not add semicolon to last data string
        }
        writer.WriteLine(line);
    }

    void StopLogging()
    {
        if (!isLogging)
            return;
        if (writer != null)
        {
            writer.Flush();
            writer.Close();
            writer = null;
        }
        isLogging = false;
        UnityEngine.Debug.Log("Logging ended");
    }


    public static List<string> makeHandColumn()
    {
        List<string> handC = new List<string> { "Time", "Palm Position X", "Palm Position Y", "Palm Position Z",
                                                  "Palm Velocity X", "Palm Velocity Y", "Palm Velocity Z",
                                                  "Palm Normal X", "Palm Normal Y", "Palm Normal Z",
                                                  "Palm Direction X", "Palm Direction Y", "Palm Direction Z",
                                                  "Palm Orientation X", "Palm Orientation Y", "Palm Orientation Z", "Palm Orientation W" };
        for (int i = 1; i <= 5; i++)
            for (int j = 1; j <= 4; j++)
            {
                handC.AddRange(new string[] { $"Finger {i} Bone {j} PrevJoint Position X", $"Finger {i} Bone {j} PrevJoint Position Y", $"Finger {i} Bone {j} PrevJoint Position Z",
                                            $"Finger {i} Bone {j} Center Position X", $"Finger {i} Bone {j} Center Position Y", $"Finger {i} Bone {j} Center Position Z",
                                            $"Finger {i} Bone {j} Direction X", $"Finger {i} Bone {j} Direction Y", $"Finger {i} Bone {j} Direction Z",
                                               $"Finger {i} Bone {j} Rotation X",
                                               $"Finger {i} Bone {j} Rotation Y",
                                               $"Finger {i} Bone {j} Rotation Z",
                                               $"Finger {i} Bone {j} Rotation W" });
            }
        columnNames.AddRange(gazeColumnNames);
        columnNames.AddRange(handC);
        columnNames.AddRange(new List<string> { "FPS" });
        return columnNames;
    }

}
