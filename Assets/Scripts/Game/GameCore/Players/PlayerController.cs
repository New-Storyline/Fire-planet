using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class PlayerController
    {
        private List<Player> players;
        private int currTurn = 0;
        private Game game;

        public PlayerController(int playersCount, Game game)
        {
            if (playersCount > GameConfig.MAX_PLAYERS)
                throw new System.ArgumentException($"playersCount can't be greater than {GameConfig.MAX_PLAYERS}");

            PlayersCount = playersCount;

            players = new List<Player>();
            for (int i = 0; i < playersCount; i++)
            {
                players.Add(new Player(((Player.TeamColor)i)));
            }

            this.game = game;
        }

        public Player GetCurrentPlayer()
        {
            return players[currTurn];
        }
        public Player GetMainPlayer() { return players[0]; }

        /// <summary>
        /// Distributes villages among players one by one
        /// </summary>
        /// <param name="villages"></param>
        /// <exception cref="System.ArgumentException"></exception>
        internal void DistributeVillages(List<Village> villages)
        {

            if (villages.Count < players.Count)
                throw new System.ArgumentException($"Can't distribute villages! Villages count can't be less than players count. villages -> {villages.Count}. players -> {players.Count}");

            // Shuffle the villages
            for (int i = villages.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (villages[i], villages[j]) = (villages[j], villages[i]);
            }

            for (int i = 0; i < players.Count; i++)
            {
                game.CaputeSettlement(players[i], villages[i].GetPosition());
            }
        }

        /// <summary>
        /// Distributes start unti/units among players on game beginnig
        /// </summary>
        internal void DistributeStartUnits()
        {

            foreach (Player player in players)
            {
                Vector2Int pos = player.GetCity(0).GetPosition();

                game.SpawnUnit(new Rifleman(pos, player));
            }

        }

        internal void NextTurn()
        {
            currTurn++;

            if (currTurn >= players.Count)
                currTurn = 0;

            GetCurrentPlayer().NextTurn();
        }

        internal int PlayersCount { get; }
    }

}
