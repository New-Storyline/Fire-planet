using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController
{
    private List<Player> players;
    private int currTurn = 0;
    
    public delegate void CaputeSettlement(Player player,Vector2Int pos);
    public delegate void SpawnUnit(Unit unit);

    private CaputeSettlement caputeSettlement_del;
    private SpawnUnit spawnUnit_del;

    public PlayerController(int playersCount, CaputeSettlement caputeSettlement_del, SpawnUnit spawnUnit_del)
    {
        if (playersCount > GameConfig.MAX_PLAYERS)
            throw new System.ArgumentException($"playersCount can't be greater than {GameConfig.MAX_PLAYERS}");

        PlayersCount = playersCount;

        players = new List<Player>();
        for (int i = 0; i < playersCount; i++)
        {
            players.Add(new Player(((Player.TeamColor)i)));
        }

        this.caputeSettlement_del = caputeSettlement_del;
        this.spawnUnit_del = spawnUnit_del;
    }

    public Player GetCurrentPlayer()
    {
        return players[currTurn];
    }

    /// <summary>
    /// Distributes villages among players one by one
    /// </summary>
    /// <param name="villages"></param>
    /// <exception cref="System.ArgumentException"></exception>
    public void DistributeVillages(List<Village> villages) {

        if(villages.Count < players.Count)
            throw new System.ArgumentException($"Can't distribute villages! Villages count can't be less than players count. villages -> {villages.Count}. players -> {players.Count}");

        // Shuffle the villages
        for (int i = villages.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (villages[i], villages[j]) = (villages[j], villages[i]); 
        }

        for (int i = 0; i < players.Count;i++)
        {
            caputeSettlement_del?.Invoke(players[i], villages[i].GetPosition());
        }
    }

    /// <summary>
    /// Distributes start unti/units among players on game beginnig
    /// </summary>
    public void DistributeStartUnits() {

        foreach (Player player in players)
        {
            Vector2Int pos = player.GetCity(0).GetPosition();

            spawnUnit_del?.Invoke(new Rifleman(pos, player));
        }

    }

    public void NextTurn()
    {
        currTurn++;

        GetCurrentPlayer().ActivateUnits();

        if (currTurn >= players.Count)
            currTurn = 0;
    }

    public int PlayersCount { get; }
}
