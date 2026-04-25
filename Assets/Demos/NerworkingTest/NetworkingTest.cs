using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.WiiU;


public class NetworkingTest : MonoBehaviour {

    public Text textOb;
    public Text textObaddress;

    public bool IsPC;
    public string PCAddress;
    public int PCAddressPort;



    [Header("Data")]
    [SerializeField] public string UserName;
    [SerializeField] public int RndValueTemp;

    // Use this for initialization
    void Start ()
    {
        udp = new UdpClient();


        uint rawIp = AutoConnection.address;
        string ipString = IntToIP(rawIp);

        textObaddress.text = (ipString);
    }

    string IntToIP(uint ip)
    {
        return string.Format("{0}.{1}.{2}.{3}",
            (ip >> 24) & 0xFF,
            (ip >> 16) & 0xFF,
            (ip >> 8) & 0xFF,
            ip & 0xFF);
    }

    // Update is called once per frame
    void Update ()
    {
        SendData();

    }


    public int XposValue = 0;

    private UdpClient udp;

    public void SendData()
    {
        // Update value like your original code
        XposValue += 2;

        // Create a simple message (like a fake form)
        string message = "username=" + UserName +
                         "&name=XCordinate" +
                         "&value=" + XposValue;

        // Convert to bytes
        byte[] data = Encoding.UTF8.GetBytes(message);

        // Send to server
        udp.Send(data, data.Length, PCAddress, PCAddressPort);

        Debug.Log("Sent: " + message);
    }

}
