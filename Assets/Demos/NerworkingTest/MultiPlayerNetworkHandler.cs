using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class MultiPlayerNetworkHandler : MonoBehaviour
{
    [Header("Network")]
    [SerializeField] private string _remoteAddress = "127.0.0.1";
    [SerializeField] private int _remotePort = 9050;
    [SerializeField] private int _localPort = 9051;

    [Header("Send options")]
    [SerializeField] private float _sendInterval = 0.05f; // optional automatic send loop interval

    private UdpClient _sendClient;
    private UdpClient _recvClient;
    private Thread _recvThread;
    private readonly Queue<string> _recvQueue = new Queue<string>();
    private readonly object _queueLock = new object();
    private volatile bool _running;

    void Start()
    {
        StartClient();
        // optional: start automatic transmission loop if you want to broadcast regularly
        // StartCoroutine(AutoSendLoop());
    }

    void Update()
    {
        // Drain received messages on Unity main thread
        lock (_queueLock)
        {
            while (_recvQueue.Count > 0)
            {
                string msg = _recvQueue.Dequeue();
                HandleIncomingMessage(msg);
            }
        }
    }

    void OnDisable()
    {
        StopClient();
    }

    public void StartClient()
    {
        StopClient(); // safe restart

        try
        {
            _sendClient = new UdpClient(); // not bound, used for sending
            _recvClient = new UdpClient(_localPort); // bound for receiving

            _running = true;
            _recvThread = new Thread(ReceiveLoop) { IsBackground = true };
            _recvThread.Start();
        }
        catch (Exception ex)
        {
            Debug.LogError("UdpClientHandler start failed: " + ex);
            StopClient();
        }
    }

    public void StopClient()
    {
        _running = false;

        try
        {
            if (_recvClient != null)
            {
                _recvClient.Close();
                _recvClient = null;
            }
        }
        catch { /* ignore */ }

        try
        {
            if (_sendClient != null)
            {
                _sendClient.Close();
                _sendClient = null;
            }
        }
        catch { /* ignore */ }

        try
        {
            if (_recvThread != null && _recvThread.IsAlive)
            {
                _recvThread.Join(100);
                _recvThread = null;
            }
        }
        catch { /* ignore */ }
    }

    private void ReceiveLoop()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

        while (_running)
        {
            try
            {
                byte[] data = _recvClient.Receive(ref remoteEP); // blocking
                if (data != null && data.Length > 0)
                {
                    string msg = Encoding.UTF8.GetString(data);
                    lock (_queueLock)
                    {
                        _recvQueue.Enqueue(msg);
                    }
                }
            }
            catch (SocketException se)
            {
                // socket closed or receive error
                if (!_running)
                    break;
                Debug.LogWarning("Udp receive socket exception: " + se.SocketErrorCode);
            }
            catch (Exception ex)
            {
                Debug.LogError("Udp receive error: " + ex);
            }
        }
    }

    // Public: send arbitrary UTF-8 message to remote
    public void Send(string message)
    {
        if (_sendClient == null)
            return;

        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            _sendClient.Send(bytes, bytes.Length, _remoteAddress, _remotePort);
        }
        catch (Exception ex)
        {
            Debug.LogError("Udp send failed: " + ex);
        }
    }

    // Convenience: send player position + rotation as a compact text message (JSON-like)
    public void SendPosition(Vector3 pos, Quaternion rot, string playerName = null)
    {
        // InvariantCulture to ensure '.' decimal separator across locales
        string msg = string.Format(CultureInfo.InvariantCulture,
            "{{\"type\":\"pos\",\"name\":\"{0}\",\"x\":{1},\"y\":{2},\"z\":{3},\"rx\":{4},\"ry\":{5},\"rz\":{6},\"rw\":{7}}}",
            string.IsNullOrEmpty(playerName) ? "?" : playerName,
            pos.x, pos.y, pos.z,
            rot.x, rot.y, rot.z, rot.w);

        Send(msg);
    }

    private void HandleIncomingMessage(string msg)
    {
        // Minimal handler: you should parse JSON or custom protocol here.
        // Example: print and optionally react.
        Debug.Log("[UDP RX] " + msg);

        // TODO: parse message and update player objects / queue events for other systems.
    }

    // Optional coroutine to send repeated state (call StartCoroutine(AutoSendLoop()))
    private System.Collections.IEnumerator AutoSendLoop()
    {
        while (_running)
        {
            // Example: send a heartbeat or local player state
            // Replace with actual player references in your project
            //SendPosition(localTransform.position, localTransform.rotation, "LocalPlayer");

            yield return new WaitForSeconds(_sendInterval);
        }
    }
}
