using System.Collections.Generic;
using Unity.Netcode;
using TMPro;
using UnityEngine;

public class GameUI : NetworkBehaviour
{
    [SerializeField] TMP_Text scoreboardText;
    [SerializeField] TMP_Text winnerText;
    [SerializeField] TMP_Text countdownText;

    Dictionary<ulong, int> localScores = new();

    [ClientRpc]
    public void UpdateScoreboardClientRpc(ulong playerId, int score)
    {
        localScores[playerId] = score;
        RedrawScoreboard();
    }

    void RedrawScoreboard()
    {
        scoreboardText.text = "";
        foreach (var kv in localScores)
        {
            scoreboardText.text += $"Player {kv.Key}: +{kv.Value} point";
        }
    }

    [ClientRpc]
    public void ClearScoresClientRpc()
    {
        localScores.Clear();
        scoreboardText.text = "";
        winnerText.text = "";
        countdownText.text = "";
    }

    [ClientRpc]
    public void ShowWinnerClientRpc(ulong winnerId)
    {
        winnerText.text = $"Player {winnerId} WON!";
    }

    [ClientRpc]
    public void ShowCountdownClientRpc(string text)
    {
        if (countdownText != null)
            countdownText.text = text;
    }
}
