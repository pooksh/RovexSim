using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField usernameInput; // username 0
    public TMP_InputField passwordInput; // password 1
    public TMP_Text messageText; // login message
    public Button loginButton; 
    public int currentInput = 0;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift)) // detect if shift tab is pressed, and go to previous input field
        {
            currentInput--;
            if (currentInput < 0) currentInput = 1;
            SelectInputField();
        }

        if (Input.GetKeyDown(KeyCode.Tab)) // detect is tab is pressed and go to next input field
        {
            currentInput++;
            if (currentInput > 1) currentInput = 0;
            SelectInputField();
        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) // detect if return/enter is pressed to submit input after validation
        {

            if (usernameInput.text != "" && passwordInput.text != "") // validate that user and password are filled out before accepting submit
            {
                OnLoginClicked();
            }               
            
        }

        void SelectInputField()
        {
            switch (currentInput)
            {
                case 0:
                    usernameInput.Select();
                    break;
                case 1:
                    passwordInput.Select();
                    break;                
            }
        }
    }

    public void UsernameSelected() => currentInput = 0; // username = 0   
    public void PasswordSelected() => currentInput = 1; // password = 1
    

    private string apiUrl = "http://localhost:5000/api/Auth/login"; // backend API URL

    void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked); // login button action
        usernameInput.Select();
    }

    void OnLoginClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            messageText.text = "Please enter username and password.";
            return;
        }

        StartCoroutine(LoginRoutine(username, password));
    }

    IEnumerator LoginRoutine(string username, string password) // send API POST to /api/auth/login
    {
        messageText.text = "Logging in...";

        string jsonBody = JsonUtility.ToJson(new LoginRequest(username, password));

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"); 
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) // if we get a successful response, store response info
        {
            var response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
            if (!string.IsNullOrEmpty(response.token))
            {
                messageText.text = "Login successful!";

                // Store name, access token, and permission level
                PlayerPrefs.SetString("FirstName", response.firstName);
                PlayerPrefs.SetString("LastName", response.lastName);
                PlayerPrefs.SetString("AuthToken", response.token);
                PlayerPrefs.SetString("Permission", response.permission);
                PlayerPrefs.Save();

                // Load the next scene
                UnityEngine.SceneManagement.SceneManager.LoadScene("OptionsScene");
            }
            else
            {
                messageText.text = "Invalid credentials.";
            }
        }
        else
        {
            messageText.text = $"Error: {request.error}";
        }
    }


    [System.Serializable]
    public class LoginRequest
    {
        public string username; // username
        public string password; // password will be encrypted by the backend and stored in the DB

        public LoginRequest(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
    }

    [System.Serializable]    
    public class LoginResponse
    {
        public int id; // returns ID in DB
        public string firstName; // returns first name of user
        public string lastName; // returns last name of user
        public string permission; // use for rights delegation
        public string token; // use for subsequent API calls 
    }

}