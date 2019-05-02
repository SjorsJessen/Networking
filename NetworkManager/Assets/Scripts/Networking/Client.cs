using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class Client : MonoBehaviour
{
    
    public static Client Instance { private set; get; }
       
    private byte reliableChannel;
    private byte error;
    
    private const int MAX_USERS = 100;
    private const int PORT = 26000;
    private const int BYTE_SIZE = 1024;
    
    private const string SERVER_IP = "127.0.0.1";

    private int hostID;
    private int connectionId;
    
    private bool isStarted;
    
    private void Start()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Init();
    }

    public void Init()
    {
        NetworkTransport.Init();
        SetupNetwork();
    }



    private void Update()
    {
        UpdateMessagePump();
    }

    private void SetupNetwork()
    {
        ConnectionConfig connectionConfig = new ConnectionConfig();
        connectionConfig.AddChannel(QosType.Reliable);
        HostTopology hostTopology = new HostTopology(connectionConfig, MAX_USERS);

        hostID = NetworkTransport.AddHost(hostTopology, 0);
        connectionId = NetworkTransport.Connect(hostID, SERVER_IP, PORT, 0, out error);

        Debug.Log(string.Format("Attempting to connect on {0}...", SERVER_IP));
        isStarted = true;
    }
    
    public void ShutdownNetwork()
    {
        NetworkTransport.Shutdown();
        isStarted = false;
    }
    
    public void UpdateMessagePump()
    {
        if (!isStarted) return;

        int recHostId; //Is this from Web? or Standalone?
        int connectionId; //Which user is sending me this?
        int channelId; //Which lane is he sending that message from?
        
        byte[] receivingBuffer = new byte[BYTE_SIZE];
        int dataSize;

        NetworkEventType networkEventType = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, receivingBuffer, receivingBuffer.Length,
            out dataSize, out error);

        switch (networkEventType)
        {
            case NetworkEventType.Nothing:
                break;

            case NetworkEventType.ConnectEvent:
                Debug.Log("We have connected to the server!");
                break;
            
            case NetworkEventType.DisconnectEvent:
                Debug.Log("We have been disconnected!");
                break;
            
            case NetworkEventType.DataEvent:
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                MemoryStream memoryStream = new MemoryStream(receivingBuffer);
                NetMessage message = (NetMessage)binaryFormatter.Deserialize(memoryStream);

                OnData(connectionId, channelId, recHostId, message);
                break;
            
            default:
            case NetworkEventType.BroadcastEvent:
                Debug.Log("Unexpected Network Event Type");
                break;
        }
    }

    private void OnData(int connectionID, int channelID, int receivingHostID, NetMessage message)
    {
        switch (message.OperationCode)
        {
            case NetOperationCode.None:
                Debug.LogError("Unexpected NetOperationCode!");
                break;
            case NetOperationCode.OnCreateAccount:
                OnCreateAccount((Net_OnCreateAccount)message);
                break;
            case NetOperationCode.OnLoginRequest:
                OnLoginRequest((Net_OnLoginRequest)message);
                break;
        }
    }

    private void OnCreateAccount(Net_OnCreateAccount onCreateAccount)
    {
        LobbyScene.Instance.EnableInputs();
        LobbyScene.Instance.ChangeAuthenticationMessage(onCreateAccount.Information);      
    }
    
    private void OnLoginRequest(Net_OnLoginRequest onLoginRequest)
    {
        LobbyScene.Instance.ChangeAuthenticationMessage(onLoginRequest.Information);
        
        if (onLoginRequest.Success != 0)
        {
            //Unable to Login!
            LobbyScene.Instance.EnableInputs();
        }
        else
        {
            //Successful Login!
        }
    }
    
    public void SendToServer(NetMessage message)
    {
        byte[] buffer = new byte[BYTE_SIZE];
        
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        MemoryStream memoryStream = new MemoryStream(buffer);
        binaryFormatter.Serialize(memoryStream, message);
        
        NetworkTransport.Send(hostID, connectionId, reliableChannel, buffer, BYTE_SIZE, out error);       
    }

    public void SendCreateAccount(string username, string password, string email)
    {
        Net_CreateAccount netCreateAccount = new Net_CreateAccount();
        netCreateAccount.Username = username;
        netCreateAccount.Password = password;
        netCreateAccount.Email = email;
        
        SendToServer(netCreateAccount);
    }

    public void SendLoginRequest(string usernameOrEmail, string password)
    {
        Net_LoginRequest netLoginRequest = new Net_LoginRequest();
        netLoginRequest.UsernameOrEmail = usernameOrEmail;
        netLoginRequest.Password = password;
        
        SendToServer(netLoginRequest);
    }
}
