[System.Serializable]
public class Net_OnLoginRequest: NetMessage
{
    public Net_OnLoginRequest()
    {
        OperationCode = NetOperationCode.OnLoginRequest;
    }
    
    public byte Success { set; get; }
    public string Information { set; get; }
    
    public int ConnectionID { set; get; }
    public string Username { set; get; }
    public string Discriminator { set; get; }
    public string Token { set; get; }
}
