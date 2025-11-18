using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text messageText;
    public Button loginButton;

    private string apiUrl = "http://localhost:5000/api/Auth/login"; // replace with your backend endpoint

    void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
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

    IEnumerator LoginRoutine(string username, string password)
    {
        messageText.text = "Logging in...";

        string jsonBody = JsonUtility.ToJson(new LoginRequest(username, password));

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
            if (!string.IsNullOrEmpty(response.token))
            {
                messageText.text = "Login successful!";

                // Store token
                PlayerPrefs.SetString("AuthToken", response.token);
                PlayerPrefs.Save();

                // Load the next scene
                UnityEngine.SceneManagement.SceneManager.LoadScene("SimulationScene");
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
        public string username;
        public string password;

        public LoginRequest(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
    }

    [System.Serializable]    
    public class LoginResponse
    {
        public int id;
        public string firstName;
        public string lastName;
        public string permission;
        public string token;
    }

}