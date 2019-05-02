using TMPro;
using UnityEngine;

public class LobbyScene : MonoBehaviour
{
    public static LobbyScene Instance { private set; get; }
    [SerializeField] private CanvasGroup _canvasGroup;
    
    #region CreateAccountOrLoginInputFields
    [SerializeField] private TextMeshProUGUI _welcomeMessageText;
    [SerializeField] private TextMeshProUGUI _authenticationMessageText;
    
    [SerializeField] private TMP_InputField _createAccountUsername;
    [SerializeField] private TMP_InputField _createAccountPassword;
    [SerializeField] private TMP_InputField _createAccountEmail;
    
    [SerializeField] private TMP_InputField _loginUsernameOrEmail;
    [SerializeField] private TMP_InputField _loginPassword;
     #endregion
 
    private void Start()
    {
        Instance = this;
    }

    public void OnClickCreateAccount()
    {
        DisableInputs();
        
        string username = _createAccountUsername.text;
        string password = _createAccountPassword.text;
        string email = _createAccountEmail.text;
        
        Client.Instance.SendCreateAccount(username, password, email);
    }

    public void OnClickLoginRequest()
    {
        DisableInputs();
        
        string usernameOrEmail = _loginUsernameOrEmail.text;
        string password = _loginPassword.text;
        
        Client.Instance.SendLoginRequest(usernameOrEmail, password);
    }

    public void ChangeWelcomeMessage(string message)
    {
        _welcomeMessageText.text = message;
    } 
    
    public void ChangeAuthenticationMessage(string message)
    {
        _authenticationMessageText.text = message;
    }

    public void EnableInputs()
    {
        _canvasGroup.interactable = true;
    }

    public void DisableInputs()
    {
        _canvasGroup.interactable = false;
    }
}


