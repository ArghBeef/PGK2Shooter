using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Automatically switches the host to the Gameplay scene as soon as the
/// NetworkManager reports "server started".  Clients follow via NGO's
/// built-in scene sync (requires Enable Scene Management).
/// </summary>
public class AutoSceneLoader : MonoBehaviour
{
    [SerializeField] string gameplayScene = "Game";

    void Awake()
    {
        // Fires when the widget calls StartHost()
        NetworkManager.Singleton.OnServerStarted += ChangeScene;
    }

    void ChangeScene()
    {
        NetworkManager.Singleton.SceneManager
                       .LoadScene(gameplayScene, LoadSceneMode.Single);
        // Unsubscribe so it doesn't trigger twice in play-mode reloads
        NetworkManager.Singleton.OnServerStarted -= ChangeScene;
    }
}
