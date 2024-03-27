using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class PythonTest : MonoBehaviour
{
    //int numToSendToPython = 0;
    public UdpSocket udpSocket;

    public testManager tM;
    [HideInInspector] public message2Python data;

    //[HideInInspector] public string dataStr;


    public void QuitApp()
    {
        print("Quitting");
        Application.Quit();
    }
    public void SendToPython(string input)
    {
        udpSocket.SendData(input);
        //numToSendToPython++;
        //Debug.Log("Send :" + data.ToString());
        Debug.Log("Send: " + input);
    }

    void Start()
    {
        //udpSocket = FindObjectOfType<UdpSocket>();
        //Debug.Log("Send Number :" + numToSendToPython);
    }
    void Update()
    {
        data = tM.data;
        //Debug.Log(data.ToString());
    }
}
