[System.Serializable]
public class Net_OnCreateAccount: NetMessage
{
    public Net_OnCreateAccount()
    {
        OperationCode = NetOperationCode.OnCreateAccount;
    }
    
    public byte Success { set; get; }
    public string Information { set; get; }
    
}
