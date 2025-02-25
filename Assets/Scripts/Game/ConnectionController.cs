using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionController : MonoBehaviour
{
    public TMPro.TMP_InputField ipInput;
    public TMPro.TMP_InputField portInput;

    public void Connect()
    {
        string ip = ipInput.text;
        string port = portInput.text;
        GameController.connection = new GameClient(ip, port);
        SceneManager.LoadScene("Game");
    }
    public void CreateServer() {

        string port = portInput.text;
        GameController.connection = new GameServer(port);
        SceneManager.LoadScene("Game");
    }
}
