using UnityEngine;
using System;
using System.Text;
using System.Collections.Concurrent;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using AR_GPS;

public class MqttManager : MonoBehaviour
{
    [Header("MQTT Configuration")]
    public string mqttBroker = "mqtt.univ-cotedazur.fr";
    public int mqttPort = 8443;
    public string mqttTopic = "FABLAB_21_22/unity/testgps/in";
    public string username = "fablab2122";
    public string password = "2122";
    public bool processMessages = true;

    [Header("Out Configuration")]
    public string mqttTopicOut = "FABLAB_21_22/unity/testgps/out";
    public float reconnectDelay = 5.0f;

    [Header("References")]
    public Listeprefabs2 prefabManager;

    private MqttClient client;
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();
    private bool isReconnecting = false;

    void Start()
    {
        StartCoroutine(ConnectionRoutine());
    }

    System.Collections.IEnumerator ConnectionRoutine()
    {
        while (true)
        {
            if (client == null || !client.IsConnected)
            {
                Connect();
            }
            yield return new WaitForSeconds(reconnectDelay);
        }
    }

    void Connect()
    {
        try
        {
            if(client != null && client.IsConnected) return;

            // Attempting SSL connection on port 8443
            client = new MqttClient(mqttBroker, mqttPort, true, MqttSslProtocols.TLSv1_2, null, null);
            
            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId, username, password);

            if (client.IsConnected)
            {
                Debug.Log("Connected to MQTT Broker!");
                client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
                client.ConnectionClosed += Client_ConnectionClosed;
                client.Subscribe(new string[] { mqttTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });

                 // Send Hello Message
                string helloMsg = "Hello from unity !";
                client.Publish(mqttTopicOut, Encoding.UTF8.GetBytes(helloMsg), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
                Debug.Log("Sent Hello message to: " + mqttTopicOut);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("MQTT Connection Failed: " + e.Message);
        }
    }

    private void Client_ConnectionClosed(object sender, EventArgs e)
    {
        Debug.LogWarning("MQTT Connection Closed. Auto-reconnect will handle it.");
    }

    private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string msg = Encoding.UTF8.GetString(e.Message);
        Debug.Log("MQTT Message received: " + msg);
        messageQueue.Enqueue(msg);
    }

    void Update()
    {
        // Process received messages on the main thread
        if (processMessages && !messageQueue.IsEmpty)
        {
            string msg;
            while (messageQueue.TryDequeue(out msg))
            {
                if (prefabManager != null)
                {
                    prefabManager.UpdatePrefabsFromJSON(msg);
                }
            }
        }

        // Optional: Press 'S' to send current positions
        if (Input.GetKeyDown(KeyCode.S))
        {
            SendPositions();
        }
    }

    public void SendPositions()
    {
        if (client != null && client.IsConnected && prefabManager != null)
        {
            string json = prefabManager.GetJsonFromCurrentList();
            client.Publish(mqttTopic, Encoding.UTF8.GetBytes(json), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
            Debug.Log("Sent positions: " + json);
        }
        else
        {
            Debug.LogWarning("Cannot send positions: MQTT not connected or Manager missing.");
        }
    }

    void OnDestroy()
    {
        if (client != null && client.IsConnected)
        {
            client.Disconnect();
        }
    }
}
