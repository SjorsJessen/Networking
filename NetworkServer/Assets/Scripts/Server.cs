using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour
{
    private byte reliableChannel;
    private byte error;
    
    private const int MAX_USERS = 100;
    private const int PORT = 26000;
    private const int BYTE_SIZE = 1024;

    private int hostID;
    private int webHostID;

    private bool isStarted;
    
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Init();
    }

    public void Init()
    {
        NetworkTransport.Init();
        ConnectionConfig connectionConfig = new ConnectionConfig();
        connectionConfig.AddChannel(QosType.Reliable);
        
        HostTopology hostTopology = new HostTopology(connectionConfig, MAX_USERS);
        
        //Server only code
        hostID = NetworkTransport.AddHost(hostTopology, PORT, null);
        //webHostID = NetworkTransport.AddWebsocketHost(hostTopology, WEB_PORT, null);

        Debug.Log(string.Format("Opening connection on PORT {0}", PORT));
        isStarted = true;
    }

    private void Update()
    {
        UpdateMessagePump();
    }

    public void ShutdownNetwork()
    {
        NetworkTransport.Shutdown();
        isStarted = false;
    }
    
    public void UpdateMessagePump()
    {
        if (!isStarted) return;

        int receivingHostId; //Is this from Web? or Standalone?
        int connectionId; //Which user is sending me this?
        int channelId; //Which lane is he sending that message from?
        
        byte[] receivingBuffer = new byte[BYTE_SIZE];
        int dataSize;

       NetworkEventType networkEventType = NetworkTransport.Receive(out receivingHostId, out connectionId, out channelId, receivingBuffer, receivingBuffer.Length,
            out dataSize, out error);

        switch (networkEventType)
        {
            case NetworkEventType.Nothing:
                break;

            case NetworkEventType.ConnectEvent:
                Debug.Log(string.Format("User {0} has connected!", connectionId));
                break;
            
            case NetworkEventType.DisconnectEvent:
                Debug.Log(string.Format("User {0} has disconnected!", connectionId));
                break;
            
            case NetworkEventType.DataEvent:
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                MemoryStream memoryStream = new MemoryStream(receivingBuffer);
                NetMessage message = (NetMessage)binaryFormatter.Deserialize(memoryStream);

                OnData(connectionId, channelId, receivingHostId, message);
                break;
            
            default:
            case NetworkEventType.BroadcastEvent:
                Debug.Log("Unexpected Network Event Type");
                break;
        }
    }

    private void OnData(int connectionId, int channelId, int receivingHostId, NetMessage message)
    {
        Debug.Log("Received a message of type " + message.OperationCode);

        switch (message.OperationCode)
        {
            case NetOperationCode.None:
                break;
            case NetOperationCode.CreateAccount:
                CreateAccount(connectionId, channelId, receivingHostId, (Net_CreateAccount) message);
                break;
            case NetOperationCode.LoginRequest:
                LoginRequest(connectionId, channelId, receivingHostId, (Net_LoginRequest) message);
                break;
        }
    }

    private void CreateAccount(int connectionId, int channelId, int receivingHostId, Net_CreateAccount createAccount)
    {
        Debug.Log(string.Format("{0},{1},{2}", createAccount.Username, createAccount.Password, createAccount.Email));
        
        Net_OnCreateAccount onCreateAccount = new Net_OnCreateAccount();
        onCreateAccount.Success = 0;
        onCreateAccount.Information = "Account was created!";

        SendClient(receivingHostId, connectionId, onCreateAccount);
    }

    private void LoginRequest(int connectionId, int channelId, int receivingHostId, Net_LoginRequest loginRequest)
    {
        Debug.Log(string.Format("{0},{1}", loginRequest.UsernameOrEmail, loginRequest.Password));
        
        Net_OnLoginRequest onLoginRequest = new Net_OnLoginRequest();
        onLoginRequest.Success = 0;
        onLoginRequest.Information = "Login successful!";
        onLoginRequest.Username = "Sjra";
        onLoginRequest.Discriminator = "0000";
        onLoginRequest.Token = "TOKEN";

        SendClient(receivingHostId, connectionId, onLoginRequest);
    }

    
    public void SendClient(int receivingHost, int connectionID, NetMessage message)
    {
        byte[] buffer = new byte[BYTE_SIZE];
        
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        MemoryStream memoryStream = new MemoryStream(buffer);
        binaryFormatter.Serialize(memoryStream, message);
    
        if (receivingHost == 0)
        {
            NetworkTransport.Send(hostID, connectionID, reliableChannel, buffer, BYTE_SIZE, out error);
        }
        else
        {
            NetworkTransport.Send(webHostID, connectionID, reliableChannel, buffer, BYTE_SIZE, out error);
        }      
    }
}
