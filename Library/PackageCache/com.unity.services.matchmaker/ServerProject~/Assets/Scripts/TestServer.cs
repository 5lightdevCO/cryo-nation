using UnityEngine;
using Testing;
using UnityEditor;

/// <summary>
/// Simple monobehaviour to act as a game server with a messaging agent for testing purposes.
/// </summary>
public class TestServer : MonoBehaviour
{
    const string quitMessage = "!q";

    SimpleTcpListener messageListener;
    TestAgent agent;
    bool endGameTriggered = false;
    bool initialized = false;

    public void Init(string ip, uint messagingPort)
    {
        //Setting up a TCP server which listens for connections w/ test agent
        messageListener = new SimpleTcpListener(ip, (int)messagingPort);
        agent = new TestAgent();
        agent.OnQuitMessageRecieved += OnRecievedQuitRequest;
        initialized = true;
    }

    /// <summary>
    /// Starts the game, opening the configured query and game ports.
    /// </summary>
    public void StartAgent()
    {
        if (!initialized) 
        {
            Debug.LogError($"[{nameof(TestServer)}]: Call Init() before starting agent!");
            return;
        }

        Debug.Log($"[{nameof(TestServer)}]: Server started agent with listener..");
        messageListener.Start();
        agent.RegisterMessenger(messageListener);
    }

    void Update()
    {
        //Workaround for exit play mode not being supported in callbacks.
        if (endGameTriggered) 
        {
#if UNITY_EDITOR
            //Application.Quit() does not work in the editor so
            //UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    void OnDestroy()
    {
        agent.UnregisterMessenger(messageListener);
        agent.OnQuitMessageRecieved -= OnRecievedQuitRequest;
        messageListener.CloseAllConnections();
    }

    void OnRecievedQuitRequest()
    {
        endGameTriggered = true;
    }
}

