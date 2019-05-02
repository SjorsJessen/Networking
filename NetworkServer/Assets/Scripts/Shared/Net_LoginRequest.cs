[System.Serializable]
public class Net_LoginRequest : NetMessage
{
    public Net_LoginRequest()
    {
        OperationCode = NetOperationCode.LoginRequest;
    }
    
    public string UsernameOrEmail { set; get; }
    public string Password { set; get; }
}
