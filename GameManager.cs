using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public NetworkVariable<bool> roundInProgress = new(false);
    public Dictionary<ulong, int> scores = new();

    [SerializeField] int winningScore = 5;
    [SerializeField] GameUI gameUI;

    void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            StartCoroutine(StartRound());
    }

    IEnumerator StartRound()
    {
        roundInProgress.Value = true;
        gameUI.ClearScoresClientRpc();

        for (int i = 3; i >= 1; i--)
        {
            gameUI.ShowCountdownClientRpc($"Round begins in: {i}");
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(1f);
        gameUI.ShowCountdownClientRpc("");

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            ulong id = client.ClientId;
            var player = client.PlayerObject.GetComponent<Player>();

            player.Freeze(true);
            yield return null;

            int index = (int)id + 1;
            var spawn = SpawnManager.Instance.GetSpawnPointForPlayer(index);
            if (spawn != null)
            {
                player.TeleportClientRpc(spawn.position, spawn.rotation);
            }

            yield return null;

            player.GetComponent<Health>().ResetHealth();
            player.Freeze(false);
        }

        yield return null;
    }

    public void OnPlayerEliminated(ulong eliminatedId)
    {
        if (!IsServer) return;

        int aliveCount = 0;
        ulong? lastAlive = null;

        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            var hp = client.Value.PlayerObject.GetComponent<Health>();
            if (hp.current.Value > 0)
            {
                aliveCount++;
                lastAlive = client.Key;
            }
        }

        if (aliveCount <= 1 && lastAlive.HasValue)
        {
            roundInProgress.Value = false;

            if (!scores.ContainsKey(lastAlive.Value))
                scores[lastAlive.Value] = 0;

            scores[lastAlive.Value]++;
            gameUI.UpdateScoreboardClientRpc(lastAlive.Value, scores[lastAlive.Value]);

            if (scores[lastAlive.Value] >= winningScore)
            {
                gameUI.ShowWinnerClientRpc(lastAlive.Value);
                StartCoroutine(ReturnToMainMenu());
            }
            else
            {
                StartCoroutine(RestartRoundAfterDelay());
            }
        }
    }

    IEnumerator RestartRoundAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        StartCoroutine(StartRound());
    }

    IEnumerator ReturnToMainMenu()
    {
        yield return new WaitForSeconds(5f);
        NetworkManager.Singleton.SceneManager.LoadScene("MenuMain", LoadSceneMode.Single);
    }
}
