[System.Serializable]
public class Net_CreateAccount : NetMessage
{
    public Net_CreateAccount()
    {
        OperationCode = NetOperationCode.CreateAccount;
    }
    
    public string Username { set; get; }
    public string Password { set; get; }
    public string Email { set; get; }
}
